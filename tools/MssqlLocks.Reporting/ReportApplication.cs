namespace MssqlLocks.Reporting;

public sealed record ReportColumn(string Name);

public sealed record ReportRow(IReadOnlyList<object?> Values);

public interface IReportResultConsumer
{
    Task BeginAsync(IReadOnlyList<ReportColumn> columns, CancellationToken cancellationToken);
    Task WriteRowAsync(ReportRow row, CancellationToken cancellationToken);
    Task CompleteAsync(long rowCount, TimeSpan elapsed, CancellationToken cancellationToken);
}

public interface IReportSqlSource
{
    Task<string> ReadAsync(ReportDefinition report, CancellationToken cancellationToken);
}

public interface IReportQueryExecutor
{
    Task ExecuteAsync(
        string sql,
        string connectionString,
        IReportResultConsumer consume,
        CancellationToken cancellationToken);
}

public interface IReportApplicationService
{
    Task RunAsync(
        ReportPack pack,
        ReportDefinition report,
        string connectionString,
        IReportResultConsumer consume,
        CancellationToken cancellationToken);
}

public sealed class ReportApplicationService(
    IReportSqlSource sqlSource,
    IReportQueryExecutor queryExecutor) : IReportApplicationService
{
    public async Task RunAsync(
        ReportPack pack,
        ReportDefinition report,
        string connectionString,
        IReportResultConsumer consume,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pack);
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(consume);

        if (!pack.Reports.Contains(report))
        {
            throw new InvalidOperationException("The requested report is not part of the selected report pack.");
        }

        var sql = await sqlSource.ReadAsync(report, cancellationToken);
        await queryExecutor.ExecuteAsync(sql, connectionString, consume, cancellationToken);
    }
}