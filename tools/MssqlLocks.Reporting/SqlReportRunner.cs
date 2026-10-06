using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace MssqlLocks.Reporting;

public sealed class SqlReportRunner : IReportQueryExecutor
{
    public async Task ExecuteAsync(
        string sql,
        string connectionString,
        IReportResultConsumer consume,
        CancellationToken cancellationToken)
    {
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

        var columns = Enumerable.Range(0, reader.FieldCount)
            .Select(index => new ReportColumn(reader.GetName(index)))
            .ToArray();
        var stopwatch = Stopwatch.StartNew();
        await consume.BeginAsync(columns, cancellationToken);

        long rowCount = 0;
        while (await reader.ReadAsync(cancellationToken))
        {
            var values = Enumerable.Range(0, reader.FieldCount)
                .Select(index => reader.IsDBNull(index) ? null : reader.GetValue(index))
                .ToArray();
            await consume.WriteRowAsync(new ReportRow(values), cancellationToken);
            rowCount++;
        }

        stopwatch.Stop();
        await consume.CompleteAsync(rowCount, stopwatch.Elapsed, cancellationToken);
    }
}
