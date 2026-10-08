using Microsoft.EntityFrameworkCore;
using MiniProject.Migrations.Entities;

namespace MiniProject.Migrations;

public class MiniDbContext : DbContext
{
    public MiniDbContext(DbContextOptions<MiniDbContext> options)
        : base(options)
    {
    }

    public DbSet<PortfolioProfile> Profiles => Set<PortfolioProfile>();
    public DbSet<PortfolioProject> Projects => Set<PortfolioProject>();
    public DbSet<PortfolioExperience> Experiences => Set<PortfolioExperience>();
    public DbSet<PortfolioSkill> Skills => Set<PortfolioSkill>();
    public DbSet<ProjectSkill> ProjectSkills => Set<ProjectSkill>();
    public DbSet<PortfolioSocialLink> SocialLinks => Set<PortfolioSocialLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PortfolioProfile>(entity =>
        {
            entity.ToTable("PortfolioProfiles");
            entity.HasKey(profile => profile.Id);
            entity.Property(profile => profile.DisplayName).HasMaxLength(160).IsRequired();
            entity.Property(profile => profile.Headline).HasMaxLength(240).IsRequired();
            entity.Property(profile => profile.Bio).HasMaxLength(4000);
            entity.Property(profile => profile.Location).HasMaxLength(160);
            entity.Property(profile => profile.ContactEmail).HasMaxLength(320);
            entity.Property(profile => profile.ProfileImageUrl).HasMaxLength(2048);
            entity.Property(profile => profile.ResumeUrl).HasMaxLength(2048);

            entity.HasMany(profile => profile.Projects)
                .WithOne(project => project.Profile)
                .HasForeignKey(project => project.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(profile => profile.Experiences)
                .WithOne(experience => experience.Profile)
                .HasForeignKey(experience => experience.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(profile => profile.Skills)
                .WithOne(skill => skill.Profile)
                .HasForeignKey(skill => skill.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(profile => profile.SocialLinks)
                .WithOne(link => link.Profile)
                .HasForeignKey(link => link.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PortfolioProject>(entity =>
        {
            entity.ToTable("PortfolioProjects", table =>
            {
                table.HasCheckConstraint(
                    "CK_PortfolioProjects_DateRange",
                    "[EndDate] IS NULL OR [StartDate] IS NULL OR [EndDate] >= [StartDate]");
                table.HasCheckConstraint("CK_PortfolioProjects_DisplayOrder", "[DisplayOrder] >= 0");
            });
            entity.HasKey(project => project.Id);
            entity.Property(project => project.Title).HasMaxLength(200).IsRequired();
            entity.Property(project => project.Slug).HasMaxLength(220).IsRequired();
            entity.Property(project => project.Summary).HasMaxLength(500).IsRequired();
            entity.Property(project => project.Description).HasMaxLength(8000);
            entity.Property(project => project.Role).HasMaxLength(160);
            entity.Property(project => project.Organization).HasMaxLength(200);
            entity.Property(project => project.DemoUrl).HasMaxLength(2048);
            entity.Property(project => project.SourceUrl).HasMaxLength(2048);
            entity.Property(project => project.StartDate).HasColumnType("date");
            entity.Property(project => project.EndDate).HasColumnType("date");
            entity.HasIndex(project => new { project.ProfileId, project.Slug }).IsUnique();
            entity.HasIndex(project => new { project.ProfileId, project.IsFeatured, project.DisplayOrder });

            entity.HasMany(project => project.ProjectSkills)
                .WithOne(projectSkill => projectSkill.Project)
                .HasForeignKey(projectSkill => projectSkill.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PortfolioExperience>(entity =>
        {
            entity.ToTable("PortfolioExperiences", table =>
            {
                table.HasCheckConstraint(
                    "CK_PortfolioExperiences_DateRange",
                    "[EndDate] IS NULL OR [EndDate] >= [StartDate]");
                table.HasCheckConstraint(
                    "CK_PortfolioExperiences_CurrentHasNoEndDate",
                    "[IsCurrent] = 0 OR [EndDate] IS NULL");
                table.HasCheckConstraint("CK_PortfolioExperiences_DisplayOrder", "[DisplayOrder] >= 0");
            });
            entity.HasKey(experience => experience.Id);
            entity.Property(experience => experience.Company).HasMaxLength(200).IsRequired();
            entity.Property(experience => experience.JobTitle).HasMaxLength(160).IsRequired();
            entity.Property(experience => experience.Description).HasMaxLength(4000);
            entity.Property(experience => experience.StartDate).HasColumnType("date").IsRequired();
            entity.Property(experience => experience.EndDate).HasColumnType("date");
            entity.HasIndex(experience => new { experience.ProfileId, experience.DisplayOrder });
        });

        modelBuilder.Entity<PortfolioSkill>(entity =>
        {
            entity.ToTable("PortfolioSkills", table =>
                table.HasCheckConstraint("CK_PortfolioSkills_DisplayOrder", "[DisplayOrder] >= 0"));
            entity.HasKey(skill => skill.Id);
            entity.Property(skill => skill.Name).HasMaxLength(100).IsRequired();
            entity.Property(skill => skill.Category).HasMaxLength(80).IsRequired();
            entity.HasIndex(skill => new { skill.ProfileId, skill.Name }).IsUnique();
            entity.HasIndex(skill => new { skill.ProfileId, skill.Category, skill.DisplayOrder });

            entity.HasMany(skill => skill.ProjectSkills)
                .WithOne(projectSkill => projectSkill.Skill)
                .HasForeignKey(projectSkill => projectSkill.SkillId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ProjectSkill>(entity =>
        {
            entity.ToTable("ProjectSkills");
            entity.HasKey(projectSkill => new { projectSkill.ProjectId, projectSkill.SkillId });
        });

        modelBuilder.Entity<PortfolioSocialLink>(entity =>
        {
            entity.ToTable("PortfolioSocialLinks", table =>
                table.HasCheckConstraint("CK_PortfolioSocialLinks_DisplayOrder", "[DisplayOrder] >= 0"));
            entity.HasKey(link => link.Id);
            entity.Property(link => link.Label).HasMaxLength(80).IsRequired();
            entity.Property(link => link.Url).HasMaxLength(2048).IsRequired();
            entity.HasIndex(link => new { link.ProfileId, link.Label }).IsUnique();
            entity.HasIndex(link => new { link.ProfileId, link.DisplayOrder });
        });
    }
}
