using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MiniProject.Migrations;
using MiniProject.Migrations.Entities;

namespace MiniProject.ComplexLogicInMiddle;

public interface ITalentInsightsService
{
    Task<TalentMarketFacets> GetFacetsAsync(CancellationToken cancellationToken);

    Task<TalentMarketPage> GetTalentMarketAsync(TalentMarketFilter filter, CancellationToken cancellationToken);
}

public sealed record TalentMarketFilter(
    string? Query,
    string? Category,
    string? Location,
    EmploymentType? EmploymentType,
    WorkMode? WorkMode,
    int Page,
    int PageSize);

public sealed record TalentMarketFacets(
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> Locations,
    IReadOnlyList<string> EmploymentTypes,
    IReadOnlyList<string> WorkModes,
    QueryDiagnostics Diagnostics);

public sealed record TalentMarketPage(
    int Page,
    int PageSize,
    int TotalSegments,
    IReadOnlyList<TalentMarketSegment> Segments,
    QueryDiagnostics Diagnostics);

public sealed record TalentMarketSegment(
    string Category,
    string Skill,
    string Location,
    string Company,
    string JobTitle,
    string EmploymentType,
    string WorkMode,
    string SegmentLabel,
    int CandidateCount,
    IReadOnlyList<TalentMarketCandidate> SampleCandidates);

public sealed record TalentMarketCandidate(int ProfileId, string DisplayName, string AvatarUrl);

public sealed record QueryDiagnostics(long ElapsedMilliseconds, int DatabaseRoundTrips);

/// <summary>
/// "Talent market explorer": which skill / location / company / contract-type / work-mode segments
/// exist and how many candidates sit in each. It is written the way such features often end up in real
/// projects and is intentionally expensive on SQL Server:
///  - fan-out joins (ProfileSkills x Skills x Profiles x Experiences) before de-duplication;
///  - TRIM/UPPER/COALESCE/concatenation on seven columns (incl. job title) inside SELECT DISTINCT, so no
///    index can supply order or uniqueness and SQL Server must hash/sort the whole intermediate set;
///  - "most candidates first" ordering: a correlated COUNT(DISTINCT) subquery evaluated for EVERY distinct
///    segment (not just the page), re-joining the fan-out with trimmed, non-sargable equality predicates;
///  - accent-insensitive "search everything" (COLLATE ..._CI_AI over a concatenated haystack + LIKE '%x%');
///  - a separate DISTINCT COUNT over the same pipeline for paging;
///  - an N+1 loop that re-runs the joined, trimmed predicate for every row on the page.
/// </summary>
public sealed class TalentInsightsService : ITalentInsightsService
{
    private const int MaxPageSize = 50;
    private const int SampleCandidateCount = 3;
    private const string AccentInsensitiveCollation = "Latin1_General_100_CI_AI";

    private readonly MiniDbContext _dbContext;

    public TalentInsightsService(MiniDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TalentMarketFacets> GetFacetsAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var categories = await _dbContext.Skills
            .AsNoTracking()
            .Select(skill => skill.Category.Trim())
            .Distinct()
            .OrderBy(category => category)
            .ToListAsync(cancellationToken);

        var locations = await (
                from profileSkill in _dbContext.ProfileSkills
                join profile in _dbContext.Profiles on profileSkill.ProfileId equals profile.Id
                select (profile.Location ?? string.Empty).Trim())
            .Where(location => location != string.Empty)
            .Distinct()
            .OrderBy(location => location)
            .ToListAsync(cancellationToken);

        return new TalentMarketFacets(
            categories,
            locations,
            Enum.GetNames<EmploymentType>(),
            Enum.GetNames<WorkMode>(),
            new QueryDiagnostics(stopwatch.ElapsedMilliseconds, 2));
    }

    public async Task<TalentMarketPage> GetTalentMarketAsync(
        TalentMarketFilter filter,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var roundTrips = 0;
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);

        var segments = BuildSegmentQuery(filter).Distinct();

        var totalSegments = await segments.CountAsync(cancellationToken);
        roundTrips++;

        var pageRows = await segments
            .Select(segment => new
            {
                Segment = segment,
                CandidateCount = _dbContext.ProfileSkills
                    .Where(profileSkill =>
                        profileSkill.Skill.Name.Trim() == segment.Skill &&
                        (profileSkill.Profile.Location ?? string.Empty).Trim() == segment.Location &&
                        profileSkill.Profile.Experiences.Any(experience =>
                            experience.Company.Trim() == segment.Company &&
                            experience.JobTitle.Trim() == segment.JobTitle &&
                            experience.EmploymentType == segment.EmploymentType &&
                            experience.WorkMode == segment.WorkMode))
                    .Select(profileSkill => profileSkill.ProfileId)
                    .Distinct()
                    .Count()
            })
            .OrderByDescending(row => row.CandidateCount)
            .ThenBy(row => row.Segment.Category)
            .ThenBy(row => row.Segment.Skill)
            .ThenBy(row => row.Segment.Location)
            .ThenBy(row => row.Segment.Company)
            .ThenBy(row => row.Segment.JobTitle)
            .ThenBy(row => row.Segment.EmploymentType)
            .ThenBy(row => row.Segment.WorkMode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        roundTrips++;

        var results = new List<TalentMarketSegment>(pageRows.Count);
        foreach (var pageRow in pageRows)
        {
            var row = pageRow.Segment;
            var samples = await CandidatesIn(row)
                .Select(profile => new { profile.Id, DisplayName = profile.DisplayName.Trim() })
                .Distinct()
                .OrderBy(profile => profile.DisplayName)
                .Take(SampleCandidateCount)
                .ToListAsync(cancellationToken);
            roundTrips++;

            results.Add(new TalentMarketSegment(
                row.Category,
                row.Skill,
                row.Location,
                row.Company,
                row.JobTitle,
                row.EmploymentType.ToString(),
                row.WorkMode.ToString(),
                row.Label,
                pageRow.CandidateCount,
                samples
                    .Select(sample => new TalentMarketCandidate(
                        sample.Id,
                        sample.DisplayName,
                        $"/api/portfolios/{sample.Id}/avatar"))
                    .ToList()));
        }

        return new TalentMarketPage(
            page,
            pageSize,
            totalSegments,
            results,
            new QueryDiagnostics(stopwatch.ElapsedMilliseconds, roundTrips));
    }

    private IQueryable<SegmentRow> BuildSegmentQuery(TalentMarketFilter filter)
    {
        var query =
            from profileSkill in _dbContext.ProfileSkills.AsNoTracking()
            join skill in _dbContext.Skills on profileSkill.SkillId equals skill.Id
            join profile in _dbContext.Profiles on profileSkill.ProfileId equals profile.Id
            join experience in _dbContext.Experiences on profile.Id equals experience.ProfileId
            select new SegmentRow
            {
                Category = skill.Category.Trim().ToUpper(),
                Skill = skill.Name.Trim(),
                Location = (profile.Location ?? string.Empty).Trim(),
                Company = experience.Company.Trim(),
                JobTitle = experience.JobTitle.Trim(),
                EmploymentType = experience.EmploymentType,
                WorkMode = experience.WorkMode,
                Label = skill.Category.Trim() + " / " + skill.Name.Trim() + " @ " + experience.Company.Trim()
            };

        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var term = filter.Query.Trim();
            query = query.Where(row => EF.Functions
                .Collate(
                    row.Skill + " " + row.Company + " " + row.JobTitle + " " + row.Location,
                    AccentInsensitiveCollation)
                .Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            var category = filter.Category.Trim().ToUpper();
            query = query.Where(row => row.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            var location = filter.Location.Trim();
            query = query.Where(row => row.Location == location);
        }

        if (filter.EmploymentType is { } employmentType)
        {
            query = query.Where(row => row.EmploymentType == employmentType);
        }

        if (filter.WorkMode is { } workMode)
        {
            query = query.Where(row => row.WorkMode == workMode);
        }

        return query;
    }

    private IQueryable<PortfolioProfile> CandidatesIn(SegmentRow row) =>
        from profileSkill in _dbContext.ProfileSkills.AsNoTracking()
        join skill in _dbContext.Skills on profileSkill.SkillId equals skill.Id
        join profile in _dbContext.Profiles on profileSkill.ProfileId equals profile.Id
        join experience in _dbContext.Experiences on profile.Id equals experience.ProfileId
        where skill.Name.Trim() == row.Skill &&
              (profile.Location ?? string.Empty).Trim() == row.Location &&
              experience.Company.Trim() == row.Company &&
              experience.JobTitle.Trim() == row.JobTitle &&
              experience.EmploymentType == row.EmploymentType &&
              experience.WorkMode == row.WorkMode
        select profile;

    private sealed class SegmentRow
    {
        public string Category { get; init; } = string.Empty;
        public string Skill { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public string Company { get; init; } = string.Empty;
        public string JobTitle { get; init; } = string.Empty;
        public EmploymentType EmploymentType { get; init; }
        public WorkMode WorkMode { get; init; }
        public string Label { get; init; } = string.Empty;
    }
}
