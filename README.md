# Learning MSSQL and dotnet 10
All the commands I already written inside `./tools/` directory, we execute them from here, their name already tell what it means.

# setup env
As a typical solution that runs with docker: fill the `.env` file at the same level of compose file, then just up with `--detach` flag.

# run the playground
Start the database, then run `dotnet run --project MiniProject.Playground`. The Playground serves:

- `GET /health` — reports whether the database is reachable.
- `GET /api/portfolios/{profileId}` — returns the portfolio profile and its projects, experience, skills, and social links by profile ID.
- `GET /api/portfolios/{profileId}/avatar` — returns a deterministic, generated pixel avatar saved on the profile record.
- `GET /api/projects/{slug}` — returns project details, linked skills, and profiles.
- `GET /api/search?q={query}&types=profiles,projects,skills,links` — searches the selected directory result types; omit `q` to browse.
- `GET /api/insights/facets` — distinct categories, locations, employment types and work modes for the talent market filters.
- `GET /api/insights/talent-market?q=&category=&location=&employmentType=&workMode=&page=&pageSize=` — distinct skill × location × company × role × contract × work-mode segments, ranked by candidate count.
- `/search` — browse and filter profiles, projects, skills, and links.
- `/insights` — talent market explorer with filters, paging, and query diagnostics.
- `/profile/{profileId}` — renders a profile and links to its projects and skills.
- `/project/{slug}` — renders project details and links to related profiles and skills.

The terminal also accepts `help`, `seed-bulk [count]` and `exit` while the API is running. EF Core database command logs include execution durations.

## Standalone containerized performance lab

The original `docker-compose.yml` is unchanged. This **separate** Compose project runs both SQL Server 2025 and .NET 10 inside Linux containers, without publishing any SQL port.

Prerequisites: Docker Desktop's Linux engine must be running (with sufficient resources for SQL Server), port `5080` must be free, and the root `.env` must contain a valid SQL Server `SA_PASSWORD`. Single-quote the value in `.env` if it contains `$`, so Compose does not interpret parts of the password as variable references; the existing host `.env` reader also supports single-quoted values. Keep the password unchanged when reusing an initialized volume. Run from the repository root:

```powershell
docker compose -f docker-compose.containerized.yml up --build
```

This one command builds the app, creates the isolated network/SQL volume, waits for SQL's authenticated `sqlcmd` health check, then starts the app. Before serving HTTP, the app awaits EF Core `Database.MigrateAsync`: it creates `PortfolioDb` if absent and applies the checked-in migrations, including schema and sample seed data. It then awaits a **one-time bulk seed of 500,000 additional profiles** (4,000,000 skill assignments and 2,000,000 experiences) before starting HTTP or the background console. This is not a target total of 500,000: migration samples and any existing ordinary data remain in addition. The referenced migrations assembly is included in the published app; no EF CLI or migration script is needed at runtime.

- Only `http://localhost:5080` is published, bound to host loopback. The app listens on `0.0.0.0:8080` inside its container.
- SQL is reached only through the Compose network as `mssql:1433`. The new project uses its own generated container names, network, and `mssql-locks-containerized_sql-data` volume; it does not use the original `mssql` container or `mssql-data` volume. Do not merge the two Compose files.
- `SA_PASSWORD` is supplied at runtime to both containers, never baked into the image. The app injects it into the password-free `ConnectionStrings__Default` setting. Environment settings override `appsettings.json`; explicit command-line settings take precedence over environment settings.
- `Database:ApplyMigrationsOnStartup` and `Database:BulkSeedProfileCount=500000` are opt-in settings enabled in this Compose file. A configured bulk count requires migrations enabled and must be in `1..TalentDataGenerator.MaxProfilesPerRun` (currently 1,000,000); omitting it disables startup bulk generation. Default noncontainer behavior remains unchanged. Any migration or seed failure aborts startup with a nonzero process exit; there is no fallback or automatic restart loop. Migrations have a five-minute cancellation deadline; bulk seeding has no overall deadline, uses the existing generator's ten-minute per-command limit, and waits at most ten minutes for each database seed lock. Connection timeout is 30 seconds. Normal EF lab-query timeouts are not increased.
- Restarts check migration history and apply only pending migrations, then skip the completed bulk seed. Existing data and seed progress persist in the SQL volume. Changing `SA_PASSWORD` in `.env` does **not** reset the password of an already-initialized SQL volume.
- `PerformanceLab:Enabled` is enabled here. The same problem-only lab routes and existing EF command SQL/timing logs are used—no benchmark runner, new diagnostic tooling, optimized path, artificial delay, or automatic workload after seeding.
- The Docker build uses the .NET 10 SDK and the repository's EF Core `10.0.12` packages. The final ASP.NET runtime image runs `dotnet MiniProject.Playground.dll` rather than `dotnet run`: this serves the same application without shipping the SDK/source or rebuilding at startup. Published `appsettings.json`, `wwwroot`, and migrations are included; `.env`, captures, logs, and build artifacts are excluded from the build context.

In another PowerShell terminal:

```powershell
docker compose -f docker-compose.containerized.yml logs -f app
Invoke-RestMethod http://localhost:5080/health
Invoke-RestMethod 'http://localhost:5080/api/lab/shortlist?count=2'
```

The foreground `up` already shows container logs. Both services use Docker `json-file` rotation (`10m`, three files). The shortlist request should produce the existing EF SQL command/duration output in the app logs.

**Initial startup can be long and needs substantial SQL disk, transaction-log, CPU, and memory resources.** `/health` and all other HTTP routes remain unavailable until seeding completes. Logs report committed progress and elapsed time, then completion (or a completed-seed skip on later startups). Verbose seed SQL is muted using the existing `EfCommandLogging.Suppress`; normal EF lab-query SQL/timing logs remain enabled. No requests are issued automatically. In particular, **do not invoke `/api/lab/directory-config` on this 500,000-profile dataset**: its legacy path fully loads profiles and related data before filtering and can exhaust application memory.

No interactive terminal is required: after startup completes, the default container has redirected stdin, so the existing interactive console returns without stopping HTTP. The existing console `seed-bulk [count]` remains independent and additive; it neither reads nor updates the startup seed marker. Do not run it blindly against this already-large database. No public seed endpoint is added.

### Database-backed once-only seed and interrupted startup

- `dbo.StartupBulkSeedRuns` is a small **startup-only operational table**, created transactionally after migrations. It is deliberately outside the portfolio EF model/migration snapshot: it tracks container initialization, not portfolio domain data, and is only created when startup seeding is opted in. Its primary key is the fixed `containerized-talent-bulk-v1` identity, with expected count, committed count, and completion timestamp. Profile totals never determine completion; unrelated inserts or deletes do not trigger another seed.
- Each transaction acquires SQL Server `sp_getapplock` with an exclusive, **transaction-owned** database-scoped lock before inspecting or creating the table/marker. It calls the existing `ITalentDataGenerator` for at most its 50,000-profile chunk, then updates durable progress and commits **together with all generated profiles, skills, and experiences**. There is no transaction spanning all 500,000 profiles. The current SQL configuration has no execution-strategy retries; startup refuses retry-enabled configuration rather than replaying generator SQL outside this contract.
- Concurrent startups serialize each chunk and reread persisted progress under the lock; they cannot generate the same chunk twice. The final chunk also sets completion in that same transaction. Later startups skip it, regardless of the number of ordinary profiles currently present.
- On failure/cancellation, the active transaction rolls back; previously committed chunks remain. A subsequent `up` resumes from that committed count, including when a connection was lost after a commit but before the app observed success. Ctrl+C/SIGTERM cancellation is handled before the host starts; force-kill also leaves SQL to roll back any uncommitted transaction. Every failure propagates: HTTP never starts with an incomplete seed.
- The requested count is stored with the fixed identity. Changing it later (including after completion) fails explicitly; restore the original count. It does not silently reset progress, append another startup seed, or use a new count-derived identity. Do not manually remove/edit the marker.

Restart only the new app, or safely stop the new stack while keeping its data:

```powershell
docker compose -f docker-compose.containerized.yml restart app
docker compose -f docker-compose.containerized.yml down
```

`up --build`, app restarts, and `down` followed by `up` reuse the same SQL volume and skip the completed seed (or resume an interrupted one). Do **not** add `-v` unless deliberately deleting this lab's entire database. A deliberate reset is:

```powershell
docker compose -f docker-compose.containerized.yml down -v
docker compose -f docker-compose.containerized.yml up --build
```

This destroys the sample data, bulk data, and marker; the new database is migrated and seeded again. Never print the full interpolated Compose configuration (it contains credentials); validate syntax with `docker compose -f docker-compose.containerized.yml config --quiet`.

## Talent market performance lab
`TalentInsightsService` (in `MiniProject.ComplexLogicInMiddle`) is deliberately written the way such reports often end up in real code, so it degrades as data grows:

- Joins `ProfileSkills × Skills × Profiles × Experiences`, which multiplies rows before de-duplication.
- `SELECT DISTINCT` over `UPPER(LTRIM(RTRIM(...)))`, `COALESCE`, string concatenations, the job title and the string-stored `EmploymentType`/`WorkMode` enums, so SQL Server must hash or sort the whole intermediate set.
- "Most candidates first" ordering: a correlated `COUNT(DISTINCT ...)` subquery with trimmed, non-sargable equality predicates. EF emits it twice, once in `SELECT` and once in `ORDER BY`, and it runs for every distinct segment, not just the page.
- Accent-insensitive "search everything": `(... + ... + ...) COLLATE Latin1_General_100_CI_AI LIKE '%x%'`.
- A separate `DISTINCT` count for paging, plus an N+1 loop that issues one more joined query for every row on the page.

To reproduce it, run `seed-bulk` in the Playground console. It defaults to 100,000 profiles; the maximum is 1,000,000. Rows are generated set-based in T-SQL, in chunks of 50,000, with SQL logging muted. Each profile gets padded locations and companies, Vietnamese names and cities spelled with and without diacritics, 8 skills and 4 experiences. Then open `/insights` and compare the elapsed time and round-trip count, and the SQL timings in the console. Queries that exceed EF's default 30-second command timeout fail with a timeout, as they would in production.

## Three portfolio problem reproductions

These endpoints reproduce problematic behavior only, using the existing portfolio schema. They are **disabled by default** behind `PerformanceLab:Enabled`; existing APIs and pages are unchanged. Enable only against an isolated local database once SQL Server and the existing schema are ready:

```powershell
dotnet run --project MiniProject.Playground -- --urls http://localhost:5080 --PerformanceLab:Enabled=true
```

| GET endpoint | Deliberate problem |
| --- | --- |
| `/api/lab/shortlist?count=40` | Reads selected profiles asynchronously in ID order, then performs separate initial/preferred assignment lookups and reloads the entire skill catalog for each lookup before C# matching. Mirrors repeated baseline/current setup-material reads (N+1). Default count: 40; allowed: 1..200. |
| `/api/lab/skill-badges?category=Backend` | Joins skill assignments to skills, selects trimmed name/category pairs, synchronously transfers all duplicate pairs, then applies C# two-field `Distinct` and ordering. Mirrors material-color processing, not SQL `DISTINCT`. Category is optional, at most 80 characters. |
| `/api/lab/directory-config?profileId=1` | An `IEnumerable` helper synchronously loads all profiles with skills, experience, projects and project skills, and social links in one no-tracking query, then filters by profile ID in C#. Mirrors lost SQL filtering and blocking I/O in production-line configuration. Default profile ID: 1; must be positive. |

Responses contain data only. Invalid bounds return HTTP 400. Synchronous database reads cannot be interrupted by cancellation; cancellation is checked before and after each read.

The existing console command `seed-bulk 10000` adds 10,000 profiles, 80,000 skill assignments and 40,000 experiences. It is additive; do not rerun blindly or add more data to an already-large dataset.

**Warning:** directory loading can exhaust application memory on large datasets. Start small, make requests serially, and do not use parallel requests against a large dataset.

Live SQL behavior has not been verified. Actual timeouts or locking are not guaranteed by these code patterns.