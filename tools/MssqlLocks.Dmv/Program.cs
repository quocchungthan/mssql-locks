using MssqlLocks.Reporting;

return await ReportApplication.RunAsync(args, "dmv", "tools/MssqlLocks.Dmv");
