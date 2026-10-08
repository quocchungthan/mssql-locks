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

## Talent market performance lab
`TalentInsightsService` (in `MiniProject.ComplexLogicInMiddle`) is deliberately written the way such reports often end up in real code, so it degrades as data grows:

- Joins `ProfileSkills × Skills × Profiles × Experiences`, which multiplies rows before de-duplication.
- `SELECT DISTINCT` over `UPPER(LTRIM(RTRIM(...)))`, `COALESCE`, string concatenations, the job title and the string-stored `EmploymentType`/`WorkMode` enums, so SQL Server must hash or sort the whole intermediate set.
- "Most candidates first" ordering: a correlated `COUNT(DISTINCT ...)` subquery with trimmed, non-sargable equality predicates. EF emits it twice, once in `SELECT` and once in `ORDER BY`, and it runs for every distinct segment, not just the page.
- Accent-insensitive "search everything": `(... + ... + ...) COLLATE Latin1_General_100_CI_AI LIKE '%x%'`.
- A separate `DISTINCT` count for paging, plus an N+1 loop that issues one more joined query for every row on the page.

To reproduce it, run `seed-bulk` in the Playground console. It defaults to 100,000 profiles; the maximum is 1,000,000. Rows are generated set-based in T-SQL, in chunks of 50,000, with SQL logging muted. Each profile gets padded locations and companies, Vietnamese names and cities spelled with and without diacritics, 8 skills and 4 experiences. Then open `/insights` and compare the elapsed time and round-trip count, and the SQL timings in the console. Queries that exceed EF's default 30-second command timeout fail with a timeout, as they would in production.