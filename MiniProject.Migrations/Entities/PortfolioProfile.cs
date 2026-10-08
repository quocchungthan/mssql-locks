namespace MiniProject.Migrations.Entities;

public class PortfolioProfile
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? ContactEmail { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? ResumeUrl { get; set; }

    public ICollection<PortfolioProject> Projects { get; set; } = new List<PortfolioProject>();
    public ICollection<PortfolioExperience> Experiences { get; set; } = new List<PortfolioExperience>();
    public ICollection<PortfolioSkill> Skills { get; set; } = new List<PortfolioSkill>();
    public ICollection<PortfolioSocialLink> SocialLinks { get; set; } = new List<PortfolioSocialLink>();
}
