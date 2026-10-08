using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MiniProject.Migrations;

namespace MiniProject.ComplexLogicInMiddle;

public interface IPortfolioService
{
    Task<PortfolioDetail?> GetByProfileIdAsync(int profileId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PortfolioSearchResult>> SearchAsync(
        string? query,
        IReadOnlyCollection<string> types,
        CancellationToken cancellationToken);
    Task<ProjectDetail?> GetProjectBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<string?> GetAvatarSvgAsync(int profileId, CancellationToken cancellationToken);
}

public sealed record PortfolioDetail(
    int Id,
    string DisplayName,
    string Headline,
    string? Bio,
    string? Location,
    string? ContactEmail,
    string? ProfileImageUrl,
    string? ResumeUrl,
    IReadOnlyList<PortfolioProjectDetail> Projects,
    IReadOnlyList<PortfolioExperienceDetail> Experiences,
    IReadOnlyList<PortfolioSkillDetail> Skills,
    IReadOnlyList<PortfolioSocialLinkDetail> SocialLinks);

public sealed record PortfolioProjectDetail(
    int Id,
    string Title,
    string Slug,
    string Summary,
    string? Description,
    string? DemoUrl,
    string? SourceUrl,
    string? Role,
    string? Organization,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsFeatured,
    IReadOnlyList<PortfolioSkillDetail> Skills);

public sealed record PortfolioExperienceDetail(
    int Id,
    string Company,
    string JobTitle,
    string? Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrent);

public sealed record PortfolioSkillDetail(int Id, string Name, string Category);

public sealed record PortfolioSocialLinkDetail(int Id, string Label, string Url);

public sealed record PortfolioSearchResult(
    string Type,
    string Title,
    string Description,
    string Href,
    string? Context,
    string? ExternalUrl = null,
    string? AvatarUrl = null);

public sealed record ProjectProfileReference(int Id, string DisplayName, string? Role, string AvatarUrl);

public sealed record ProjectDetail(
    int Id,
    string Title,
    string Slug,
    string Summary,
    string? Description,
    string? DemoUrl,
    string? SourceUrl,
    IReadOnlyList<PortfolioSkillDetail> Skills,
    IReadOnlyList<ProjectProfileReference> Profiles);

public sealed class PortfolioService : IPortfolioService
{
    private const int MaxResultsPerType = 100;
    private readonly MiniDbContext _dbContext;

    public PortfolioService(MiniDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PortfolioDetail?> GetByProfileIdAsync(
        int profileId,
        CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Profiles
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Projects)
                .ThenInclude(item => item.Project)
                    .ThenInclude(item => item.ProjectSkills)
                        .ThenInclude(item => item.Skill)
            .Include(item => item.Experiences)
            .Include(item => item.Skills)
                .ThenInclude(item => item.Skill)
            .Include(item => item.SocialLinks)
            .SingleOrDefaultAsync(item => item.Id == profileId, cancellationToken);

        if (profile is null)
        {
            return null;
        }

        return new PortfolioDetail(
            profile.Id,
            profile.DisplayName,
            profile.Headline,
            profile.Bio,
            profile.Location,
            profile.ContactEmail,
            profile.ProfileImageUrl,
            profile.ResumeUrl,
            profile.Projects
                .OrderBy(item => item.DisplayOrder)
                .Select(item => new PortfolioProjectDetail(
                    item.Project.Id,
                    item.Project.Title,
                    item.Project.Slug,
                    item.Project.Summary,
                    item.Project.Description,
                    item.Project.DemoUrl,
                    item.Project.SourceUrl,
                    item.Role,
                    item.Organization,
                    item.StartDate,
                    item.EndDate,
                    item.IsFeatured,
                    item.Project.ProjectSkills
                        .OrderBy(skill => skill.Skill.Name)
                        .Select(skill => new PortfolioSkillDetail(
                            skill.Skill.Id,
                            skill.Skill.Name,
                            skill.Skill.Category))
                        .ToArray()))
                .ToArray(),
            profile.Experiences
                .OrderBy(item => item.DisplayOrder)
                .Select(item => new PortfolioExperienceDetail(
                    item.Id,
                    item.Company,
                    item.JobTitle,
                    item.Description,
                    item.StartDate,
                    item.EndDate,
                    item.IsCurrent))
                .ToArray(),
            profile.Skills
                .OrderBy(item => item.DisplayOrder)
                .Select(item => new PortfolioSkillDetail(
                    item.Skill.Id,
                    item.Skill.Name,
                    item.Skill.Category))
                .ToArray(),
            profile.SocialLinks
                .OrderBy(item => item.DisplayOrder)
                .Select(item => new PortfolioSocialLinkDetail(
                    item.Id,
                    item.Label,
                    item.Url))
                .ToArray());
    }

    public async Task<IReadOnlyList<PortfolioSearchResult>> SearchAsync(
        string? query,
        IReadOnlyCollection<string> types,
        CancellationToken cancellationToken)
    {
        query = query?.Trim() ?? string.Empty;
        var results = new List<PortfolioSearchResult>();

        if (types.Contains("profiles", StringComparer.OrdinalIgnoreCase))
        {
            var profiles = await _dbContext.Profiles
                .AsNoTracking()
                .Where(profile => query == string.Empty ||
                    profile.DisplayName.Contains(query) ||
                    profile.Headline.Contains(query) ||
                    (profile.Bio != null && profile.Bio.Contains(query)) ||
                    (profile.Location != null && profile.Location.Contains(query)))
                .OrderBy(profile => profile.DisplayName)
                .Take(MaxResultsPerType)
                .Select(profile => new PortfolioSearchResult(
                    "profile",
                    profile.DisplayName,
                    profile.Headline,
                    $"/profile/{profile.Id}",
                    profile.Location,
                    null,
                    $"/api/portfolios/{profile.Id}/avatar"))
                .ToListAsync(cancellationToken);
            results.AddRange(profiles);
        }

        if (types.Contains("projects", StringComparer.OrdinalIgnoreCase))
        {
            var projects = await _dbContext.Projects
                .AsNoTracking()
                .Where(project => query == string.Empty ||
                    project.Title.Contains(query) ||
                    project.Summary.Contains(query) ||
                    (project.Description != null && project.Description.Contains(query)))
                .OrderBy(project => project.Title)
                .Take(MaxResultsPerType)
                .Select(project => new PortfolioSearchResult(
                    "project",
                    project.Title,
                    project.Summary,
                    $"/project/{project.Slug}",
                    project.Profiles
                        .OrderBy(profileProject => profileProject.DisplayOrder)
                        .Select(profileProject => profileProject.Profile.DisplayName)
                        .FirstOrDefault()))
                .ToListAsync(cancellationToken);
            results.AddRange(projects);
        }

        if (types.Contains("skills", StringComparer.OrdinalIgnoreCase))
        {
            var skills = await _dbContext.Skills
                .AsNoTracking()
                .Where(skill => query == string.Empty ||
                    skill.Name.Contains(query) ||
                    skill.Category.Contains(query))
                .OrderBy(skill => skill.Name)
                .Take(MaxResultsPerType)
                .Select(skill => new PortfolioSearchResult(
                    "skill",
                    skill.Name,
                    skill.Category,
                    $"/search?q={Uri.EscapeDataString(skill.Name)}",
                    "Technology"))
                .ToListAsync(cancellationToken);
            results.AddRange(skills);
        }

        if (types.Contains("links", StringComparer.OrdinalIgnoreCase))
        {
            var links = await _dbContext.SocialLinks
                .AsNoTracking()
                .Where(link => query == string.Empty ||
                    link.Label.Contains(query) ||
                    link.Url.Contains(query) ||
                    link.Profile.DisplayName.Contains(query))
                .OrderBy(link => link.Label)
                .ThenBy(link => link.Profile.DisplayName)
                .Take(MaxResultsPerType)
                .Select(link => new PortfolioSearchResult(
                    "link",
                    link.Label,
                    link.Url,
                    $"/profile/{link.ProfileId}#contact",
                    link.Profile.DisplayName,
                    link.Url))
                .ToListAsync(cancellationToken);
            results.AddRange(links);
        }

        return results;
    }

    public async Task<ProjectDetail?> GetProjectBySlugAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(item => item.Slug == slug)
            .Select(item => new ProjectDetail(
                item.Id,
                item.Title,
                item.Slug,
                item.Summary,
                item.Description,
                item.DemoUrl,
                item.SourceUrl,
                item.ProjectSkills
                    .OrderBy(skill => skill.Skill.Name)
                    .Select(skill => new PortfolioSkillDetail(
                        skill.Skill.Id,
                        skill.Skill.Name,
                        skill.Skill.Category))
                    .ToArray(),
                item.Profiles
                    .OrderBy(profile => profile.DisplayOrder)
                    .Select(profile => new ProjectProfileReference(
                        profile.ProfileId,
                        profile.Profile.DisplayName,
                        profile.Role,
                        $"/api/portfolios/{profile.ProfileId}/avatar"))
                    .ToArray()))
            .SingleOrDefaultAsync(cancellationToken);

        return project;
    }

    public async Task<string?> GetAvatarSvgAsync(
        int profileId,
        CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Profiles
            .SingleOrDefaultAsync(item => item.Id == profileId, cancellationToken);

        if (profile is null)
        {
            return null;
        }

        if (profile.AvatarSvg is null)
        {
            profile.AvatarSvg = GeneratePixelAvatar(profile.Id, profile.DisplayName);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return profile.AvatarSvg;
    }

    private static string GeneratePixelAvatar(int profileId, string displayName)
    {
        var seed = SHA256.HashData(Encoding.UTF8.GetBytes($"{profileId}:{displayName}"));
        var background = new[] { "#202822", "#25271f", "#1c2828", "#29221f" }[seed[0] % 4];
        var foreground = new[] { "#acc8a3", "#d6af80", "#91c6be", "#d8a4a0" }[seed[1] % 4];
        var accent = new[] { "#789077", "#b78558", "#658e94", "#a87873" }[seed[2] % 4];
        const int pixelSize = 12;
        const int gridSize = 7;
        var pixels = new StringBuilder();
        var hashIndex = 3;

        for (var row = 0; row < gridSize; row++)
        {
            for (var column = 0; column < (gridSize + 1) / 2; column++)
            {
                var value = seed[hashIndex++];
                if (value % 3 == 0)
                {
                    continue;
                }

                var color = value % 5 == 0 ? accent : foreground;
                var x = column * pixelSize;
                var y = row * pixelSize;
                pixels.Append(CultureInfo.InvariantCulture,
                    $"<rect x=\"{x}\" y=\"{y}\" width=\"{pixelSize}\" height=\"{pixelSize}\" fill=\"{color}\"/>");

                if (column != gridSize / 2)
                {
                    var mirroredX = (gridSize - 1 - column) * pixelSize;
                    pixels.Append(CultureInfo.InvariantCulture,
                        $"<rect x=\"{mirroredX}\" y=\"{y}\" width=\"{pixelSize}\" height=\"{pixelSize}\" fill=\"{color}\"/>");
                }
            }
        }

        return string.Create(CultureInfo.InvariantCulture,
            $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {gridSize * pixelSize} {gridSize * pixelSize}\" role=\"img\" aria-label=\"Pixel avatar for {System.Net.WebUtility.HtmlEncode(displayName)}\"><rect width=\"100%\" height=\"100%\" rx=\"10\" fill=\"{background}\"/>{pixels}</svg>");
    }
}
