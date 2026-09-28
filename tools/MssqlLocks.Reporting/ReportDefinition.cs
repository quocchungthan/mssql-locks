namespace MssqlLocks.Reporting;

public sealed record ReportDefinition(
    string Category,
    string FileName,
    string RelativePath,
    string AbsolutePath);
