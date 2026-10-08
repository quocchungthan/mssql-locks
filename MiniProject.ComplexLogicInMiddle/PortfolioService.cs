using Microsoft.EntityFrameworkCore;
using MiniProject.Migrations;

namespace MiniProject.ComplexLogicInMiddle;

public interface IPortfolioService
{
    Task<PortfolioDetail?> GetByProfileIdAsync(int profileId, CancellationToken cancellationToken);
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

public sealed class PortfolioService : IPortfolioService
{
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
}
