using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RopeParison.Data;

// Migration tooling never falls back to the application's Azure configuration.
public sealed class DesignTimeDataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ROPEPARISON_MIGRATION_CONNECTION")
            ?? throw new InvalidOperationException("Set ROPEPARISON_MIGRATION_CONNECTION explicitly before running EF tools.");
        return new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlServer(connection).Options);
    }
}
