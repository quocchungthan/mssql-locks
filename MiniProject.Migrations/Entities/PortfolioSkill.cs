namespace MiniProject.Migrations.Entities;

public class PortfolioSkill
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public PortfolioProfile Profile { get; set; } = null!;
    public ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();
}
