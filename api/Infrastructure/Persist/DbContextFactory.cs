using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace api.Infrastructure.Persist;

public class BloggingContextFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        //var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
        //optionsBuilder.UseNpgsql("Data Source=blog.db");

        //return new MyDbContext(optionsBuilder.Options);

        IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

        var builder = new DbContextOptionsBuilder<MyDbContext>();
        builder.UseNpgsql(configuration.GetConnectionString("pgsql"),
            optionsBuilder => optionsBuilder.MigrationsAssembly("api"));

        return new MyDbContext(builder.Options);
    }
}
