using Microsoft.EntityFrameworkCore;
using MiniProject.Migrations.Entities;
using MiniProject.Migrations.SeedData;

namespace MiniProject.Migrations;

public class MiniDbContext : DbContext
{
    public MiniDbContext(DbContextOptions<MiniDbContext> options)
        : base(options)
    {
    }

    public DbSet<PortfolioProfile> Profiles => Set<PortfolioProfile>();
    public DbSet<PortfolioProject> Projects => Set<PortfolioProject>();
    public DbSet<ProfileProject> ProfileProjects => Set<ProfileProject>();
    public DbSet<PortfolioExperience> Experiences => Set<PortfolioExperience>();
    public DbSet<PortfolioSkill> Skills => Set<PortfolioSkill>();
    public DbSet<ProfileSkill> ProfileSkills => Set<ProfileSkill>();
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
            entity.ToTable("PortfolioProjects");
            entity.HasKey(project => project.Id);
            entity.Property(project => project.Title).HasMaxLength(200).IsRequired();
            entity.Property(project => project.Slug).HasMaxLength(220).IsRequired();
            entity.Property(project => project.Summary).HasMaxLength(500).IsRequired();
            entity.Property(project => project.Description).HasMaxLength(8000);
            entity.Property(project => project.DemoUrl).HasMaxLength(2048);
            entity.Property(project => project.SourceUrl).HasMaxLength(2048);
            entity.HasIndex(project => project.Slug).IsUnique();

            entity.HasMany(project => project.ProjectSkills)
                .WithOne(projectSkill => projectSkill.Project)
                .HasForeignKey(projectSkill => projectSkill.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfileProject>(entity =>
        {
            entity.ToTable("ProfileProjects", table =>
            {
                table.HasCheckConstraint(
                    "CK_ProfileProjects_DateRange",
                    "[EndDate] IS NULL OR [StartDate] IS NULL OR [EndDate] >= [StartDate]");
                table.HasCheckConstraint("CK_ProfileProjects_DisplayOrder", "[DisplayOrder] >= 0");
            });
            entity.HasKey(profileProject => new { profileProject.ProfileId, profileProject.ProjectId });
            entity.Property(profileProject => profileProject.Role).HasMaxLength(160);
            entity.Property(profileProject => profileProject.Organization).HasMaxLength(200);
            entity.Property(profileProject => profileProject.StartDate).HasColumnType("date");
            entity.Property(profileProject => profileProject.EndDate).HasColumnType("date");
            entity.HasIndex(profileProject => new
            {
                profileProject.ProfileId,
                profileProject.IsFeatured,
                profileProject.DisplayOrder
            });

            entity.HasOne(profileProject => profileProject.Project)
                .WithMany(project => project.Profiles)
                .HasForeignKey(profileProject => profileProject.ProjectId)
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
            entity.ToTable("PortfolioSkills");
            entity.HasKey(skill => skill.Id);
            entity.Property(skill => skill.Name).HasMaxLength(100).IsRequired();
            entity.Property(skill => skill.Category).HasMaxLength(80).IsRequired();
            entity.HasIndex(skill => skill.Name).IsUnique();

            entity.HasMany(skill => skill.Profiles)
                .WithOne(profileSkill => profileSkill.Skill)
                .HasForeignKey(profileSkill => profileSkill.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfileSkill>(entity =>
        {
            entity.ToTable("ProfileSkills", table =>
                table.HasCheckConstraint("CK_ProfileSkills_DisplayOrder", "[DisplayOrder] >= 0"));
            entity.HasKey(profileSkill => new { profileSkill.ProfileId, profileSkill.SkillId });
            entity.HasIndex(profileSkill => new { profileSkill.ProfileId, profileSkill.DisplayOrder });
        });

        modelBuilder.Entity<ProjectSkill>(entity =>
        {
            entity.ToTable("ProjectSkills");
            entity.HasKey(projectSkill => new { projectSkill.ProjectId, projectSkill.SkillId });
            entity.HasOne(projectSkill => projectSkill.Skill)
                .WithMany(skill => skill.ProjectSkills)
                .HasForeignKey(projectSkill => projectSkill.SkillId)
                .OnDelete(DeleteBehavior.NoAction);
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

        modelBuilder.HasPortfolioSeedData();
    }
}
