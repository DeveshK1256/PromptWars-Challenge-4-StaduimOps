using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StadiumOps.Infrastructure.Persistence;

public sealed class StadiumOpsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<StadiumOpsDbContext>
{
    public StadiumOpsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StadiumOpsDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=StadiumOpsDesignTime;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new StadiumOpsDbContext(options);
    }
}
