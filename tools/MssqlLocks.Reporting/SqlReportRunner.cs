using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace MssqlLocks.Reporting;

public sealed class SqlReportRunner
{
    public async Task RunAsync(
        ReportDefinition report,
        string connectionString,
        Func<DbDataReader, CancellationToken, Task> consume,
        CancellationToken cancellationToken)
    {
        var sql = await File.ReadAllTextAsync(report.AbsolutePath, cancellationToken);
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            ApplicationIntent = ApplicationIntent.ReadOnly
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = 120
        };
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
        await consume(reader, cancellationToken);
    }
}
