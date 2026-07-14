using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TwfApp.Infrastructure;

public class KierownikappDbContextFactory : IDesignTimeDbContextFactory<KierownikappDbContext>
{
    public KierownikappDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KierownikappDbContext>();
        // Use a hardcoded path for design-time. 
        // This is only used for generating migration files.
        optionsBuilder.UseSqlite("Data Source=design_time_db.db;"); 

        return new KierownikappDbContext(optionsBuilder.Options);
    }
}