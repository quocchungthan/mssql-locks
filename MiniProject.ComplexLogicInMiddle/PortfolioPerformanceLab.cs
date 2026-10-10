using Microsoft.EntityFrameworkCore;
using MiniProject.Migrations;
using MiniProject.Migrations.Entities;

namespace MiniProject.ComplexLogicInMiddle;

public sealed record SkillBadge(string Name, string Category);

public sealed record CandidateSkillSetup(
    int ProfileId,
    string DisplayName,
    SkillBadge? InitialSkill,
    SkillBadge? PreferredSkill);

public sealed record DirectoryConfiguration(
    int ProfileId,
    string DisplayName,
    IReadOnlyList<int> SkillIds,
    IReadOnlyList<int> ProjectIds,
    IReadOnlyList<int> ExperienceIds,
    IReadOnlyList<int> SocialLinkIds);

public sealed class PortfolioPerformanceLab(MiniDbContext dbContext)
{
    public async Task<IReadOnlyList<CandidateSkillSetup>> GetShortlistAsync(
        int count,
        CancellationToken cancellationToken)
    {
        if (count is < 1 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 1 and 200.");
        }

        var profiles = await dbContext.Profiles.AsNoTracking()
            .OrderBy(profile => profile.Id)
            .Take(count)
            .Select(profile => new { profile.Id, profile.DisplayName })
            .ToListAsync(cancellationToken);
        var result = new List<CandidateSkillSetup>(profiles.Count);

        foreach (var profile in profiles)
        {
            var initial = await ReadSkillAsync(profile.Id, preferred: false);
            var preferred = await ReadSkillAsync(profile.Id, preferred: true);
            result.Add(new CandidateSkillSetup(profile.Id, profile.DisplayName, initial, preferred));
        }

        return result;

        async Task<SkillBadge?> ReadSkillAsync(int profileId, bool preferred)
        {
            var assignments = dbContext.ProfileSkills.AsNoTracking()
                .Where(assignment => assignment.ProfileId == profileId);
            var ordered = preferred
                ? assignments.OrderByDescending(assignment => assignment.DisplayOrder)
                    .ThenByDescending(assignment => assignment.SkillId)
                : assignments.OrderBy(assignment => assignment.DisplayOrder)
                    .ThenBy(assignment => assignment.SkillId);
            var skillId = await ordered.Select(assignment => (int?)assignment.SkillId)
                .FirstOrDefaultAsync(cancellationToken);

            // Each individual setup lookup reloads the entire catalog before C# matching.
            var catalog = await dbContext.Skills.AsNoTracking().ToListAsync(cancellationToken);
            return skillId is null ? null : Badge(catalog.Single(skill => skill.Id == skillId.Value));
        }
    }

    public IReadOnlyList<SkillBadge> GetSkillBadges(
        string? category,
        CancellationToken cancellationToken)
    {
        var assignments = dbContext.ProfileSkills.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(category))
        {
            var selectedCategory = category.Trim();
            assignments = assignments.Where(assignment => assignment.Skill.Category.Trim() == selectedCategory);
        }

        cancellationToken.ThrowIfCancellationRequested();
        // Transfer every duplicate pair synchronously before two-field C# de-duplication.
        var rows = assignments
            .Select(assignment => new SkillBadge(assignment.Skill.Name.Trim(), assignment.Skill.Category.Trim()))
            .ToList();
        cancellationToken.ThrowIfCancellationRequested();

        return rows.Distinct()
            .OrderBy(badge => badge.Category, StringComparer.Ordinal)
            .ThenBy(badge => badge.Name, StringComparer.Ordinal)
            .ToArray();
    }

    public DirectoryConfiguration? GetDirectoryConfiguration(
        int profileId,
        CancellationToken cancellationToken)
    {
        if (profileId < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(profileId));
        }

        cancellationToken.ThrowIfCancellationRequested();
        // IEnumerable loses SQL composition: read all roots and included graphs synchronously.
        var allProfiles = DirectoryRows().ToList();
        cancellationToken.ThrowIfCancellationRequested();
        var profile = allProfiles.SingleOrDefault(candidate => candidate.Id == profileId);

        return profile is null ? null : new DirectoryConfiguration(
            profile.Id,
            profile.DisplayName,
            profile.Skills.Select(skill => skill.SkillId).Order().ToArray(),
            profile.Projects.Select(project => project.ProjectId).Order().ToArray(),
            profile.Experiences.Select(experience => experience.Id).Order().ToArray(),
            profile.SocialLinks.Select(link => link.Id).Order().ToArray());
    }

    private IEnumerable<PortfolioProfile> DirectoryRows() =>
        dbContext.Profiles.AsNoTracking()
            .Include(profile => profile.Skills).ThenInclude(assignment => assignment.Skill)
            .Include(profile => profile.Experiences)
            .Include(profile => profile.Projects).ThenInclude(assignment => assignment.Project)
                .ThenInclude(project => project.ProjectSkills).ThenInclude(assignment => assignment.Skill)
            .Include(profile => profile.SocialLinks)
            .AsSingleQuery();

    private static SkillBadge Badge(PortfolioSkill skill) => new(skill.Name, skill.Category);
}
