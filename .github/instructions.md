## Environment Secrets

- The root `.env` file defines `MSSQL_LOCKS_CONNECTION_STRING` for the database connection string.
- Load the connection string through environment or configuration mechanisms.
- Never read, print, log, expose, or commit the secret value.
