## Environment Secrets

- The root `.env` file defines `MSSQL_LOCKS_CONNECTION_STRING` for the database connection string.
- Load the connection string through environment or configuration mechanisms.
- Never read, print, log, expose, or commit the secret value.

## Windows Repository Conventions

- This repository runs on Windows.
- In instructions, commands, and tool arguments, write repository-relative paths with `/` forward slashes, for example `tools/MssqlLocks.Dmv/Program.cs`.
- Do not write repository paths with escaped `\\` backslashes.
- Keep commands PowerShell-compatible.
