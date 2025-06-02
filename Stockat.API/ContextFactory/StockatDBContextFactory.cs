using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Stockat.EF;

namespace Stockat.API.ContextFactory;

public class StockatDBContextFactory : IDesignTimeDbContextFactory<StockatDBContext>
{
    public StockatDBContext CreateDbContext(string[] args)
    {

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<StockatDBContext>()
            .UseSqlServer(configuration.GetConnectionString("sqlConnection"),
            b => b.MigrationsAssembly("Stockat.EF")); // if you wanna put migrations in another project pass its name to that function
        // note default project for the migraions is beside its db context 
        // that's why this class is useless in this case since our StockatDBContext is in the ef layer which is the same project we want to put our migrations there // so there's no need for that class // we're only doing this for knowledge no more

        return new StockatDBContext(builder.Options);
    }
}