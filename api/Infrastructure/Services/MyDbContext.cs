using api.Domain;
using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.Services;

public class MyDbContext : DbContext
{
    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<VoiceRecordEntity> VoiceRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }
}