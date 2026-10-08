## Environment Secrets

- The root `.env` file defines `SA_PASSWORD` for SQL Server; Playground reads it to configure EF Core.
- Do not use or add an `MSSQL_LOCKS_CONNECTION_STRING` variable.
- Never print, log, expose, or commit the password value.

## Windows Repository Conventions

- This repository runs on Windows.
- In instructions, commands, and tool arguments, write repository-relative paths with `/` forward slashes, for example `tools/MssqlLocks.Dmv/Program.cs`.
- Do not write repository paths with escaped `\\` backslashes.
- Keep commands PowerShell-compatible.
