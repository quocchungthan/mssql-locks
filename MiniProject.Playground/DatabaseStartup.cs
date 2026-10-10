using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniProject.ComplexLogicInMiddle;
using MiniProject.Migrations;

namespace MiniProject.Playground;

internal static class DatabaseStartup
{
    // One identity per database, independent of the requested count and ordinary profile data.
    private const string SeedKey = "containerized-talent-bulk-v1";

    public static async Task RunAsync(WebApplication app)
    {
        var migrate = app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
        var requestedCount = app.Configuration.GetValue<int?>("Database:BulkSeedProfileCount");
        if (requestedCount is not null &&
            requestedCount is < 1 or > TalentDataGenerator.MaxProfilesPerRun)
        {
            throw new InvalidOperationException(
                $"Database:BulkSeedProfileCount must be between 1 and {TalentDataGenerator.MaxProfilesPerRun}.");
        }

        if (requestedCount is not null && !migrate)
        {
            throw new InvalidOperationException("Startup bulk seeding requires Database:ApplyMigrationsOnStartup=true.");
        }

        if (!migrate)
        {
            return;
        }

        // Host signal handling is not yet active: HTTP/hosted services start only after this returns.
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(app.Lifetime.ApplicationStopping);
        ConsoleCancelEventHandler onCancel = (_, args) =>
        {
            args.Cancel = true;
            cancellation.Cancel();
        };
        Console.CancelKeyPress += onCancel;
        using var termination = OperatingSystem.IsWindows()
            ? null
            : PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
            {
                context.Cancel = true;
                cancellation.Cancel();
            });

        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<MiniDbContext>();
            app.Logger.LogInformation("Applying database migrations before HTTP startup.");
            using (var migrationTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation.Token))
            {
                migrationTimeout.CancelAfter(TimeSpan.FromMinutes(5));
                await database.Database.MigrateAsync(migrationTimeout.Token);
            }

            app.Logger.LogInformation("Database migrations completed.");
            if (requestedCount is int count)
            {
                var generator = scope.ServiceProvider.GetRequiredService<ITalentDataGenerator>();
                await SeedAsync(database, generator, count, app.Logger, cancellation.Token);
            }

            cancellation.Token.ThrowIfCancellationRequested();
            app.Logger.LogInformation("Database startup completed; starting HTTP server.");
        }
        catch (OperationCanceledException)
        {
            app.Logger.LogWarning("Database startup cancelled; HTTP was not started. Bulk seed resumes from committed progress.");
            throw;
        }
        catch (Exception)
        {
            // Do not log SQL/connection details here; propagate failure, never serve a partial seed.
            app.Logger.LogError("Database startup failed; HTTP was not started. Bulk seed resumes from committed progress.");
            throw;
        }
        finally
        {
            Console.CancelKeyPress -= onCancel;
        }
    }

    private static async Task SeedAsync(
        MiniDbContext database,
        ITalentDataGenerator generator,
        int requestedCount,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Current UseSqlServer has no retries. Prevent a future retry strategy from replaying
        // non-idempotent generator SQL independently of its progress transaction.
        if (database.Database.CreateExecutionStrategy().RetriesOnFailure)
        {
            throw new InvalidOperationException("Startup bulk seeding requires SQL command retries to be disabled.");
        }

        using var suppressed = EfCommandLogging.Suppress();
        var stopwatch = Stopwatch.StartNew();
        logger.LogInformation(
            "Startup bulk seed {SeedKey}: requested {Count} profiles, 8 skills and 4 experiences each; SQL logging muted. Waiting for database lock per chunk.",
            SeedKey, requestedCount);

        var previousTimeout = database.Database.GetCommandTimeout();
        database.Database.SetCommandTimeout(TimeSpan.FromMinutes(11));
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int committed;
                bool skipped;
                // Disposal closes the EF-opened connection (including generator temp tables).
                // Lock, DDL/marker creation, rows, and progress belong to this same transaction.
                await using (var transaction = await database.Database.BeginTransactionAsync(cancellationToken))
                {
                    await database.Database.ExecuteSqlRawAsync("""
                        DECLARE @result int;
                        EXEC @result = sys.sp_getapplock
                            @Resource = N'mssql-locks:startup-bulk-seed',
                            @LockMode = N'Exclusive',
                            @LockOwner = N'Transaction',
                            @LockTimeout = 600000,
                            @DbPrincipal = N'public';
                        IF @result < 0
                            THROW 51000, 'Could not acquire startup bulk seed lock.', 1;

                        IF OBJECT_ID(N'dbo.StartupBulkSeedRuns', N'U') IS NULL
                        BEGIN
                            CREATE TABLE dbo.StartupBulkSeedRuns (
                                SeedKey nvarchar(100) NOT NULL PRIMARY KEY,
                                ExpectedProfiles int NOT NULL,
                                CommittedProfiles int NOT NULL,
                                CompletedAtUtc datetime2 NULL,
                                CONSTRAINT CK_StartupBulkSeedRuns_Progress CHECK (
                                    ExpectedProfiles BETWEEN 1 AND 1000000 AND
                                    CommittedProfiles BETWEEN 0 AND ExpectedProfiles),
                                CONSTRAINT CK_StartupBulkSeedRuns_Completed CHECK (
                                    (CommittedProfiles = ExpectedProfiles AND CompletedAtUtc IS NOT NULL) OR
                                    (CommittedProfiles < ExpectedProfiles AND CompletedAtUtc IS NULL))
                            );
                        END;
                        """, cancellationToken);

                    // Unmapped, startup-only operational state: not part of the portfolio EF model.
                    using var command = database.Database.GetDbConnection().CreateCommand();
                    command.Transaction = transaction.GetDbTransaction();
                    command.CommandTimeout = 660;
                    command.CommandText = """
                        SELECT ExpectedProfiles, CommittedProfiles
                        FROM dbo.StartupBulkSeedRuns WHERE SeedKey = @key;
                        """;
                    var key = command.CreateParameter();
                    key.ParameterName = "@key";
                    key.DbType = DbType.String;
                    key.Size = 100;
                    key.Value = SeedKey;
                    command.Parameters.Add(key);
                    int? expected = null;
                    committed = 0;
                    await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            expected = reader.GetInt32(0);
                            committed = reader.GetInt32(1);
                        }
                    }

                    if (expected is int storedCount && storedCount != requestedCount)
                    {
                        throw new InvalidOperationException(
                            $"Startup seed {SeedKey} already expects {storedCount} profiles; requested {requestedCount}. " +
                            "Restore the original configured count; changing it must not create another seed.");
                    }

                    if (expected is null)
                    {
                        await database.Database.ExecuteSqlInterpolatedAsync($"""
                            INSERT dbo.StartupBulkSeedRuns (SeedKey, ExpectedProfiles, CommittedProfiles)
                            VALUES ({SeedKey}, {requestedCount}, 0);
                            """, cancellationToken);
                    }

                    skipped = committed == requestedCount;
                    if (!skipped)
                    {
                        var chunk = Math.Min(TalentDataGenerator.ChunkSize, requestedCount - committed);
                        var inserted = await generator.GenerateAsync(chunk, null, cancellationToken);
                        if (inserted != chunk)
                        {
                            throw new InvalidOperationException("Bulk generator returned an unexpected profile count.");
                        }

                        committed += inserted;
                        var updated = await database.Database.ExecuteSqlInterpolatedAsync($"""
                            UPDATE dbo.StartupBulkSeedRuns
                            SET CommittedProfiles = {committed},
                                CompletedAtUtc = CASE WHEN {committed} = ExpectedProfiles
                                    THEN SYSUTCDATETIME() ELSE NULL END
                            WHERE SeedKey = {SeedKey};
                            """, cancellationToken);
                        if (updated != 1)
                        {
                            throw new InvalidOperationException("Startup bulk seed progress update failed.");
                        }
                    }

                    await transaction.CommitAsync(cancellationToken);
                }

                if (skipped)
                {
                    logger.LogInformation(
                        "Startup bulk seed {SeedKey} skipped: completed {Count}/{Count} profiles in database; elapsed {Seconds:F1}s this startup.",
                        SeedKey, requestedCount, requestedCount, stopwatch.Elapsed.TotalSeconds);
                    return;
                }

                logger.LogInformation(
                    "Startup bulk seed {SeedKey}: committed {Progress}/{Count} profiles; elapsed {Seconds:F1}s this startup.",
                    SeedKey, committed, requestedCount, stopwatch.Elapsed.TotalSeconds);
                if (committed == requestedCount)
                {
                    logger.LogInformation("Startup bulk seed {SeedKey} completed; elapsed {Seconds:F1}s this startup.",
                        SeedKey, stopwatch.Elapsed.TotalSeconds);
                    return;
                }
            }
        }
        finally
        {
            database.Database.SetCommandTimeout(previousTimeout);
        }
    }
}
