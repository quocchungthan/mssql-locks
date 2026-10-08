using Microsoft.EntityFrameworkCore;
using MiniProject.Migrations.Entities;

namespace MiniProject.Migrations.SeedData;

public static class PortfolioSeedDataExtensions
{
    public static ModelBuilder HasPortfolioSeedData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PortfolioProfile>().HasData(
            new PortfolioProfile
            {
                Id = 1,
                DisplayName = "Alex Rivera",
                Headline = "Software Engineer | Data-rich web applications",
                Bio = "I build reliable web products that make complex workflows easier to understand. " +
                      "My work spans API design, data modeling, and thoughtful frontend experiences.",
                Location = "Seattle, WA",
                ContactEmail = "alex.rivera@example.com",
                ProfileImageUrl = "https://images.example.com/profiles/alex-rivera.jpg",
                ResumeUrl = "https://example.com/resumes/alex-rivera.pdf"
            },
            new PortfolioProfile
            {
                Id = 2,
                DisplayName = "Maya Chen",
                Headline = "Full-stack Developer | Cloud and product engineering",
                Bio = "I turn early product ideas into maintainable software, from the first database schema " +
                      "to the polished interface. I enjoy making developer tools and collaborative products.",
                Location = "Toronto, ON",
                ContactEmail = "maya.chen@example.com",
                ProfileImageUrl = "https://images.example.com/profiles/maya-chen.jpg",
                ResumeUrl = "https://example.com/resumes/maya-chen.pdf"
            });

        modelBuilder.Entity<PortfolioProject>().HasData(
            new PortfolioProject
            {
                Id = 101,
                Title = "Fieldnote",
                Slug = "fieldnote",
                Summary = "A searchable knowledge hub for teams working across customer projects.",
                Description = "Designed a workspace that brings meeting notes, decisions, and follow-up tasks " +
                              "together. Added full-text search, project timelines, and clear ownership so teams " +
                              "can recover context without digging through chat history.",
                DemoUrl = "https://fieldnote.example.com",
                SourceUrl = "https://github.com/example/fieldnote"
            },
            new PortfolioProject
            {
                Id = 102,
                Title = "Signal Board",
                Slug = "signal-board",
                Summary = "An operations dashboard that turns incoming events into actionable queues.",
                Description = "Built a role-aware dashboard with saved filters, live status updates, and an audit " +
                              "trail. Optimized the API and database queries to keep large work queues responsive."
            },
            new PortfolioProject
            {
                Id = 103,
                Title = "Open Data Explorer",
                Slug = "open-data-explorer",
                Summary = "A lightweight way to browse, compare, and export public datasets.",
                Description = "Created a responsive data catalog with reusable chart views, accessible controls, " +
                              "and shareable query URLs for researchers and community groups.",
                SourceUrl = "https://github.com/example/open-data-explorer"
            },
            new PortfolioProject
            {
                Id = 104,
                Title = "Studio Calendar",
                Slug = "studio-calendar",
                Summary = "A collaborative scheduling workspace for small creative studios.",
                Description = "Delivered shared availability, conflict detection, and approval flows with a " +
                              "mobile-first calendar. Added background reminders and clear permissions for " +
                              "members, guests, and administrators.",
                DemoUrl = "https://studio-calendar.example.com"
            },
            new PortfolioProject
            {
                Id = 105,
                Title = "Release Companion",
                Slug = "release-companion",
                Summary = "A deployment companion that makes release status visible to every team.",
                Description = "Integrated build events, deployment checks, and incident notes into one release " +
                              "timeline. Added retry-safe webhooks and notifications that link directly to the " +
                              "relevant change.",
                SourceUrl = "https://github.com/example/release-companion"
            },
            new PortfolioProject
            {
                Id = 106,
                Title = "Community Pantry Map",
                Slug = "community-pantry-map",
                Summary = "A volunteer-maintained map of food resources and opening hours.",
                Description = "Built an accessible map and contribution flow that lets local organizations keep " +
                              "resource details current. Focused on clear mobile navigation and simple moderation.",
                DemoUrl = "https://pantry-map.example.com"
            },
            new PortfolioProject
            {
                Id = 107,
                Title = "Pocket Planner",
                Slug = "pocket-planner",
                Summary = "A simple planning tool for turning a big goal into steady weekly progress.",
                Description = "Built a lightweight planner with reusable checklists, calendar views, and gentle " +
                              "progress summaries for individuals and small groups.",
                DemoUrl = "https://pocket-planner.example.com",
                SourceUrl = "https://github.com/example/pocket-planner"
            },
            new PortfolioProject
            {
                Id = 108,
                Title = "Trail Atlas",
                Slug = "trail-atlas",
                Summary = "A community guide to accessible trails, parks, and outdoor facilities.",
                Description = "Created a map-first directory with crowd-sourced updates, practical access notes, " +
                              "and downloadable trip details for visitors planning ahead.",
                DemoUrl = "https://trail-atlas.example.com"
            });

        modelBuilder.Entity<ProfileProject>().HasData(
            new ProfileProject { ProfileId = 1, ProjectId = 101, Role = "Lead Engineer", Organization = "Independent project", StartDate = new DateOnly(2024, 2, 1), EndDate = new DateOnly(2024, 8, 1), IsFeatured = true, DisplayOrder = 0 },
            new ProfileProject { ProfileId = 1, ProjectId = 102, Role = "Backend and UI Engineer", Organization = "Northstar Labs", StartDate = new DateOnly(2023, 3, 1), EndDate = new DateOnly(2023, 12, 1), IsFeatured = true, DisplayOrder = 1 },
            new ProfileProject { ProfileId = 1, ProjectId = 103, Role = "Full-stack Engineer", Organization = "Civic Works", StartDate = new DateOnly(2022, 4, 1), EndDate = new DateOnly(2022, 11, 1), IsFeatured = false, DisplayOrder = 2 },
            new ProfileProject { ProfileId = 2, ProjectId = 104, Role = "Product Engineer", Organization = "Brightside Studio", StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 10, 1), IsFeatured = true, DisplayOrder = 0 },
            new ProfileProject { ProfileId = 2, ProjectId = 105, Role = "Full-stack Developer", Organization = "Cloudline", StartDate = new DateOnly(2023, 2, 1), EndDate = new DateOnly(2023, 11, 1), IsFeatured = true, DisplayOrder = 1 },
            new ProfileProject { ProfileId = 2, ProjectId = 106, Role = "Volunteer Developer", Organization = "Neighbourhood Network", StartDate = new DateOnly(2022, 5, 1), EndDate = new DateOnly(2022, 12, 1), IsFeatured = false, DisplayOrder = 2 });

        modelBuilder.Entity<PortfolioExperience>().HasData(
            new PortfolioExperience
            {
                Id = 201,
                ProfileId = 1,
                Company = "Northstar Labs",
                JobTitle = "Software Engineer",
                Description = "Delivered customer-facing workflow tools, improved API observability, and worked " +
                              "with design and support to turn recurring customer issues into product improvements.",
                StartDate = new DateOnly(2022, 1, 1),
                EndDate = new DateOnly(2024, 1, 1),
                IsCurrent = false,
                DisplayOrder = 0
            },
            new PortfolioExperience
            {
                Id = 202,
                ProfileId = 1,
                Company = "Civic Works",
                JobTitle = "Software Developer",
                Description = "Built data visualization features and helped maintain open-source tools used by " +
                              "local research and community teams.",
                StartDate = new DateOnly(2020, 6, 1),
                EndDate = new DateOnly(2021, 12, 1),
                IsCurrent = false,
                DisplayOrder = 1
            },
            new PortfolioExperience
            {
                Id = 203,
                ProfileId = 2,
                Company = "Brightside Studio",
                JobTitle = "Full-stack Developer",
                Description = "Partnered with product designers to ship collaboration features, improve accessibility, " +
                              "and keep application performance predictable as usage grew.",
                StartDate = new DateOnly(2023, 7, 1),
                EndDate = null,
                IsCurrent = true,
                DisplayOrder = 0
            },
            new PortfolioExperience
            {
                Id = 204,
                ProfileId = 2,
                Company = "Cloudline",
                JobTitle = "Associate Developer",
                Description = "Developed internal release tools and integrations, with an emphasis on reliable " +
                              "background processing and useful operational feedback.",
                StartDate = new DateOnly(2021, 8, 1),
                EndDate = new DateOnly(2023, 6, 1),
                IsCurrent = false,
                DisplayOrder = 1
            });

        var skills = new[]
        {
            new PortfolioSkill { Id = 301, Name = "C#", Category = "Languages" },
            new PortfolioSkill { Id = 302, Name = "ASP.NET Core", Category = "Backend" },
            new PortfolioSkill { Id = 303, Name = "SQL Server", Category = "Data" },
            new PortfolioSkill { Id = 304, Name = "TypeScript", Category = "Languages" },
            new PortfolioSkill { Id = 305, Name = "React", Category = "Frontend" },
            new PortfolioSkill { Id = 306, Name = "Azure", Category = "Cloud" },
            new PortfolioSkill { Id = 307, Name = "Node.js", Category = "Backend" },
            new PortfolioSkill { Id = 308, Name = "PostgreSQL", Category = "Data" },
            new PortfolioSkill { Id = 309, Name = "Docker", Category = "Cloud" },
            new PortfolioSkill { Id = 310, Name = "Accessibility", Category = "Design" },
            new PortfolioSkill { Id = 311, Name = "JavaScript", Category = "Languages" },
            new PortfolioSkill { Id = 312, Name = "Vue", Category = "Frontend" },
            new PortfolioSkill { Id = 313, Name = "Redis", Category = "Data" },
            new PortfolioSkill { Id = 314, Name = "Angular", Category = "Frontend" },
            new PortfolioSkill { Id = 315, Name = "Python", Category = "Languages" },
            new PortfolioSkill { Id = 316, Name = "Django", Category = "Backend" },
            new PortfolioSkill { Id = 317, Name = "AWS", Category = "Cloud" },
            new PortfolioSkill { Id = 318, Name = "Kotlin", Category = "Languages" },
            new PortfolioSkill { Id = 319, Name = "React Native", Category = "Frontend" },
            new PortfolioSkill { Id = 320, Name = "Java", Category = "Backend" },
            new PortfolioSkill { Id = 321, Name = "MySQL", Category = "Data" },
            new PortfolioSkill { Id = 322, Name = "Go", Category = "Languages" },
            new PortfolioSkill { Id = 323, Name = "Svelte", Category = "Frontend" },
            new PortfolioSkill { Id = 324, Name = "MongoDB", Category = "Data" },
            new PortfolioSkill { Id = 325, Name = "Kubernetes", Category = "Cloud" }
        };
        modelBuilder.Entity<PortfolioSkill>().HasData(skills);

        modelBuilder.Entity<ProfileSkill>().HasData(
            new ProfileSkill { ProfileId = 1, SkillId = 301, DisplayOrder = 0 },
            new ProfileSkill { ProfileId = 1, SkillId = 302, DisplayOrder = 1 },
            new ProfileSkill { ProfileId = 1, SkillId = 303, DisplayOrder = 2 },
            new ProfileSkill { ProfileId = 1, SkillId = 304, DisplayOrder = 3 },
            new ProfileSkill { ProfileId = 1, SkillId = 305, DisplayOrder = 4 },
            new ProfileSkill { ProfileId = 1, SkillId = 306, DisplayOrder = 5 },
            new ProfileSkill { ProfileId = 2, SkillId = 304, DisplayOrder = 0 },
            new ProfileSkill { ProfileId = 2, SkillId = 305, DisplayOrder = 1 },
            new ProfileSkill { ProfileId = 2, SkillId = 307, DisplayOrder = 2 },
            new ProfileSkill { ProfileId = 2, SkillId = 308, DisplayOrder = 3 },
            new ProfileSkill { ProfileId = 2, SkillId = 309, DisplayOrder = 4 },
            new ProfileSkill { ProfileId = 2, SkillId = 310, DisplayOrder = 5 });

        modelBuilder.Entity<PortfolioSocialLink>().HasData(
            new PortfolioSocialLink { Id = 401, ProfileId = 1, Label = "GitHub", Url = "https://github.com/alex-rivera", DisplayOrder = 0 },
            new PortfolioSocialLink { Id = 402, ProfileId = 1, Label = "LinkedIn", Url = "https://www.linkedin.com/in/alex-rivera", DisplayOrder = 1 },
            new PortfolioSocialLink { Id = 403, ProfileId = 1, Label = "Website", Url = "https://alex-rivera.example.com", DisplayOrder = 2 },
            new PortfolioSocialLink { Id = 404, ProfileId = 2, Label = "GitHub", Url = "https://github.com/maya-chen", DisplayOrder = 0 },
            new PortfolioSocialLink { Id = 405, ProfileId = 2, Label = "LinkedIn", Url = "https://www.linkedin.com/in/maya-chen", DisplayOrder = 1 },
            new PortfolioSocialLink { Id = 406, ProfileId = 2, Label = "Website", Url = "https://maya-chen.example.com", DisplayOrder = 2 });

        modelBuilder.Entity<ProjectSkill>().HasData(
            new ProjectSkill { ProjectId = 101, SkillId = 304 },
            new ProjectSkill { ProjectId = 101, SkillId = 305 },
            new ProjectSkill { ProjectId = 101, SkillId = 303 },
            new ProjectSkill { ProjectId = 102, SkillId = 301 },
            new ProjectSkill { ProjectId = 102, SkillId = 302 },
            new ProjectSkill { ProjectId = 102, SkillId = 303 },
            new ProjectSkill { ProjectId = 102, SkillId = 304 },
            new ProjectSkill { ProjectId = 102, SkillId = 305 },
            new ProjectSkill { ProjectId = 103, SkillId = 304 },
            new ProjectSkill { ProjectId = 103, SkillId = 307 },
            new ProjectSkill { ProjectId = 103, SkillId = 308 },
            new ProjectSkill { ProjectId = 103, SkillId = 309 },
            new ProjectSkill { ProjectId = 104, SkillId = 304 },
            new ProjectSkill { ProjectId = 104, SkillId = 305 },
            new ProjectSkill { ProjectId = 104, SkillId = 310 },
            new ProjectSkill { ProjectId = 105, SkillId = 304 },
            new ProjectSkill { ProjectId = 105, SkillId = 305 },
            new ProjectSkill { ProjectId = 105, SkillId = 308 },
            new ProjectSkill { ProjectId = 105, SkillId = 310 },
            new ProjectSkill { ProjectId = 106, SkillId = 301 },
            new ProjectSkill { ProjectId = 106, SkillId = 302 },
            new ProjectSkill { ProjectId = 106, SkillId = 303 },
            new ProjectSkill { ProjectId = 106, SkillId = 306 },
            new ProjectSkill { ProjectId = 107, SkillId = 304 },
            new ProjectSkill { ProjectId = 107, SkillId = 305 },
            new ProjectSkill { ProjectId = 107, SkillId = 308 },
            new ProjectSkill { ProjectId = 108, SkillId = 304 },
            new ProjectSkill { ProjectId = 108, SkillId = 305 },
            new ProjectSkill { ProjectId = 108, SkillId = 303 },
            new ProjectSkill { ProjectId = 108, SkillId = 310 });

        return modelBuilder.HasAdditionalPortfolioSeedData();
    }
}
