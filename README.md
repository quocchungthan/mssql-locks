# Learning MSSQL and dotnet 10
All the commands I already written inside `./tools/` directory, we execute them from here, their name already tell what it means.

# setup env
As a typical solution that runs with docker: fill the `.env` file at the same level of compose file, then just up with `--detach` flag.

# Portfolio database
Portfolio entities and `MiniDbContext` are in `MiniProject.Migrations`. The Playground project references that project and registers the context as scoped.

The connection details default to the local SQL Server in `MiniProject.Playground/appsettings.json`. The root `.env` supplies `SA_PASSWORD`; a process environment variable with that name takes precedence. The connection string is assembled at runtime, and the password is never stored in `appsettings.json`.

Generate a migration with:

```sh
dotnet ef migrations add MigrationName --project MiniProject.Migrations/MiniProject.Migrations.csproj --startup-project MiniProject.Playground/MiniProject.Playground.csproj --context MiniDbContext
```

Apply pending migrations (creating the database if it does not exist) with:

```sh
./tools/migrate-database.sh
```