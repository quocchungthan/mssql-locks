namespace MssqlLocks.Reporting;

public static class DotEnvConnection
{
    private const string VariableName = "MSSQL_LOCKS_CONNECTION_STRING";

    public static string Load(string repositoryRoot)
    {
        var environmentValue = Environment.GetEnvironmentVariable(VariableName);
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue;
        }

        var envPath = Path.Combine(repositoryRoot, ".env");
        if (!File.Exists(envPath))
        {
            throw new InvalidOperationException($"Required .env variable '{VariableName}' is missing or empty.");
        }

        foreach (var line in File.ReadLines(envPath))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            if (trimmed.StartsWith("export ", StringComparison.Ordinal))
            {
                trimmed = trimmed[7..].TrimStart();
            }

            var separator = trimmed.IndexOf('=');
            if (separator <= 0 || !string.Equals(trimmed[..separator].Trim(), VariableName, StringComparison.Ordinal))
            {
                continue;
            }

            var value = trimmed[(separator + 1)..].Trim();
            if (value.Length >= 2 && value[0] == value[^1] && (value[0] == '\'' || value[0] == '"'))
            {
                value = value[1..^1];
            }
            else if (value.Length >= 2 && (value[^1] == '\'' || value[^1] == '"') && value[^2] == ';')
            {
                value = value[..^1];
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            break;
        }

        throw new InvalidOperationException($"Required .env variable '{VariableName}' is missing or empty.");
    }
}
