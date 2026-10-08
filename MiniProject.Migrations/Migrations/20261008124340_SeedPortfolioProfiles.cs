using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiniProject.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SeedPortfolioProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PortfolioProfiles",
                columns: new[] { "Id", "Bio", "ContactEmail", "DisplayName", "Headline", "Location", "ProfileImageUrl", "ResumeUrl" },
                values: new object[,]
                {
                    { 1, "I build reliable web products that make complex workflows easier to understand. My work spans API design, data modeling, and thoughtful frontend experiences.", "alex.rivera@example.com", "Alex Rivera", "Software Engineer | Data-rich web applications", "Seattle, WA", "https://images.example.com/profiles/alex-rivera.jpg", "https://example.com/resumes/alex-rivera.pdf" },
                    { 2, "I turn early product ideas into maintainable software, from the first database schema to the polished interface. I enjoy making developer tools and collaborative products.", "maya.chen@example.com", "Maya Chen", "Full-stack Developer | Cloud and product engineering", "Toronto, ON", "https://images.example.com/profiles/maya-chen.jpg", "https://example.com/resumes/maya-chen.pdf" }
                });

            migrationBuilder.InsertData(
                table: "PortfolioExperiences",
                columns: new[] { "Id", "Company", "Description", "DisplayOrder", "EndDate", "IsCurrent", "JobTitle", "ProfileId", "StartDate" },
                values: new object[,]
                {
                    { 201, "Northstar Labs", "Delivered customer-facing workflow tools, improved API observability, and worked with design and support to turn recurring customer issues into product improvements.", 0, new DateOnly(2024, 1, 1), false, "Software Engineer", 1, new DateOnly(2022, 1, 1) },
                    { 202, "Civic Works", "Built data visualization features and helped maintain open-source tools used by local research and community teams.", 1, new DateOnly(2021, 12, 1), false, "Software Developer", 1, new DateOnly(2020, 6, 1) },
                    { 203, "Brightside Studio", "Partnered with product designers to ship collaboration features, improve accessibility, and keep application performance predictable as usage grew.", 0, null, true, "Full-stack Developer", 2, new DateOnly(2023, 7, 1) },
                    { 204, "Cloudline", "Developed internal release tools and integrations, with an emphasis on reliable background processing and useful operational feedback.", 1, new DateOnly(2023, 6, 1), false, "Associate Developer", 2, new DateOnly(2021, 8, 1) }
                });

            migrationBuilder.InsertData(
                table: "PortfolioProjects",
                columns: new[] { "Id", "DemoUrl", "Description", "DisplayOrder", "EndDate", "IsFeatured", "Organization", "ProfileId", "Role", "Slug", "SourceUrl", "StartDate", "Summary", "Title" },
                values: new object[,]
                {
                    { 101, "https://fieldnote.example.com", "Designed a workspace that brings meeting notes, decisions, and follow-up tasks together. Added full-text search, project timelines, and clear ownership so teams can recover context without digging through chat history.", 0, new DateOnly(2024, 8, 1), true, "Independent project", 1, "Lead Engineer", "fieldnote", "https://github.com/example/fieldnote", new DateOnly(2024, 2, 1), "A searchable knowledge hub for teams working across customer projects.", "Fieldnote" },
                    { 102, null, "Built a role-aware dashboard with saved filters, live status updates, and an audit trail. Optimized the API and database queries to keep large work queues responsive.", 1, new DateOnly(2023, 12, 1), true, "Northstar Labs", 1, "Backend and UI Engineer", "signal-board", null, new DateOnly(2023, 3, 1), "An operations dashboard that turns incoming events into actionable queues.", "Signal Board" },
                    { 103, null, "Created a responsive data catalog with reusable chart views, accessible controls, and shareable query URLs for researchers and community groups.", 2, new DateOnly(2022, 11, 1), false, "Civic Works", 1, "Full-stack Engineer", "open-data-explorer", "https://github.com/example/open-data-explorer", new DateOnly(2022, 4, 1), "A lightweight way to browse, compare, and export public datasets.", "Open Data Explorer" },
                    { 104, "https://studio-calendar.example.com", "Delivered shared availability, conflict detection, and approval flows with a mobile-first calendar. Added background reminders and clear permissions for members, guests, and administrators.", 0, new DateOnly(2024, 10, 1), true, "Brightside Studio", 2, "Product Engineer", "studio-calendar", null, new DateOnly(2024, 1, 1), "A collaborative scheduling workspace for small creative studios.", "Studio Calendar" },
                    { 105, null, "Integrated build events, deployment checks, and incident notes into one release timeline. Added retry-safe webhooks and notifications that link directly to the relevant change.", 1, new DateOnly(2023, 11, 1), true, "Cloudline", 2, "Full-stack Developer", "release-companion", "https://github.com/example/release-companion", new DateOnly(2023, 2, 1), "A deployment companion that makes release status visible to every team.", "Release Companion" },
                    { 106, "https://pantry-map.example.com", "Built an accessible map and contribution flow that lets local organizations keep resource details current. Focused on clear mobile navigation and simple moderation.", 2, new DateOnly(2022, 12, 1), false, "Neighbourhood Network", 2, "Volunteer Developer", "community-pantry-map", null, new DateOnly(2022, 5, 1), "A volunteer-maintained map of food resources and opening hours.", "Community Pantry Map" }
                });

            migrationBuilder.InsertData(
                table: "PortfolioSkills",
                columns: new[] { "Id", "Category", "DisplayOrder", "Name", "ProfileId" },
                values: new object[,]
                {
                    { 301, "Backend", 0, "C#", 1 },
                    { 302, "Backend", 1, "ASP.NET Core", 1 },
                    { 303, "Data", 0, "SQL Server", 1 },
                    { 304, "Frontend", 0, "TypeScript", 1 },
                    { 305, "Frontend", 1, "React", 1 },
                    { 306, "Cloud", 0, "Azure", 1 },
                    { 307, "Languages", 0, "TypeScript", 2 },
                    { 308, "Frontend", 0, "React", 2 },
                    { 309, "Backend", 0, "Node.js", 2 },
                    { 310, "Data", 0, "PostgreSQL", 2 },
                    { 311, "Cloud", 0, "Docker", 2 },
                    { 312, "Design", 0, "Accessibility", 2 }
                });

            migrationBuilder.InsertData(
                table: "PortfolioSocialLinks",
                columns: new[] { "Id", "DisplayOrder", "Label", "ProfileId", "Url" },
                values: new object[,]
                {
                    { 401, 0, "GitHub", 1, "https://github.com/alex-rivera" },
                    { 402, 1, "LinkedIn", 1, "https://www.linkedin.com/in/alex-rivera" },
                    { 403, 2, "Website", 1, "https://alex-rivera.example.com" },
                    { 404, 0, "GitHub", 2, "https://github.com/maya-chen" },
                    { 405, 1, "LinkedIn", 2, "https://www.linkedin.com/in/maya-chen" },
                    { 406, 2, "Website", 2, "https://maya-chen.example.com" }
                });

            migrationBuilder.InsertData(
                table: "ProjectSkills",
                columns: new[] { "ProjectId", "SkillId" },
                values: new object[,]
                {
                    { 101, 301 },
                    { 101, 302 },
                    { 101, 303 },
                    { 101, 304 },
                    { 101, 305 },
                    { 102, 301 },
                    { 102, 302 },
                    { 102, 303 },
                    { 102, 306 },
                    { 103, 303 },
                    { 103, 304 },
                    { 103, 305 },
                    { 104, 307 },
                    { 104, 308 },
                    { 104, 310 },
                    { 104, 312 },
                    { 105, 307 },
                    { 105, 309 },
                    { 105, 310 },
                    { 105, 311 },
                    { 106, 307 },
                    { 106, 308 },
                    { 106, 312 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 201);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 202);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 203);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 204);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 401);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 402);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 403);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 404);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 405);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 406);

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 101, 301 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 101, 302 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 101, 303 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 101, 304 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 101, 305 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 102, 301 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 102, 302 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 102, 303 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 102, 306 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 103, 303 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 103, 304 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 103, 305 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 104, 307 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 104, 308 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 104, 310 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 104, 312 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 105, 307 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 105, 309 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 105, 310 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 105, 311 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 106, 307 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 106, 308 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 106, 312 });

            migrationBuilder.DeleteData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 301);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 302);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 303);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 304);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 305);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 306);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 307);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 308);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 309);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 310);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 311);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 312);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
