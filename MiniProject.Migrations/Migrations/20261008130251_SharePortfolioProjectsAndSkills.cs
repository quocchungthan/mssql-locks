using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiniProject.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SharePortfolioProjectsAndSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioProjects_PortfolioProfiles_ProfileId",
                table: "PortfolioProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioSkills_PortfolioProfiles_ProfileId",
                table: "PortfolioSkills");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioSkills_ProfileId_Category_DisplayOrder",
                table: "PortfolioSkills");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioSkills_ProfileId_Name",
                table: "PortfolioSkills");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PortfolioSkills_DisplayOrder",
                table: "PortfolioSkills");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioProjects_ProfileId_IsFeatured_DisplayOrder",
                table: "PortfolioProjects");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioProjects_ProfileId_Slug",
                table: "PortfolioProjects");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PortfolioProjects_DateRange",
                table: "PortfolioProjects");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PortfolioProjects_DisplayOrder",
                table: "PortfolioProjects");

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
                keyValues: new object[] { 102, 306 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 103, 303 });

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

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "PortfolioSkills");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "PortfolioSkills");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "Organization",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "PortfolioProjects");

            migrationBuilder.CreateTable(
                name: "ProfileProjects",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Organization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileProjects", x => new { x.ProfileId, x.ProjectId });
                    table.CheckConstraint("CK_ProfileProjects_DateRange", "[EndDate] IS NULL OR [StartDate] IS NULL OR [EndDate] >= [StartDate]");
                    table.CheckConstraint("CK_ProfileProjects_DisplayOrder", "[DisplayOrder] >= 0");
                    table.ForeignKey(
                        name: "FK_ProfileProjects_PortfolioProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "PortfolioProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileProjects_PortfolioProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PortfolioProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileSkills",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileSkills", x => new { x.ProfileId, x.SkillId });
                    table.CheckConstraint("CK_ProfileSkills_DisplayOrder", "[DisplayOrder] >= 0");
                    table.ForeignKey(
                        name: "FK_ProfileSkills_PortfolioProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "PortfolioProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileSkills_PortfolioSkills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "PortfolioSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PortfolioProfiles",
                columns: new[] { "Id", "Bio", "ContactEmail", "DisplayName", "Headline", "Location", "ProfileImageUrl", "ResumeUrl" },
                values: new object[,]
                {
                    { 3, "I build calm, dependable tools for busy teams, with an eye for the small details that make software feel clear.", "jordan-brooks@example.com", "Jordan Brooks", "Product-minded software engineer", "Portland, OR", "https://images.example.com/profiles/jordan-brooks.jpg", "https://example.com/resumes/jordan-brooks.pdf" },
                    { 4, "I turn complicated service journeys into approachable products and enjoy working closely with people who use what I build.", "priya-nair@example.com", "Priya Nair", "Full-stack developer | Digital health", "Vancouver, BC", "https://images.example.com/profiles/priya-nair.jpg", "https://example.com/resumes/priya-nair.pdf" },
                    { 5, "I like making reliable systems observable and understandable, especially when many services and people meet in one workflow.", "mateo-alvarez@example.com", "Mateo Alvarez", "Backend engineer | Distributed systems", "Austin, TX", "https://images.example.com/profiles/mateo-alvarez.jpg", "https://example.com/resumes/mateo-alvarez.pdf" },
                    { 6, "I build accessible interfaces that help people move through complex information without losing the thread.", "olivia-martin@example.com", "Olivia Martin", "UX-focused frontend engineer", "Montreal, QC", "https://images.example.com/profiles/olivia-martin.jpg", "https://example.com/resumes/olivia-martin.pdf" },
                    { 7, "I enjoy improving the everyday experience of building, testing, and shipping software for small product teams.", "ethan-kim@example.com", "Ethan Kim", "Software developer | Developer tools", "San Jose, CA", "https://images.example.com/profiles/ethan-kim.jpg", "https://example.com/resumes/ethan-kim.pdf" },
                    { 8, "I work on trustworthy data products that help researchers and communities understand a changing environment.", "sofia-rossi@example.com", "Sofia Rossi", "Data engineer | Climate technology", "Denver, CO", "https://images.example.com/profiles/sofia-rossi.jpg", "https://example.com/resumes/sofia-rossi.pdf" },
                    { 9, "I help teams ship safely by making infrastructure repeatable, observable, and easier to operate.", "noah-patel@example.com", "Noah Patel", "Cloud engineer | Reliable services", "Calgary, AB", "https://images.example.com/profiles/noah-patel.jpg", "https://example.com/resumes/noah-patel.pdf" },
                    { 10, "I care about useful public-interest software and build tools that make community information easier to reach.", "amara-okafor@example.com", "Amara Okafor", "Software engineer | Civic technology", "Chicago, IL", "https://images.example.com/profiles/amara-okafor.jpg", "https://example.com/resumes/amara-okafor.pdf" },
                    { 11, "I create practical mobile-first experiences, from the first sketch through launch and iteration.", "lucas-bennett@example.com", "Lucas Bennett", "Mobile and web developer", "Boston, MA", "https://images.example.com/profiles/lucas-bennett.jpg", "https://example.com/resumes/lucas-bennett.pdf" },
                    { 12, "I build thoughtful creative software and enjoy bridging design systems, product needs, and implementation details.", "mei-tanaka@example.com", "Mei Tanaka", "Application developer | Creative tools", "Seattle, WA", "https://images.example.com/profiles/mei-tanaka.jpg", "https://example.com/resumes/mei-tanaka.pdf" },
                    { 13, "I make collaborative products feel simple, even when the underlying permissions, data, and workflows are not.", "gabriel-santos@example.com", "Gabriel Santos", "Full-stack engineer | Collaboration products", "Miami, FL", "https://images.example.com/profiles/gabriel-santos.jpg", "https://example.com/resumes/gabriel-santos.pdf" },
                    { 14, "I help teams build secure-by-default products while keeping the user experience direct and approachable.", "zara-ahmed@example.com", "Zara Ahmed", "Security-minded software engineer", "Ottawa, ON", "https://images.example.com/profiles/zara-ahmed.jpg", "https://example.com/resumes/zara-ahmed.pdf" },
                    { 15, "I build clear financial tools that help people understand their options and make confident decisions.", "caleb-foster@example.com", "Caleb Foster", "Software engineer | Financial products", "New York, NY", "https://images.example.com/profiles/caleb-foster.jpg", "https://example.com/resumes/caleb-foster.pdf" },
                    { 16, "I enjoy building reusable interface foundations that help product teams move faster without sacrificing accessibility.", "isabella-garcia@example.com", "Isabella Garcia", "Frontend engineer | Design systems", "Phoenix, AZ", "https://images.example.com/profiles/isabella-garcia.jpg", "https://example.com/resumes/isabella-garcia.pdf" },
                    { 17, "I make data pipelines and internal tools easier to trust, debug, and extend as a product grows.", "aiden-wilson@example.com", "Aiden Wilson", "Data platform developer", "Edmonton, AB", "https://images.example.com/profiles/aiden-wilson.jpg", "https://example.com/resumes/aiden-wilson.pdf" },
                    { 18, "I build inclusive learning experiences and care about giving educators useful feedback without adding busywork.", "leila-haddad@example.com", "Leila Haddad", "Software developer | Education", "Philadelphia, PA", "https://images.example.com/profiles/leila-haddad.jpg", "https://example.com/resumes/leila-haddad.pdf" },
                    { 19, "I work on developer platforms and open-source projects that make the reliable path the easy path.", "theo-nguyen@example.com", "Theo Nguyen", "Platform engineer | Open source", "San Diego, CA", "https://images.example.com/profiles/theo-nguyen.jpg", "https://example.com/resumes/theo-nguyen.pdf" },
                    { 20, "I create approachable digital services for local groups and the people who rely on them.", "nina-kowalski@example.com", "Nina Kowalski", "Full-stack developer | Local communities", "Milwaukee, WI", "https://images.example.com/profiles/nina-kowalski.jpg", "https://example.com/resumes/nina-kowalski.pdf" },
                    { 21, "I build map-based products that make place, context, and useful local knowledge easier to explore.", "owen-clarke@example.com", "Owen Clarke", "Software engineer | Mapping", "Halifax, NS", "https://images.example.com/profiles/owen-clarke.jpg", "https://example.com/resumes/owen-clarke.pdf" },
                    { 22, "I design resilient commerce services and enjoy smoothing out the small points of friction in a customer journey.", "fatima-rahman@example.com", "Fatima Rahman", "Backend developer | Commerce", "Dallas, TX", "https://images.example.com/profiles/fatima-rahman.jpg", "https://example.com/resumes/fatima-rahman.pdf" },
                    { 23, "I build tools for distributed teams and like turning scattered updates into shared understanding.", "henry-adams@example.com", "Henry Adams", "Product engineer | Remote collaboration", "Minneapolis, MN", "https://images.example.com/profiles/henry-adams.jpg", "https://example.com/resumes/henry-adams.pdf" },
                    { 24, "I help cultural organizations share their work online through accessible, expressive, and maintainable software.", "camila-ferreira@example.com", "Camila Ferreira", "Software engineer | Arts and culture", "San Antonio, TX", "https://images.example.com/profiles/camila-ferreira.jpg", "https://example.com/resumes/camila-ferreira.pdf" },
                    { 25, "I make production systems easier to understand with practical telemetry, useful dashboards, and careful automation.", "arjun-mehta@example.com", "Arjun Mehta", "Cloud developer | Observability", "Waterloo, ON", "https://images.example.com/profiles/arjun-mehta.jpg", "https://example.com/resumes/arjun-mehta.pdf" },
                    { 26, "I build accessible public-service interfaces that help people complete important tasks with confidence.", "grace-thompson@example.com", "Grace Thompson", "Frontend developer | Public services", "Columbus, OH", "https://images.example.com/profiles/grace-thompson.jpg", "https://example.com/resumes/grace-thompson.pdf" },
                    { 27, "I build tools that connect local food networks, independent producers, and the communities around them.", "rafael-costa@example.com", "Rafael Costa", "Software developer | Food systems", "Orlando, FL", "https://images.example.com/profiles/rafael-costa.jpg", "https://example.com/resumes/rafael-costa.pdf" },
                    { 28, "I create useful travel products that help people plan flexible trips and find memorable local experiences.", "chloe-dubois@example.com", "Chloe Dubois", "Full-stack engineer | Travel", "Quebec City, QC", "https://images.example.com/profiles/chloe-dubois.jpg", "https://example.com/resumes/chloe-dubois.pdf" },
                    { 29, "I build dependable health software with careful attention to privacy, clarity, and the needs of care teams.", "isaac-morgan@example.com", "Isaac Morgan", "Software engineer | Health platforms", "Nashville, TN", "https://images.example.com/profiles/isaac-morgan.jpg", "https://example.com/resumes/isaac-morgan.pdf" },
                    { 30, "I love building small, repeatable learning moments that help people practice a new language every day.", "elena-petrova@example.com", "Elena Petrova", "Developer | Language learning", "Victoria, BC", "https://images.example.com/profiles/elena-petrova.jpg", "https://example.com/resumes/elena-petrova.pdf" },
                    { 31, "I improve the software behind moving goods, coordinating people, and responding when plans change.", "david-okoye@example.com", "David Okoye", "Backend engineer | Logistics", "Atlanta, GA", "https://images.example.com/profiles/david-okoye.jpg", "https://example.com/resumes/david-okoye.pdf" },
                    { 32, "I build friendly financial tools that turn everyday decisions into manageable, understandable steps.", "mina-park@example.com", "Mina Park", "Software developer | Personal finance", "Irvine, CA", "https://images.example.com/profiles/mina-park.jpg", "https://example.com/resumes/mina-park.pdf" },
                    { 33, "I make outdoor planning tools that work well on the move and remain useful when connectivity is limited.", "samuel-reed@example.com", "Samuel Reed", "Mobile developer | Outdoor recreation", "Boise, ID", "https://images.example.com/profiles/samuel-reed.jpg", "https://example.com/resumes/samuel-reed.pdf" },
                    { 34, "I help mission-driven teams make better use of their data without losing sight of the people behind it.", "aisha-yusuf@example.com", "Aisha Yusuf", "Data developer | Nonprofit technology", "Detroit, MI", "https://images.example.com/profiles/aisha-yusuf.jpg", "https://example.com/resumes/aisha-yusuf.pdf" },
                    { 35, "I build publishing and media tools that make it easier for creators to organize, distribute, and learn from their work.", "julian-meyer@example.com", "Julian Meyer", "Software engineer | Media platforms", "Los Angeles, CA", "https://images.example.com/profiles/julian-meyer.jpg", "https://example.com/resumes/julian-meyer.pdf" },
                    { 36, "I turn accessibility principles into practical interface patterns and help teams include more people by default.", "hana-suzuki@example.com", "Hana Suzuki", "Frontend engineer | Accessibility", "Boulder, CO", "https://images.example.com/profiles/hana-suzuki.jpg", "https://example.com/resumes/hana-suzuki.pdf" },
                    { 37, "I build pragmatic software for small businesses, focusing on useful workflows and low-maintenance operations.", "benjamin-lee@example.com", "Benjamin Lee", "Full-stack developer | Small businesses", "Richmond, BC", "https://images.example.com/profiles/benjamin-lee.jpg", "https://example.com/resumes/benjamin-lee.pdf" },
                    { 38, "I work on digital products that help people and organizations make more sustainable everyday choices.", "layla-hassan@example.com", "Layla Hassan", "Software engineer | Sustainability", "Tucson, AZ", "https://images.example.com/profiles/layla-hassan.jpg", "https://example.com/resumes/layla-hassan.pdf" }
                });

            migrationBuilder.InsertData(
                table: "PortfolioProjects",
                columns: new[] { "Id", "DemoUrl", "Description", "Slug", "SourceUrl", "Summary", "Title" },
                values: new object[,]
                {
                    { 107, "https://pocket-planner.example.com", "Built a lightweight planner with reusable checklists, calendar views, and gentle progress summaries for individuals and small groups.", "pocket-planner", "https://github.com/example/pocket-planner", "A simple planning tool for turning a big goal into steady weekly progress.", "Pocket Planner" },
                    { 108, "https://trail-atlas.example.com", "Created a map-first directory with crowd-sourced updates, practical access notes, and downloadable trip details for visitors planning ahead.", "trail-atlas", null, "A community guide to accessible trails, parks, and outdoor facilities.", "Trail Atlas" }
                });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 301,
                column: "Category",
                value: "Languages");

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 304,
                column: "Category",
                value: "Languages");

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 307,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Backend", "Node.js" });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 308,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Data", "PostgreSQL" });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 309,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Cloud", "Docker" });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 310,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Design", "Accessibility" });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 311,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Languages", "JavaScript" });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 312,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Frontend", "Vue" });

            migrationBuilder.InsertData(
                table: "PortfolioSkills",
                columns: new[] { "Id", "Category", "Name" },
                values: new object[,]
                {
                    { 313, "Data", "Redis" },
                    { 314, "Frontend", "Angular" },
                    { 315, "Languages", "Python" },
                    { 316, "Backend", "Django" },
                    { 317, "Cloud", "AWS" },
                    { 318, "Languages", "Kotlin" },
                    { 319, "Frontend", "React Native" },
                    { 320, "Backend", "Java" },
                    { 321, "Data", "MySQL" },
                    { 322, "Languages", "Go" },
                    { 323, "Frontend", "Svelte" },
                    { 324, "Data", "MongoDB" },
                    { 325, "Cloud", "Kubernetes" }
                });

            migrationBuilder.InsertData(
                table: "ProfileProjects",
                columns: new[] { "ProfileId", "ProjectId", "DisplayOrder", "EndDate", "IsFeatured", "Organization", "Role", "StartDate" },
                values: new object[,]
                {
                    { 1, 101, 0, new DateOnly(2024, 8, 1), true, "Independent project", "Lead Engineer", new DateOnly(2024, 2, 1) },
                    { 1, 102, 1, new DateOnly(2023, 12, 1), true, "Northstar Labs", "Backend and UI Engineer", new DateOnly(2023, 3, 1) },
                    { 1, 103, 2, new DateOnly(2022, 11, 1), false, "Civic Works", "Full-stack Engineer", new DateOnly(2022, 4, 1) },
                    { 2, 104, 0, new DateOnly(2024, 10, 1), true, "Brightside Studio", "Product Engineer", new DateOnly(2024, 1, 1) },
                    { 2, 105, 1, new DateOnly(2023, 11, 1), true, "Cloudline", "Full-stack Developer", new DateOnly(2023, 2, 1) },
                    { 2, 106, 2, new DateOnly(2022, 12, 1), false, "Neighbourhood Network", "Volunteer Developer", new DateOnly(2022, 5, 1) }
                });

            migrationBuilder.InsertData(
                table: "ProfileSkills",
                columns: new[] { "ProfileId", "SkillId", "DisplayOrder" },
                values: new object[,]
                {
                    { 1, 301, 0 },
                    { 1, 302, 1 },
                    { 1, 303, 2 },
                    { 1, 304, 3 },
                    { 1, 305, 4 },
                    { 1, 306, 5 },
                    { 2, 304, 0 },
                    { 2, 305, 1 },
                    { 2, 307, 2 },
                    { 2, 308, 3 },
                    { 2, 309, 4 },
                    { 2, 310, 5 }
                });

            migrationBuilder.InsertData(
                table: "ProjectSkills",
                columns: new[] { "ProjectId", "SkillId" },
                values: new object[,]
                {
                    { 102, 304 },
                    { 102, 305 },
                    { 103, 307 },
                    { 103, 308 },
                    { 103, 309 },
                    { 104, 304 },
                    { 104, 305 },
                    { 105, 304 },
                    { 105, 305 },
                    { 105, 308 },
                    { 106, 301 },
                    { 106, 302 },
                    { 106, 303 },
                    { 106, 306 }
                });

            migrationBuilder.InsertData(
                table: "PortfolioExperiences",
                columns: new[] { "Id", "Company", "Description", "DisplayOrder", "EndDate", "IsCurrent", "JobTitle", "ProfileId", "StartDate" },
                values: new object[,]
                {
                    { 2000, "Cedarline", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Cedarline's core workflows.", 0, null, true, "Senior Software Engineer", 3, new DateOnly(2020, 1, 1) },
                    { 2001, "Maple Systems", "Built and maintained product features at Maple Systems, learning to ship in small increments and support software after release.", 1, new DateOnly(2019, 12, 1), false, "Software Engineer", 3, new DateOnly(2018, 5, 1) },
                    { 2002, "Wellnest", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Wellnest's core workflows.", 0, null, true, "Full-stack Developer", 4, new DateOnly(2021, 2, 1) },
                    { 2003, "CarePath", "Built and maintained product features at CarePath, learning to ship in small increments and support software after release.", 1, new DateOnly(2020, 12, 1), false, "Application Developer", 4, new DateOnly(2019, 6, 1) },
                    { 2004, "Orbit Works", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Orbit Works's core workflows.", 0, null, true, "Backend Engineer", 5, new DateOnly(2022, 3, 1) },
                    { 2005, "Copperfield", "Built and maintained product features at Copperfield, learning to ship in small increments and support software after release.", 1, new DateOnly(2021, 12, 1), false, "Software Developer", 5, new DateOnly(2020, 7, 1) },
                    { 2006, "Northwind Studio", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Northwind Studio's core workflows.", 0, null, true, "Frontend Engineer", 6, new DateOnly(2023, 4, 1) },
                    { 2007, "Brightside Digital", "Built and maintained product features at Brightside Digital, learning to ship in small increments and support software after release.", 1, new DateOnly(2022, 12, 1), false, "UI Developer", 6, new DateOnly(2021, 8, 1) },
                    { 2008, "Buildcraft", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Buildcraft's core workflows.", 0, null, true, "Developer Experience Engineer", 7, new DateOnly(2024, 5, 1) },
                    { 2009, "Pixel Forge", "Built and maintained product features at Pixel Forge, learning to ship in small increments and support software after release.", 1, new DateOnly(2023, 12, 1), false, "Software Engineer", 7, new DateOnly(2022, 9, 1) },
                    { 2010, "Terrain Labs", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Terrain Labs's core workflows.", 0, null, true, "Data Engineer", 8, new DateOnly(2020, 6, 1) },
                    { 2011, "OpenField", "Built and maintained product features at OpenField, learning to ship in small increments and support software after release.", 1, new DateOnly(2019, 12, 1), false, "Research Developer", 8, new DateOnly(2018, 10, 1) },
                    { 2012, "Cloud Harbor", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Cloud Harbor's core workflows.", 0, null, true, "Cloud Engineer", 9, new DateOnly(2021, 7, 1) },
                    { 2013, "Redwood Apps", "Built and maintained product features at Redwood Apps, learning to ship in small increments and support software after release.", 1, new DateOnly(2020, 12, 1), false, "Platform Developer", 9, new DateOnly(2019, 11, 1) },
                    { 2014, "Common Ground", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Common Ground's core workflows.", 0, null, true, "Software Engineer", 10, new DateOnly(2022, 8, 1) },
                    { 2015, "CityLab", "Built and maintained product features at CityLab, learning to ship in small increments and support software after release.", 1, new DateOnly(2021, 12, 1), false, "Web Developer", 10, new DateOnly(2020, 12, 1) },
                    { 2016, "Harborlight", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Harborlight's core workflows.", 0, null, true, "Mobile Engineer", 11, new DateOnly(2023, 9, 1) },
                    { 2017, "Tandem Apps", "Built and maintained product features at Tandem Apps, learning to ship in small increments and support software after release.", 1, new DateOnly(2022, 12, 1), false, "Junior Developer", 11, new DateOnly(2021, 1, 1) },
                    { 2018, "Mosaic Studio", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Mosaic Studio's core workflows.", 0, null, true, "Application Developer", 12, new DateOnly(2024, 10, 1) },
                    { 2019, "Paper Kite", "Built and maintained product features at Paper Kite, learning to ship in small increments and support software after release.", 1, new DateOnly(2023, 12, 1), false, "Frontend Developer", 12, new DateOnly(2022, 2, 1) },
                    { 2020, "Relay Labs", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Relay Labs's core workflows.", 0, null, true, "Full-stack Engineer", 13, new DateOnly(2020, 11, 1) },
                    { 2021, "Loopline", "Built and maintained product features at Loopline, learning to ship in small increments and support software after release.", 1, new DateOnly(2019, 12, 1), false, "Web Engineer", 13, new DateOnly(2018, 3, 1) },
                    { 2022, "Lockstep", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Lockstep's core workflows.", 0, null, true, "Software Engineer", 14, new DateOnly(2021, 12, 1) },
                    { 2023, "Evergreen Tech", "Built and maintained product features at Evergreen Tech, learning to ship in small increments and support software after release.", 1, new DateOnly(2020, 12, 1), false, "Application Developer", 14, new DateOnly(2019, 4, 1) },
                    { 2024, "Commonwealth", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Commonwealth's core workflows.", 0, null, true, "Product Engineer", 15, new DateOnly(2022, 1, 1) },
                    { 2025, "Ledger House", "Built and maintained product features at Ledger House, learning to ship in small increments and support software after release.", 1, new DateOnly(2021, 12, 1), false, "Software Developer", 15, new DateOnly(2020, 5, 1) },
                    { 2026, "Kindred Design", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Kindred Design's core workflows.", 0, null, true, "Frontend Engineer", 16, new DateOnly(2023, 2, 1) },
                    { 2027, "Canyon Software", "Built and maintained product features at Canyon Software, learning to ship in small increments and support software after release.", 1, new DateOnly(2022, 12, 1), false, "UI Engineer", 16, new DateOnly(2021, 6, 1) },
                    { 2028, "Granite Data", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Granite Data's core workflows.", 0, null, true, "Data Platform Developer", 17, new DateOnly(2024, 3, 1) },
                    { 2029, "Signal Peak", "Built and maintained product features at Signal Peak, learning to ship in small increments and support software after release.", 1, new DateOnly(2023, 12, 1), false, "Data Developer", 17, new DateOnly(2022, 7, 1) },
                    { 2030, "Learnwell", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Learnwell's core workflows.", 0, null, true, "Software Developer", 18, new DateOnly(2020, 4, 1) },
                    { 2031, "Open Classroom", "Built and maintained product features at Open Classroom, learning to ship in small increments and support software after release.", 1, new DateOnly(2019, 12, 1), false, "Web Developer", 18, new DateOnly(2018, 8, 1) },
                    { 2032, "Meridian Tools", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Meridian Tools's core workflows.", 0, null, true, "Platform Engineer", 19, new DateOnly(2021, 5, 1) },
                    { 2033, "Foss Harbor", "Built and maintained product features at Foss Harbor, learning to ship in small increments and support software after release.", 1, new DateOnly(2020, 12, 1), false, "Software Engineer", 19, new DateOnly(2019, 9, 1) },
                    { 2034, "Block & Bridge", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Block & Bridge's core workflows.", 0, null, true, "Full-stack Developer", 20, new DateOnly(2022, 6, 1) },
                    { 2035, "Civic Thread", "Built and maintained product features at Civic Thread, learning to ship in small increments and support software after release.", 1, new DateOnly(2021, 12, 1), false, "Application Developer", 20, new DateOnly(2020, 10, 1) },
                    { 2036, "Waypoint", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Waypoint's core workflows.", 0, null, true, "Software Engineer", 21, new DateOnly(2023, 7, 1) },
                    { 2037, "Atlas North", "Built and maintained product features at Atlas North, learning to ship in small increments and support software after release.", 1, new DateOnly(2022, 12, 1), false, "GIS Developer", 21, new DateOnly(2021, 11, 1) },
                    { 2038, "Market Street", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Market Street's core workflows.", 0, null, true, "Backend Developer", 22, new DateOnly(2024, 8, 1) },
                    { 2039, "Shopwell", "Built and maintained product features at Shopwell, learning to ship in small increments and support software after release.", 1, new DateOnly(2023, 12, 1), false, "Software Engineer", 22, new DateOnly(2022, 12, 1) },
                    { 2040, "Farview", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Farview's core workflows.", 0, null, true, "Product Engineer", 23, new DateOnly(2020, 9, 1) },
                    { 2041, "Workroom", "Built and maintained product features at Workroom, learning to ship in small increments and support software after release.", 1, new DateOnly(2019, 12, 1), false, "Full-stack Developer", 23, new DateOnly(2018, 1, 1) },
                    { 2042, "Canvas House", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Canvas House's core workflows.", 0, null, true, "Software Engineer", 24, new DateOnly(2021, 10, 1) },
                    { 2043, "Public Gallery", "Built and maintained product features at Public Gallery, learning to ship in small increments and support software after release.", 1, new DateOnly(2020, 12, 1), false, "Web Developer", 24, new DateOnly(2019, 2, 1) },
                    { 2044, "Tracepoint", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Tracepoint's core workflows.", 0, null, true, "Cloud Developer", 25, new DateOnly(2022, 11, 1) },
                    { 2045, "Blue Summit", "Built and maintained product features at Blue Summit, learning to ship in small increments and support software after release.", 1, new DateOnly(2021, 12, 1), false, "Software Engineer", 25, new DateOnly(2020, 3, 1) },
                    { 2046, "Civic Bridge", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Civic Bridge's core workflows.", 0, null, true, "Frontend Developer", 26, new DateOnly(2023, 12, 1) },
                    { 2047, "State Digital", "Built and maintained product features at State Digital, learning to ship in small increments and support software after release.", 1, new DateOnly(2022, 12, 1), false, "UI Developer", 26, new DateOnly(2021, 4, 1) },
                    { 2048, "Harvest Table", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Harvest Table's core workflows.", 0, null, true, "Software Developer", 27, new DateOnly(2024, 1, 1) },
                    { 2049, "Local Basket", "Built and maintained product features at Local Basket, learning to ship in small increments and support software after release.", 1, new DateOnly(2023, 12, 1), false, "Full-stack Developer", 27, new DateOnly(2022, 5, 1) },
                    { 2050, "Roamstead", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Roamstead's core workflows.", 0, null, true, "Full-stack Engineer", 28, new DateOnly(2020, 2, 1) },
                    { 2051, "Trailmark", "Built and maintained product features at Trailmark, learning to ship in small increments and support software after release.", 1, new DateOnly(2019, 12, 1), false, "Web Developer", 28, new DateOnly(2018, 6, 1) },
                    { 2052, "Juniper Health", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Juniper Health's core workflows.", 0, null, true, "Software Engineer", 29, new DateOnly(2021, 3, 1) },
                    { 2053, "Wellnest", "Built and maintained product features at Wellnest, learning to ship in small increments and support software after release.", 1, new DateOnly(2020, 12, 1), false, "Backend Developer", 29, new DateOnly(2019, 7, 1) },
                    { 2054, "Lingua Loop", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Lingua Loop's core workflows.", 0, null, true, "Product Developer", 30, new DateOnly(2022, 4, 1) },
                    { 2055, "Wordgarden", "Built and maintained product features at Wordgarden, learning to ship in small increments and support software after release.", 1, new DateOnly(2021, 12, 1), false, "Frontend Developer", 30, new DateOnly(2020, 8, 1) },
                    { 2056, "Routecraft", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Routecraft's core workflows.", 0, null, true, "Backend Engineer", 31, new DateOnly(2023, 5, 1) },
                    { 2057, "Freightline", "Built and maintained product features at Freightline, learning to ship in small increments and support software after release.", 1, new DateOnly(2022, 12, 1), false, "Software Developer", 31, new DateOnly(2021, 9, 1) },
                    { 2058, "Pocketwise", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Pocketwise's core workflows.", 0, null, true, "Software Developer", 32, new DateOnly(2024, 6, 1) },
                    { 2059, "Clearpath Finance", "Built and maintained product features at Clearpath Finance, learning to ship in small increments and support software after release.", 1, new DateOnly(2023, 12, 1), false, "Frontend Engineer", 32, new DateOnly(2022, 10, 1) },
                    { 2060, "Trailmark", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Trailmark's core workflows.", 0, null, true, "Mobile Developer", 33, new DateOnly(2020, 7, 1) },
                    { 2061, "Summit Apps", "Built and maintained product features at Summit Apps, learning to ship in small increments and support software after release.", 1, new DateOnly(2019, 12, 1), false, "Software Engineer", 33, new DateOnly(2018, 11, 1) },
                    { 2062, "Goodworks", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Goodworks's core workflows.", 0, null, true, "Data Developer", 34, new DateOnly(2021, 8, 1) },
                    { 2063, "Community Metrics", "Built and maintained product features at Community Metrics, learning to ship in small increments and support software after release.", 1, new DateOnly(2020, 12, 1), false, "Analyst Developer", 34, new DateOnly(2019, 12, 1) },
                    { 2064, "Signal House", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Signal House's core workflows.", 0, null, true, "Software Engineer", 35, new DateOnly(2022, 9, 1) },
                    { 2065, "Studio North", "Built and maintained product features at Studio North, learning to ship in small increments and support software after release.", 1, new DateOnly(2021, 12, 1), false, "Web Developer", 35, new DateOnly(2020, 1, 1) },
                    { 2066, "Open Door Labs", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Open Door Labs's core workflows.", 0, null, true, "Frontend Engineer", 36, new DateOnly(2023, 10, 1) },
                    { 2067, "Kindred Design", "Built and maintained product features at Kindred Design, learning to ship in small increments and support software after release.", 1, new DateOnly(2022, 12, 1), false, "UI Developer", 36, new DateOnly(2021, 2, 1) },
                    { 2068, "Main Street Software", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Main Street Software's core workflows.", 0, null, true, "Full-stack Developer", 37, new DateOnly(2024, 11, 1) },
                    { 2069, "Cornerstone Apps", "Built and maintained product features at Cornerstone Apps, learning to ship in small increments and support software after release.", 1, new DateOnly(2023, 12, 1), false, "Software Developer", 37, new DateOnly(2022, 3, 1) },
                    { 2070, "Greenline", "Owned delivery of customer-facing features, partnered with design and product, and improved the reliability of Greenline's core workflows.", 0, null, true, "Software Engineer", 38, new DateOnly(2020, 12, 1) },
                    { 2071, "Earthwise Digital", "Built and maintained product features at Earthwise Digital, learning to ship in small increments and support software after release.", 1, new DateOnly(2019, 12, 1), false, "Application Developer", 38, new DateOnly(2018, 4, 1) }
                });

            migrationBuilder.InsertData(
                table: "PortfolioSocialLinks",
                columns: new[] { "Id", "DisplayOrder", "Label", "ProfileId", "Url" },
                values: new object[,]
                {
                    { 4000, 0, "GitHub", 3, "https://github.com/jordan-brooks" },
                    { 4001, 1, "LinkedIn", 3, "https://www.linkedin.com/in/jordan-brooks" },
                    { 4002, 0, "GitHub", 4, "https://github.com/priya-nair" },
                    { 4003, 1, "LinkedIn", 4, "https://www.linkedin.com/in/priya-nair" },
                    { 4004, 0, "GitHub", 5, "https://github.com/mateo-alvarez" },
                    { 4005, 1, "LinkedIn", 5, "https://www.linkedin.com/in/mateo-alvarez" },
                    { 4006, 0, "GitHub", 6, "https://github.com/olivia-martin" },
                    { 4007, 1, "LinkedIn", 6, "https://www.linkedin.com/in/olivia-martin" },
                    { 4008, 0, "GitHub", 7, "https://github.com/ethan-kim" },
                    { 4009, 1, "LinkedIn", 7, "https://www.linkedin.com/in/ethan-kim" },
                    { 4010, 0, "GitHub", 8, "https://github.com/sofia-rossi" },
                    { 4011, 1, "LinkedIn", 8, "https://www.linkedin.com/in/sofia-rossi" },
                    { 4012, 0, "GitHub", 9, "https://github.com/noah-patel" },
                    { 4013, 1, "LinkedIn", 9, "https://www.linkedin.com/in/noah-patel" },
                    { 4014, 0, "GitHub", 10, "https://github.com/amara-okafor" },
                    { 4015, 1, "LinkedIn", 10, "https://www.linkedin.com/in/amara-okafor" },
                    { 4016, 0, "GitHub", 11, "https://github.com/lucas-bennett" },
                    { 4017, 1, "LinkedIn", 11, "https://www.linkedin.com/in/lucas-bennett" },
                    { 4018, 0, "GitHub", 12, "https://github.com/mei-tanaka" },
                    { 4019, 1, "LinkedIn", 12, "https://www.linkedin.com/in/mei-tanaka" },
                    { 4020, 0, "GitHub", 13, "https://github.com/gabriel-santos" },
                    { 4021, 1, "LinkedIn", 13, "https://www.linkedin.com/in/gabriel-santos" },
                    { 4022, 0, "GitHub", 14, "https://github.com/zara-ahmed" },
                    { 4023, 1, "LinkedIn", 14, "https://www.linkedin.com/in/zara-ahmed" },
                    { 4024, 0, "GitHub", 15, "https://github.com/caleb-foster" },
                    { 4025, 1, "LinkedIn", 15, "https://www.linkedin.com/in/caleb-foster" },
                    { 4026, 0, "GitHub", 16, "https://github.com/isabella-garcia" },
                    { 4027, 1, "LinkedIn", 16, "https://www.linkedin.com/in/isabella-garcia" },
                    { 4028, 0, "GitHub", 17, "https://github.com/aiden-wilson" },
                    { 4029, 1, "LinkedIn", 17, "https://www.linkedin.com/in/aiden-wilson" },
                    { 4030, 0, "GitHub", 18, "https://github.com/leila-haddad" },
                    { 4031, 1, "LinkedIn", 18, "https://www.linkedin.com/in/leila-haddad" },
                    { 4032, 0, "GitHub", 19, "https://github.com/theo-nguyen" },
                    { 4033, 1, "LinkedIn", 19, "https://www.linkedin.com/in/theo-nguyen" },
                    { 4034, 0, "GitHub", 20, "https://github.com/nina-kowalski" },
                    { 4035, 1, "LinkedIn", 20, "https://www.linkedin.com/in/nina-kowalski" },
                    { 4036, 0, "GitHub", 21, "https://github.com/owen-clarke" },
                    { 4037, 1, "LinkedIn", 21, "https://www.linkedin.com/in/owen-clarke" },
                    { 4038, 0, "GitHub", 22, "https://github.com/fatima-rahman" },
                    { 4039, 1, "LinkedIn", 22, "https://www.linkedin.com/in/fatima-rahman" },
                    { 4040, 0, "GitHub", 23, "https://github.com/henry-adams" },
                    { 4041, 1, "LinkedIn", 23, "https://www.linkedin.com/in/henry-adams" },
                    { 4042, 0, "GitHub", 24, "https://github.com/camila-ferreira" },
                    { 4043, 1, "LinkedIn", 24, "https://www.linkedin.com/in/camila-ferreira" },
                    { 4044, 0, "GitHub", 25, "https://github.com/arjun-mehta" },
                    { 4045, 1, "LinkedIn", 25, "https://www.linkedin.com/in/arjun-mehta" },
                    { 4046, 0, "GitHub", 26, "https://github.com/grace-thompson" },
                    { 4047, 1, "LinkedIn", 26, "https://www.linkedin.com/in/grace-thompson" },
                    { 4048, 0, "GitHub", 27, "https://github.com/rafael-costa" },
                    { 4049, 1, "LinkedIn", 27, "https://www.linkedin.com/in/rafael-costa" },
                    { 4050, 0, "GitHub", 28, "https://github.com/chloe-dubois" },
                    { 4051, 1, "LinkedIn", 28, "https://www.linkedin.com/in/chloe-dubois" },
                    { 4052, 0, "GitHub", 29, "https://github.com/isaac-morgan" },
                    { 4053, 1, "LinkedIn", 29, "https://www.linkedin.com/in/isaac-morgan" },
                    { 4054, 0, "GitHub", 30, "https://github.com/elena-petrova" },
                    { 4055, 1, "LinkedIn", 30, "https://www.linkedin.com/in/elena-petrova" },
                    { 4056, 0, "GitHub", 31, "https://github.com/david-okoye" },
                    { 4057, 1, "LinkedIn", 31, "https://www.linkedin.com/in/david-okoye" },
                    { 4058, 0, "GitHub", 32, "https://github.com/mina-park" },
                    { 4059, 1, "LinkedIn", 32, "https://www.linkedin.com/in/mina-park" },
                    { 4060, 0, "GitHub", 33, "https://github.com/samuel-reed" },
                    { 4061, 1, "LinkedIn", 33, "https://www.linkedin.com/in/samuel-reed" },
                    { 4062, 0, "GitHub", 34, "https://github.com/aisha-yusuf" },
                    { 4063, 1, "LinkedIn", 34, "https://www.linkedin.com/in/aisha-yusuf" },
                    { 4064, 0, "GitHub", 35, "https://github.com/julian-meyer" },
                    { 4065, 1, "LinkedIn", 35, "https://www.linkedin.com/in/julian-meyer" },
                    { 4066, 0, "GitHub", 36, "https://github.com/hana-suzuki" },
                    { 4067, 1, "LinkedIn", 36, "https://www.linkedin.com/in/hana-suzuki" },
                    { 4068, 0, "GitHub", 37, "https://github.com/benjamin-lee" },
                    { 4069, 1, "LinkedIn", 37, "https://www.linkedin.com/in/benjamin-lee" },
                    { 4070, 0, "GitHub", 38, "https://github.com/layla-hassan" },
                    { 4071, 1, "LinkedIn", 38, "https://www.linkedin.com/in/layla-hassan" }
                });

            migrationBuilder.InsertData(
                table: "ProfileProjects",
                columns: new[] { "ProfileId", "ProjectId", "DisplayOrder", "EndDate", "IsFeatured", "Organization", "Role", "StartDate" },
                values: new object[,]
                {
                    { 3, 101, 0, new DateOnly(2020, 7, 1), true, "Cedarline", "Senior Software Engineer", new DateOnly(2020, 1, 1) },
                    { 3, 104, 1, new DateOnly(2020, 7, 1), false, "Maple Systems", "Software Engineer", new DateOnly(2020, 1, 1) },
                    { 4, 102, 0, new DateOnly(2021, 8, 1), true, "Wellnest", "Full-stack Developer", new DateOnly(2021, 2, 1) },
                    { 4, 105, 1, new DateOnly(2021, 8, 1), false, "CarePath", "Application Developer", new DateOnly(2021, 2, 1) },
                    { 5, 103, 0, new DateOnly(2022, 9, 1), true, "Orbit Works", "Backend Engineer", new DateOnly(2022, 3, 1) },
                    { 5, 106, 1, new DateOnly(2022, 9, 1), false, "Copperfield", "Software Developer", new DateOnly(2022, 3, 1) },
                    { 6, 104, 0, new DateOnly(2023, 10, 1), true, "Northwind Studio", "Frontend Engineer", new DateOnly(2023, 4, 1) },
                    { 6, 107, 1, new DateOnly(2023, 10, 1), false, "Brightside Digital", "UI Developer", new DateOnly(2023, 4, 1) },
                    { 7, 105, 0, new DateOnly(2024, 11, 1), true, "Buildcraft", "Developer Experience Engineer", new DateOnly(2024, 5, 1) },
                    { 7, 108, 1, new DateOnly(2024, 11, 1), false, "Pixel Forge", "Software Engineer", new DateOnly(2024, 5, 1) },
                    { 8, 101, 1, new DateOnly(2020, 12, 1), false, "OpenField", "Research Developer", new DateOnly(2020, 6, 1) },
                    { 8, 106, 0, new DateOnly(2020, 12, 1), true, "Terrain Labs", "Data Engineer", new DateOnly(2020, 6, 1) },
                    { 9, 102, 1, new DateOnly(2022, 1, 1), false, "Redwood Apps", "Platform Developer", new DateOnly(2021, 7, 1) },
                    { 9, 107, 0, new DateOnly(2022, 1, 1), true, "Cloud Harbor", "Cloud Engineer", new DateOnly(2021, 7, 1) },
                    { 10, 103, 1, new DateOnly(2023, 2, 1), false, "CityLab", "Web Developer", new DateOnly(2022, 8, 1) },
                    { 10, 108, 0, new DateOnly(2023, 2, 1), true, "Common Ground", "Software Engineer", new DateOnly(2022, 8, 1) },
                    { 11, 101, 0, new DateOnly(2024, 3, 1), true, "Harborlight", "Mobile Engineer", new DateOnly(2023, 9, 1) },
                    { 11, 104, 1, new DateOnly(2024, 3, 1), false, "Tandem Apps", "Junior Developer", new DateOnly(2023, 9, 1) },
                    { 12, 102, 0, new DateOnly(2025, 4, 1), true, "Mosaic Studio", "Application Developer", new DateOnly(2024, 10, 1) },
                    { 12, 105, 1, new DateOnly(2025, 4, 1), false, "Paper Kite", "Frontend Developer", new DateOnly(2024, 10, 1) },
                    { 13, 103, 0, new DateOnly(2021, 5, 1), true, "Relay Labs", "Full-stack Engineer", new DateOnly(2020, 11, 1) },
                    { 13, 106, 1, new DateOnly(2021, 5, 1), false, "Loopline", "Web Engineer", new DateOnly(2020, 11, 1) },
                    { 14, 104, 0, new DateOnly(2022, 6, 1), true, "Lockstep", "Software Engineer", new DateOnly(2021, 12, 1) },
                    { 14, 107, 1, new DateOnly(2022, 6, 1), false, "Evergreen Tech", "Application Developer", new DateOnly(2021, 12, 1) },
                    { 15, 105, 0, new DateOnly(2022, 7, 1), true, "Commonwealth", "Product Engineer", new DateOnly(2022, 1, 1) },
                    { 15, 108, 1, new DateOnly(2022, 7, 1), false, "Ledger House", "Software Developer", new DateOnly(2022, 1, 1) },
                    { 16, 101, 1, new DateOnly(2023, 8, 1), false, "Canyon Software", "UI Engineer", new DateOnly(2023, 2, 1) },
                    { 16, 106, 0, new DateOnly(2023, 8, 1), true, "Kindred Design", "Frontend Engineer", new DateOnly(2023, 2, 1) },
                    { 17, 102, 1, new DateOnly(2024, 9, 1), false, "Signal Peak", "Data Developer", new DateOnly(2024, 3, 1) },
                    { 17, 107, 0, new DateOnly(2024, 9, 1), true, "Granite Data", "Data Platform Developer", new DateOnly(2024, 3, 1) },
                    { 18, 103, 1, new DateOnly(2020, 10, 1), false, "Open Classroom", "Web Developer", new DateOnly(2020, 4, 1) },
                    { 18, 108, 0, new DateOnly(2020, 10, 1), true, "Learnwell", "Software Developer", new DateOnly(2020, 4, 1) },
                    { 19, 101, 0, new DateOnly(2021, 11, 1), true, "Meridian Tools", "Platform Engineer", new DateOnly(2021, 5, 1) },
                    { 19, 104, 1, new DateOnly(2021, 11, 1), false, "Foss Harbor", "Software Engineer", new DateOnly(2021, 5, 1) },
                    { 20, 102, 0, new DateOnly(2022, 12, 1), true, "Block & Bridge", "Full-stack Developer", new DateOnly(2022, 6, 1) },
                    { 20, 105, 1, new DateOnly(2022, 12, 1), false, "Civic Thread", "Application Developer", new DateOnly(2022, 6, 1) },
                    { 21, 103, 0, new DateOnly(2024, 1, 1), true, "Waypoint", "Software Engineer", new DateOnly(2023, 7, 1) },
                    { 21, 106, 1, new DateOnly(2024, 1, 1), false, "Atlas North", "GIS Developer", new DateOnly(2023, 7, 1) },
                    { 22, 104, 0, new DateOnly(2025, 2, 1), true, "Market Street", "Backend Developer", new DateOnly(2024, 8, 1) },
                    { 22, 107, 1, new DateOnly(2025, 2, 1), false, "Shopwell", "Software Engineer", new DateOnly(2024, 8, 1) },
                    { 23, 105, 0, new DateOnly(2021, 3, 1), true, "Farview", "Product Engineer", new DateOnly(2020, 9, 1) },
                    { 23, 108, 1, new DateOnly(2021, 3, 1), false, "Workroom", "Full-stack Developer", new DateOnly(2020, 9, 1) },
                    { 24, 101, 1, new DateOnly(2022, 4, 1), false, "Public Gallery", "Web Developer", new DateOnly(2021, 10, 1) },
                    { 24, 106, 0, new DateOnly(2022, 4, 1), true, "Canvas House", "Software Engineer", new DateOnly(2021, 10, 1) },
                    { 25, 102, 1, new DateOnly(2023, 5, 1), false, "Blue Summit", "Software Engineer", new DateOnly(2022, 11, 1) },
                    { 25, 107, 0, new DateOnly(2023, 5, 1), true, "Tracepoint", "Cloud Developer", new DateOnly(2022, 11, 1) },
                    { 26, 103, 1, new DateOnly(2024, 6, 1), false, "State Digital", "UI Developer", new DateOnly(2023, 12, 1) },
                    { 26, 108, 0, new DateOnly(2024, 6, 1), true, "Civic Bridge", "Frontend Developer", new DateOnly(2023, 12, 1) },
                    { 27, 101, 0, new DateOnly(2024, 7, 1), true, "Harvest Table", "Software Developer", new DateOnly(2024, 1, 1) },
                    { 27, 104, 1, new DateOnly(2024, 7, 1), false, "Local Basket", "Full-stack Developer", new DateOnly(2024, 1, 1) },
                    { 28, 102, 0, new DateOnly(2020, 8, 1), true, "Roamstead", "Full-stack Engineer", new DateOnly(2020, 2, 1) },
                    { 28, 105, 1, new DateOnly(2020, 8, 1), false, "Trailmark", "Web Developer", new DateOnly(2020, 2, 1) },
                    { 29, 103, 0, new DateOnly(2021, 9, 1), true, "Juniper Health", "Software Engineer", new DateOnly(2021, 3, 1) },
                    { 29, 106, 1, new DateOnly(2021, 9, 1), false, "Wellnest", "Backend Developer", new DateOnly(2021, 3, 1) },
                    { 30, 104, 0, new DateOnly(2022, 10, 1), true, "Lingua Loop", "Product Developer", new DateOnly(2022, 4, 1) },
                    { 30, 107, 1, new DateOnly(2022, 10, 1), false, "Wordgarden", "Frontend Developer", new DateOnly(2022, 4, 1) },
                    { 31, 105, 0, new DateOnly(2023, 11, 1), true, "Routecraft", "Backend Engineer", new DateOnly(2023, 5, 1) },
                    { 31, 108, 1, new DateOnly(2023, 11, 1), false, "Freightline", "Software Developer", new DateOnly(2023, 5, 1) },
                    { 32, 101, 1, new DateOnly(2024, 12, 1), false, "Clearpath Finance", "Frontend Engineer", new DateOnly(2024, 6, 1) },
                    { 32, 106, 0, new DateOnly(2024, 12, 1), true, "Pocketwise", "Software Developer", new DateOnly(2024, 6, 1) },
                    { 33, 102, 1, new DateOnly(2021, 1, 1), false, "Summit Apps", "Software Engineer", new DateOnly(2020, 7, 1) },
                    { 33, 107, 0, new DateOnly(2021, 1, 1), true, "Trailmark", "Mobile Developer", new DateOnly(2020, 7, 1) },
                    { 34, 103, 1, new DateOnly(2022, 2, 1), false, "Community Metrics", "Analyst Developer", new DateOnly(2021, 8, 1) },
                    { 34, 108, 0, new DateOnly(2022, 2, 1), true, "Goodworks", "Data Developer", new DateOnly(2021, 8, 1) },
                    { 35, 101, 0, new DateOnly(2023, 3, 1), true, "Signal House", "Software Engineer", new DateOnly(2022, 9, 1) },
                    { 35, 104, 1, new DateOnly(2023, 3, 1), false, "Studio North", "Web Developer", new DateOnly(2022, 9, 1) },
                    { 36, 102, 0, new DateOnly(2024, 4, 1), true, "Open Door Labs", "Frontend Engineer", new DateOnly(2023, 10, 1) },
                    { 36, 105, 1, new DateOnly(2024, 4, 1), false, "Kindred Design", "UI Developer", new DateOnly(2023, 10, 1) },
                    { 37, 103, 0, new DateOnly(2025, 5, 1), true, "Main Street Software", "Full-stack Developer", new DateOnly(2024, 11, 1) },
                    { 37, 106, 1, new DateOnly(2025, 5, 1), false, "Cornerstone Apps", "Software Developer", new DateOnly(2024, 11, 1) },
                    { 38, 104, 0, new DateOnly(2021, 6, 1), true, "Greenline", "Software Engineer", new DateOnly(2020, 12, 1) },
                    { 38, 107, 1, new DateOnly(2021, 6, 1), false, "Earthwise Digital", "Application Developer", new DateOnly(2020, 12, 1) }
                });

            migrationBuilder.InsertData(
                table: "ProfileSkills",
                columns: new[] { "ProfileId", "SkillId", "DisplayOrder" },
                values: new object[,]
                {
                    { 3, 302, 2 },
                    { 3, 303, 3 },
                    { 3, 304, 0 },
                    { 3, 305, 1 },
                    { 3, 306, 4 },
                    { 4, 307, 2 },
                    { 4, 308, 3 },
                    { 4, 309, 4 },
                    { 4, 311, 0 },
                    { 4, 312, 1 },
                    { 5, 301, 0 },
                    { 5, 302, 2 },
                    { 5, 306, 4 },
                    { 5, 313, 3 },
                    { 5, 314, 1 },
                    { 6, 305, 1 },
                    { 6, 308, 3 },
                    { 6, 315, 0 },
                    { 6, 316, 2 },
                    { 6, 317, 4 },
                    { 7, 309, 4 },
                    { 7, 318, 0 },
                    { 7, 319, 1 },
                    { 7, 320, 2 },
                    { 7, 321, 3 },
                    { 8, 307, 2 },
                    { 8, 322, 0 },
                    { 8, 323, 1 },
                    { 8, 324, 3 },
                    { 8, 325, 4 },
                    { 9, 302, 2 },
                    { 9, 303, 3 },
                    { 9, 304, 0 },
                    { 9, 305, 1 },
                    { 9, 306, 4 },
                    { 10, 307, 2 },
                    { 10, 308, 3 },
                    { 10, 309, 4 },
                    { 10, 311, 0 },
                    { 10, 312, 1 },
                    { 11, 301, 0 },
                    { 11, 302, 2 },
                    { 11, 306, 4 },
                    { 11, 313, 3 },
                    { 11, 314, 1 },
                    { 12, 305, 1 },
                    { 12, 308, 3 },
                    { 12, 315, 0 },
                    { 12, 316, 2 },
                    { 12, 317, 4 },
                    { 13, 309, 4 },
                    { 13, 318, 0 },
                    { 13, 319, 1 },
                    { 13, 320, 2 },
                    { 13, 321, 3 },
                    { 14, 307, 2 },
                    { 14, 322, 0 },
                    { 14, 323, 1 },
                    { 14, 324, 3 },
                    { 14, 325, 4 },
                    { 15, 302, 2 },
                    { 15, 303, 3 },
                    { 15, 304, 0 },
                    { 15, 305, 1 },
                    { 15, 306, 4 },
                    { 16, 307, 2 },
                    { 16, 308, 3 },
                    { 16, 309, 4 },
                    { 16, 311, 0 },
                    { 16, 312, 1 },
                    { 17, 301, 0 },
                    { 17, 302, 2 },
                    { 17, 306, 4 },
                    { 17, 313, 3 },
                    { 17, 314, 1 },
                    { 18, 305, 1 },
                    { 18, 308, 3 },
                    { 18, 315, 0 },
                    { 18, 316, 2 },
                    { 18, 317, 4 },
                    { 19, 309, 4 },
                    { 19, 318, 0 },
                    { 19, 319, 1 },
                    { 19, 320, 2 },
                    { 19, 321, 3 },
                    { 20, 307, 2 },
                    { 20, 322, 0 },
                    { 20, 323, 1 },
                    { 20, 324, 3 },
                    { 20, 325, 4 },
                    { 21, 302, 2 },
                    { 21, 303, 3 },
                    { 21, 304, 0 },
                    { 21, 305, 1 },
                    { 21, 306, 4 },
                    { 22, 307, 2 },
                    { 22, 308, 3 },
                    { 22, 309, 4 },
                    { 22, 311, 0 },
                    { 22, 312, 1 },
                    { 23, 301, 0 },
                    { 23, 302, 2 },
                    { 23, 306, 4 },
                    { 23, 313, 3 },
                    { 23, 314, 1 },
                    { 24, 305, 1 },
                    { 24, 308, 3 },
                    { 24, 315, 0 },
                    { 24, 316, 2 },
                    { 24, 317, 4 },
                    { 25, 309, 4 },
                    { 25, 318, 0 },
                    { 25, 319, 1 },
                    { 25, 320, 2 },
                    { 25, 321, 3 },
                    { 26, 307, 2 },
                    { 26, 322, 0 },
                    { 26, 323, 1 },
                    { 26, 324, 3 },
                    { 26, 325, 4 },
                    { 27, 302, 2 },
                    { 27, 303, 3 },
                    { 27, 304, 0 },
                    { 27, 305, 1 },
                    { 27, 306, 4 },
                    { 28, 307, 2 },
                    { 28, 308, 3 },
                    { 28, 309, 4 },
                    { 28, 311, 0 },
                    { 28, 312, 1 },
                    { 29, 301, 0 },
                    { 29, 302, 2 },
                    { 29, 306, 4 },
                    { 29, 313, 3 },
                    { 29, 314, 1 },
                    { 30, 305, 1 },
                    { 30, 308, 3 },
                    { 30, 315, 0 },
                    { 30, 316, 2 },
                    { 30, 317, 4 },
                    { 31, 309, 4 },
                    { 31, 318, 0 },
                    { 31, 319, 1 },
                    { 31, 320, 2 },
                    { 31, 321, 3 },
                    { 32, 307, 2 },
                    { 32, 322, 0 },
                    { 32, 323, 1 },
                    { 32, 324, 3 },
                    { 32, 325, 4 },
                    { 33, 302, 2 },
                    { 33, 303, 3 },
                    { 33, 304, 0 },
                    { 33, 305, 1 },
                    { 33, 306, 4 },
                    { 34, 307, 2 },
                    { 34, 308, 3 },
                    { 34, 309, 4 },
                    { 34, 311, 0 },
                    { 34, 312, 1 },
                    { 35, 301, 0 },
                    { 35, 302, 2 },
                    { 35, 306, 4 },
                    { 35, 313, 3 },
                    { 35, 314, 1 },
                    { 36, 305, 1 },
                    { 36, 308, 3 },
                    { 36, 315, 0 },
                    { 36, 316, 2 },
                    { 36, 317, 4 },
                    { 37, 309, 4 },
                    { 37, 318, 0 },
                    { 37, 319, 1 },
                    { 37, 320, 2 },
                    { 37, 321, 3 },
                    { 38, 307, 2 },
                    { 38, 322, 0 },
                    { 38, 323, 1 },
                    { 38, 324, 3 },
                    { 38, 325, 4 }
                });

            migrationBuilder.InsertData(
                table: "ProjectSkills",
                columns: new[] { "ProjectId", "SkillId" },
                values: new object[,]
                {
                    { 107, 304 },
                    { 107, 305 },
                    { 107, 308 },
                    { 108, 303 },
                    { 108, 304 },
                    { 108, 305 },
                    { 108, 310 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSkills_Name",
                table: "PortfolioSkills",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_Slug",
                table: "PortfolioProjects",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileProjects_ProfileId_IsFeatured_DisplayOrder",
                table: "ProfileProjects",
                columns: new[] { "ProfileId", "IsFeatured", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileProjects_ProjectId",
                table: "ProfileProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileSkills_ProfileId_DisplayOrder",
                table: "ProfileSkills",
                columns: new[] { "ProfileId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileSkills_SkillId",
                table: "ProfileSkills",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileProjects");

            migrationBuilder.DropTable(
                name: "ProfileSkills");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioSkills_Name",
                table: "PortfolioSkills");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioProjects_Slug",
                table: "PortfolioProjects");

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2000);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2001);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2002);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2003);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2004);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2005);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2006);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2007);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2008);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2009);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2010);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2011);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2012);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2013);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2014);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2015);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2016);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2017);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2018);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2019);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2020);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2021);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2022);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2023);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2024);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2025);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2026);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2027);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2028);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2029);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2030);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2031);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2032);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2033);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2034);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2035);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2036);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2037);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2038);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2039);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2040);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2041);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2042);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2043);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2044);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2045);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2046);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2047);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2048);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2049);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2050);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2051);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2052);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2053);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2054);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2055);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2056);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2057);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2058);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2059);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2060);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2061);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2062);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2063);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2064);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2065);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2066);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2067);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2068);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2069);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2070);

            migrationBuilder.DeleteData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2071);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 313);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 314);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 315);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 316);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 317);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 318);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 319);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 320);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 321);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 322);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 323);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 324);

            migrationBuilder.DeleteData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 325);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4000);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4001);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4002);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4003);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4004);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4005);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4006);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4007);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4008);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4009);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4010);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4011);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4012);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4013);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4014);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4015);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4016);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4017);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4018);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4019);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4020);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4021);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4022);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4023);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4024);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4025);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4026);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4027);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4028);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4029);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4030);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4031);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4032);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4033);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4034);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4035);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4036);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4037);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4038);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4039);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4040);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4041);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4042);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4043);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4044);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4045);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4046);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4047);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4048);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4049);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4050);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4051);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4052);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4053);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4054);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4055);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4056);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4057);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4058);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4059);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4060);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4061);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4062);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4063);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4064);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4065);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4066);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4067);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4068);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4069);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4070);

            migrationBuilder.DeleteData(
                table: "PortfolioSocialLinks",
                keyColumn: "Id",
                keyValue: 4071);

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 102, 304 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 102, 305 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 103, 307 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 103, 308 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 103, 309 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 104, 304 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 104, 305 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 105, 304 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 105, 305 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 105, 308 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 106, 301 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 106, 302 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 106, 303 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 106, 306 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 107, 304 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 107, 305 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 107, 308 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 108, 303 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 108, 304 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 108, 305 });

            migrationBuilder.DeleteData(
                table: "ProjectSkills",
                keyColumns: new[] { "ProjectId", "SkillId" },
                keyValues: new object[] { 108, 310 });

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "PortfolioProfiles",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "PortfolioSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "PortfolioSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "PortfolioProjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "PortfolioProjects",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "PortfolioProjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Organization",
                table: "PortfolioProjects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "PortfolioProjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "PortfolioProjects",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "PortfolioProjects",
                type: "date",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "DisplayOrder", "EndDate", "IsFeatured", "Organization", "ProfileId", "Role", "StartDate" },
                values: new object[] { 0, new DateOnly(2024, 8, 1), true, "Independent project", 1, "Lead Engineer", new DateOnly(2024, 2, 1) });

            migrationBuilder.UpdateData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "DisplayOrder", "EndDate", "IsFeatured", "Organization", "ProfileId", "Role", "StartDate" },
                values: new object[] { 1, new DateOnly(2023, 12, 1), true, "Northstar Labs", 1, "Backend and UI Engineer", new DateOnly(2023, 3, 1) });

            migrationBuilder.UpdateData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "DisplayOrder", "EndDate", "IsFeatured", "Organization", "ProfileId", "Role", "StartDate" },
                values: new object[] { 2, new DateOnly(2022, 11, 1), false, "Civic Works", 1, "Full-stack Engineer", new DateOnly(2022, 4, 1) });

            migrationBuilder.UpdateData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "DisplayOrder", "EndDate", "IsFeatured", "Organization", "ProfileId", "Role", "StartDate" },
                values: new object[] { 0, new DateOnly(2024, 10, 1), true, "Brightside Studio", 2, "Product Engineer", new DateOnly(2024, 1, 1) });

            migrationBuilder.UpdateData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "DisplayOrder", "EndDate", "IsFeatured", "Organization", "ProfileId", "Role", "StartDate" },
                values: new object[] { 1, new DateOnly(2023, 11, 1), true, "Cloudline", 2, "Full-stack Developer", new DateOnly(2023, 2, 1) });

            migrationBuilder.UpdateData(
                table: "PortfolioProjects",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "DisplayOrder", "EndDate", "IsFeatured", "Organization", "ProfileId", "Role", "StartDate" },
                values: new object[] { 2, new DateOnly(2022, 12, 1), false, "Neighbourhood Network", 2, "Volunteer Developer", new DateOnly(2022, 5, 1) });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "Category", "DisplayOrder", "ProfileId" },
                values: new object[] { "Backend", 0, 1 });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 302,
                columns: new[] { "DisplayOrder", "ProfileId" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 303,
                columns: new[] { "DisplayOrder", "ProfileId" },
                values: new object[] { 0, 1 });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 304,
                columns: new[] { "Category", "DisplayOrder", "ProfileId" },
                values: new object[] { "Frontend", 0, 1 });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 305,
                columns: new[] { "DisplayOrder", "ProfileId" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 306,
                columns: new[] { "DisplayOrder", "ProfileId" },
                values: new object[] { 0, 1 });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 307,
                columns: new[] { "Category", "DisplayOrder", "Name", "ProfileId" },
                values: new object[] { "Languages", 0, "TypeScript", 2 });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 308,
                columns: new[] { "Category", "DisplayOrder", "Name", "ProfileId" },
                values: new object[] { "Frontend", 0, "React", 2 });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 309,
                columns: new[] { "Category", "DisplayOrder", "Name", "ProfileId" },
                values: new object[] { "Backend", 0, "Node.js", 2 });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 310,
                columns: new[] { "Category", "DisplayOrder", "Name", "ProfileId" },
                values: new object[] { "Data", 0, "PostgreSQL", 2 });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 311,
                columns: new[] { "Category", "DisplayOrder", "Name", "ProfileId" },
                values: new object[] { "Cloud", 0, "Docker", 2 });

            migrationBuilder.UpdateData(
                table: "PortfolioSkills",
                keyColumn: "Id",
                keyValue: 312,
                columns: new[] { "Category", "DisplayOrder", "Name", "ProfileId" },
                values: new object[] { "Design", 0, "Accessibility", 2 });

            migrationBuilder.InsertData(
                table: "ProjectSkills",
                columns: new[] { "ProjectId", "SkillId" },
                values: new object[,]
                {
                    { 101, 301 },
                    { 101, 302 },
                    { 102, 306 },
                    { 103, 303 },
                    { 103, 305 },
                    { 104, 307 },
                    { 104, 308 },
                    { 104, 312 },
                    { 105, 307 },
                    { 105, 309 },
                    { 105, 311 },
                    { 106, 307 },
                    { 106, 308 },
                    { 106, 312 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSkills_ProfileId_Category_DisplayOrder",
                table: "PortfolioSkills",
                columns: new[] { "ProfileId", "Category", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSkills_ProfileId_Name",
                table: "PortfolioSkills",
                columns: new[] { "ProfileId", "Name" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PortfolioSkills_DisplayOrder",
                table: "PortfolioSkills",
                sql: "[DisplayOrder] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_ProfileId_IsFeatured_DisplayOrder",
                table: "PortfolioProjects",
                columns: new[] { "ProfileId", "IsFeatured", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_ProfileId_Slug",
                table: "PortfolioProjects",
                columns: new[] { "ProfileId", "Slug" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PortfolioProjects_DateRange",
                table: "PortfolioProjects",
                sql: "[EndDate] IS NULL OR [StartDate] IS NULL OR [EndDate] >= [StartDate]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PortfolioProjects_DisplayOrder",
                table: "PortfolioProjects",
                sql: "[DisplayOrder] >= 0");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioProjects_PortfolioProfiles_ProfileId",
                table: "PortfolioProjects",
                column: "ProfileId",
                principalTable: "PortfolioProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioSkills_PortfolioProfiles_ProfileId",
                table: "PortfolioSkills",
                column: "ProfileId",
                principalTable: "PortfolioProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
