# Learning MSSQL and dotnet 10
All the commands I already written inside `./tools/` directory, we execute them from here, their name already tell what it means.

# setup env
As a typical solution that runs with docker: fill the `.env` file at the same level of compose file, then just up with `--detach` flag.

# run the playground
Start the database, then run `dotnet run --project MiniProject.Playground`. The Playground serves:

- `GET /health` — reports whether the database is reachable.
- `GET /api/portfolios/{profileId}` — returns the portfolio profile and its projects, experience, skills, and social links by profile ID.
- `/profile/{profileId}` — renders the portfolio UI for that profile using the API above. Profile IDs are the current profile keys.

The terminal also accepts `help` and `exit` while the API is running. EF Core database command logs include execution durations.