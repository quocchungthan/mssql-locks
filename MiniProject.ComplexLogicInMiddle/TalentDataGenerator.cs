using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniProject.Migrations;
using MiniProject.Migrations.Entities;

namespace MiniProject.ComplexLogicInMiddle;

public interface ITalentDataGenerator
{
    Task<int> GenerateAsync(int profileCount, Action<int>? onProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Simulates a messy ATS/CSV import: the same city spelled with and without Vietnamese diacritics,
/// names with accents, and leading/trailing spaces on locations and companies. That dirt is the usual
/// excuse for putting Trim() and accent-insensitive collations inside report queries.
/// Rows are generated set-based in T-SQL so millions of fan-out rows can be produced in seconds.
/// </summary>
public sealed class TalentDataGenerator : ITalentDataGenerator
{
    public const int MaxProfilesPerRun = 1_000_000;
    public const int ChunkSize = 50_000;
    private const int SkillsPerProfile = 8;
    private const int ExperiencesPerProfile = 4;

    private static readonly string[] FirstNames =
        ["An", "Bình", "Chi", "Dũng", "Giang", "Hòa", "Khánh", "Linh", "Minh", "Nam", "Phương", "Quang",
         "Thảo", "Tuấn", "Vy", "Alex", "Jordan", "Taylor", "Morgan", "Sam", "Riley", "Casey"];
    private static readonly string[] LastNames =
        ["Nguyễn", "Nguyen", "Trần", "Tran", "Lê", "Phạm", "Hoàng", "Võ", "Đặng", "Bùi", "Smith", "Garcia",
         "Kim", "Patel", "Müller", "Rossi", "Silva", "Tanaka"];
    private static readonly string[] Locations =
        ["Hồ Chí Minh, Việt Nam", "Ho Chi Minh City, Vietnam", "TP. HCM, Vietnam", "Hà Nội, Việt Nam",
         "Hanoi, Vietnam", "Đà Nẵng, Việt Nam", "Da Nang, Vietnam", "Singapore", "Berlin, Germany",
         "London, United Kingdom", "Toronto, Canada", "Austin, United States", "Sydney, Australia",
         "Bangalore, India", "Tokyo, Japan", "Remote"];
    private static readonly string[] Companies =
        ["Northstar Labs", "Civic Works", "Brightside Studio", "Cloudline", "Acme Payments", "Lotus Logistics",
         "Mekong Data", "Harbor Health", "Pixel Forge", "Bluebird Retail", "Atlas Mobility", "Kite Analytics",
         "Sài Gòn Fintech", "Saigon Fintech", "Evergreen Energy", "Orbit Media", "Granite Security",
         "Phương Nam Software", "Hanoi Cloud", "Đông Á Commerce"];
    private static readonly string[] JobTitles =
        ["Software Engineer", "Senior Software Engineer", "Backend Developer", "Frontend Developer",
         "Full-stack Developer", "Data Engineer", "DevOps Engineer", "QA Engineer", "Tech Lead",
         "Engineering Manager", "Mobile Developer", "Solutions Architect"];
    private static readonly string[] Padding = ["", " ", "  ", "   "];

    private static readonly string ChunkSql = BuildChunkSql();

    private readonly MiniDbContext _dbContext;

    public TalentDataGenerator(MiniDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> GenerateAsync(
        int profileCount,
        Action<int>? onProgress,
        CancellationToken cancellationToken)
    {
        if (profileCount is < 1 or > MaxProfilesPerRun)
        {
            throw new ArgumentOutOfRangeException(
                nameof(profileCount), $"Profile count must be between 1 and {MaxProfilesPerRun}.");
        }

        if (await _dbContext.Skills.CountAsync(cancellationToken) < SkillsPerProfile)
        {
            throw new InvalidOperationException("Seed skills first; not enough skills exist to generate profiles.");
        }

        var previousTimeout = _dbContext.Database.GetCommandTimeout();
        _dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
        try
        {
            var inserted = 0;
            while (inserted < profileCount)
            {
                var chunk = Math.Min(ChunkSize, profileCount - inserted);
                await _dbContext.Database.ExecuteSqlRawAsync(
                    ChunkSql,
                    [
                        new SqlParameter("@count", chunk),
                        new SqlParameter("@seed", Random.Shared.Next()),
                        new SqlParameter("@skillsPerProfile", SkillsPerProfile),
                        new SqlParameter("@experiencesPerProfile", ExperiencesPerProfile)
                    ],
                    cancellationToken);
                inserted += chunk;
                onProgress?.Invoke(inserted);
            }

            return inserted;
        }
        finally
        {
            _dbContext.Database.SetCommandTimeout(previousTimeout);
        }
    }

    private static string BuildChunkSql()
    {
        // Lookup values are compile-time constants (not user input); only counts/seed are parameters.
        static string Values(string[] values) => string.Join(
            ",",
            values.Select((value, index) => $"({index},N'{value.Replace("'", "''")}')"));

        // Deterministic per-row pseudo-random number: MD5(seed:keys:salt) -> 0..2^32-1.
        static string Hash(string keys, string salt) =>
            $"CAST(SUBSTRING(HASHBYTES('MD5', CONCAT(@seed, ':', {keys}, ':{salt}')), 1, 4) AS bigint)";

        var employmentTypes = Enum.GetNames<EmploymentType>();
        var workModes = Enum.GetNames<WorkMode>();

        var sql = new StringBuilder();
        sql.AppendLine("SET NOCOUNT ON;");
        foreach (var (table, values) in new[]
                 {
                     ("#First", FirstNames), ("#Last", LastNames), ("#Loc", Locations), ("#Company", Companies),
                     ("#Title", JobTitles), ("#Pad", Padding), ("#Employment", employmentTypes), ("#Mode", workModes)
                 })
        {
            sql.AppendLine($"CREATE TABLE {table} (I int PRIMARY KEY, V nvarchar(200) NOT NULL);");
            sql.AppendLine($"INSERT {table} (I, V) VALUES {Values(values)};");
        }

        sql.AppendLine($"""
            CREATE TABLE #New (Id int PRIMARY KEY);
            DECLARE @today date = CAST(SYSUTCDATETIME() AS date);

            WITH Numbers AS (
                SELECT TOP (@count) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
                FROM sys.all_columns AS a CROSS JOIN sys.all_columns AS b)
            INSERT INTO PortfolioProfiles (DisplayName, Headline, Location, ContactEmail)
            OUTPUT inserted.Id INTO #New (Id)
            SELECT f.V + N' ' + l.V, t.V, p1.V + loc.V + p2.V, CONCAT(N'talent.', NEWID(), N'@example.com')
            FROM Numbers AS n
            JOIN #First AS f ON f.I = {Hash("n.N", "f")} % {FirstNames.Length}
            JOIN #Last AS l ON l.I = {Hash("n.N", "l")} % {LastNames.Length}
            JOIN #Title AS t ON t.I = {Hash("n.N", "t")} % {JobTitles.Length}
            JOIN #Loc AS loc ON loc.I = {Hash("n.N", "loc")} % {Locations.Length}
            JOIN #Pad AS p1 ON p1.I = {Hash("n.N", "p1")} % {Padding.Length}
            JOIN #Pad AS p2 ON p2.I = {Hash("n.N", "p2")} % {Padding.Length};

            INSERT INTO ProfileSkills (ProfileId, SkillId, DisplayOrder)
            SELECT ranked.ProfileId, ranked.SkillId, ranked.Rank - 1
            FROM (
                SELECT np.Id AS ProfileId, s.Id AS SkillId,
                       ROW_NUMBER() OVER (PARTITION BY np.Id ORDER BY {Hash("np.Id, ':', s.Id", "s")}) AS Rank
                FROM #New AS np CROSS JOIN PortfolioSkills AS s) AS ranked
            WHERE ranked.Rank <= @skillsPerProfile;

            WITH Slots AS (
                SELECT TOP (@experiencesPerProfile) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS K
                FROM sys.all_columns)
            INSERT INTO PortfolioExperiences
                (ProfileId, Company, JobTitle, EmploymentType, WorkMode, StartDate, EndDate, IsCurrent, DisplayOrder)
            SELECT np.Id, p1.V + c.V + p2.V, t.V, e.V, m.V,
                   DATEADD(month, -(slot.K * 24 + 6 + CAST({Hash("np.Id, ':', slot.K", "d")} % 18 AS int)), @today),
                   CASE WHEN slot.K = 0 THEN NULL ELSE DATEADD(month, -(slot.K * 24), @today) END,
                   CASE WHEN slot.K = 0 THEN 1 ELSE 0 END,
                   slot.K
            FROM #New AS np
            CROSS JOIN Slots AS slot
            JOIN #Company AS c ON c.I = {Hash("np.Id, ':', slot.K", "c")} % {Companies.Length}
            JOIN #Title AS t ON t.I = {Hash("np.Id, ':', slot.K", "t")} % {JobTitles.Length}
            JOIN #Employment AS e ON e.I = {Hash("np.Id, ':', slot.K", "e")} % {employmentTypes.Length}
            JOIN #Mode AS m ON m.I = {Hash("np.Id, ':', slot.K", "m")} % {workModes.Length}
            JOIN #Pad AS p1 ON p1.I = {Hash("np.Id, ':', slot.K", "p1")} % {Padding.Length}
            JOIN #Pad AS p2 ON p2.I = {Hash("np.Id, ':', slot.K", "p2")} % {Padding.Length};
            """);
        return sql.ToString();
    }
}
