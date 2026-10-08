using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniProject.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialPortfolioSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PortfolioProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Headline = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ResumeUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioExperiences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioExperiences", x => x.Id);
                    table.CheckConstraint("CK_PortfolioExperiences_CurrentHasNoEndDate", "[IsCurrent] = 0 OR [EndDate] IS NULL");
                    table.CheckConstraint("CK_PortfolioExperiences_DateRange", "[EndDate] IS NULL OR [EndDate] >= [StartDate]");
                    table.CheckConstraint("CK_PortfolioExperiences_DisplayOrder", "[DisplayOrder] >= 0");
                    table.ForeignKey(
                        name: "FK_PortfolioExperiences_PortfolioProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "PortfolioProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Organization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DemoUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioProjects", x => x.Id);
                    table.CheckConstraint("CK_PortfolioProjects_DateRange", "[EndDate] IS NULL OR [StartDate] IS NULL OR [EndDate] >= [StartDate]");
                    table.CheckConstraint("CK_PortfolioProjects_DisplayOrder", "[DisplayOrder] >= 0");
                    table.ForeignKey(
                        name: "FK_PortfolioProjects_PortfolioProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "PortfolioProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioSkills", x => x.Id);
                    table.CheckConstraint("CK_PortfolioSkills_DisplayOrder", "[DisplayOrder] >= 0");
                    table.ForeignKey(
                        name: "FK_PortfolioSkills_PortfolioProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "PortfolioProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioSocialLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioSocialLinks", x => x.Id);
                    table.CheckConstraint("CK_PortfolioSocialLinks_DisplayOrder", "[DisplayOrder] >= 0");
                    table.ForeignKey(
                        name: "FK_PortfolioSocialLinks_PortfolioProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "PortfolioProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSkills",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSkills", x => new { x.ProjectId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_ProjectSkills_PortfolioProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PortfolioProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectSkills_PortfolioSkills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "PortfolioSkills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioExperiences_ProfileId_DisplayOrder",
                table: "PortfolioExperiences",
                columns: new[] { "ProfileId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_ProfileId_IsFeatured_DisplayOrder",
                table: "PortfolioProjects",
                columns: new[] { "ProfileId", "IsFeatured", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_ProfileId_Slug",
                table: "PortfolioProjects",
                columns: new[] { "ProfileId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSkills_ProfileId_Category_DisplayOrder",
                table: "PortfolioSkills",
                columns: new[] { "ProfileId", "Category", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSkills_ProfileId_Name",
                table: "PortfolioSkills",
                columns: new[] { "ProfileId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSocialLinks_ProfileId_DisplayOrder",
                table: "PortfolioSocialLinks",
                columns: new[] { "ProfileId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSocialLinks_ProfileId_Label",
                table: "PortfolioSocialLinks",
                columns: new[] { "ProfileId", "Label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSkills_SkillId",
                table: "ProjectSkills",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioExperiences");

            migrationBuilder.DropTable(
                name: "PortfolioSocialLinks");

            migrationBuilder.DropTable(
                name: "ProjectSkills");

            migrationBuilder.DropTable(
                name: "PortfolioProjects");

            migrationBuilder.DropTable(
                name: "PortfolioSkills");

            migrationBuilder.DropTable(
                name: "PortfolioProfiles");
        }
    }
}
