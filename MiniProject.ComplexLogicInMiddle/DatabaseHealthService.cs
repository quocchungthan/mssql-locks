using Microsoft.EntityFrameworkCore;
using MiniProject.Migrations;

namespace MiniProject.ComplexLogicInMiddle;

public interface IDatabaseHealthService
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken);
}

public sealed class DatabaseHealthService : IDatabaseHealthService
{
    private readonly MiniDbContext _dbContext;

    public DatabaseHealthService(MiniDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken) =>
        _dbContext.Database.CanConnectAsync(cancellationToken);
}
