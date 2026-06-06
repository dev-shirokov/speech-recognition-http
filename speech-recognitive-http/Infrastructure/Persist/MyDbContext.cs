using api.Domain;
using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.Persist;

public class MyDbContext : DbContext
{
    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<VoiceRecordEntity> VoiceRecords { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }
}