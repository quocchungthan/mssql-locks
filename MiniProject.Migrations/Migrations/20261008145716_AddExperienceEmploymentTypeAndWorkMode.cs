using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniProject.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddExperienceEmploymentTypeAndWorkMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                table: "PortfolioExperiences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "FullTime");

            migrationBuilder.AddColumn<string>(
                name: "WorkMode",
                table: "PortfolioExperiences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "OnSite");

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 204,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2000,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2001,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2002,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2003,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2004,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2005,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2006,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2007,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2008,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2009,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2010,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2011,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2012,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2013,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2014,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2015,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2016,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2017,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2018,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2019,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2020,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2021,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2022,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2023,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2024,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2025,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2026,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2027,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2028,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2029,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2030,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2031,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2032,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2033,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2034,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2035,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2036,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2037,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2038,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2039,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2040,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2041,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2042,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2043,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2044,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2045,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2046,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2047,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2048,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2049,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2050,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2051,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2052,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2053,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2054,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2055,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2056,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2057,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2058,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2059,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2060,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2061,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2062,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2063,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2064,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2065,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2066,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Freelance", "OnSite" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2067,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2068,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Internship", "Hybrid" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2069,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "PartTime", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2070,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "FullTime", "Remote" });

            migrationBuilder.UpdateData(
                table: "PortfolioExperiences",
                keyColumn: "Id",
                keyValue: 2071,
                columns: new[] { "EmploymentType", "WorkMode" },
                values: new object[] { "Contract", "OnSite" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "PortfolioExperiences");

            migrationBuilder.DropColumn(
                name: "WorkMode",
                table: "PortfolioExperiences");
        }
    }
}
