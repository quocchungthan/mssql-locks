namespace MiniProject.Migrations.Entities;

public class PortfolioProject
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Role { get; set; }
    public string? Organization { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? DemoUrl { get; set; }
    public string? SourceUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }

    public PortfolioProfile Profile { get; set; } = null!;
    public ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();
}
