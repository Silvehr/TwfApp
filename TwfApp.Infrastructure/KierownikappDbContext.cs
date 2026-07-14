using Microsoft.EntityFrameworkCore;
using TwfApp.Core;

namespace TwfApp.Infrastructure;

public class KierownikappDbContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<JokeObject> JokeObjects { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JokeObject>(builder =>
        {
            
            builder.HasKey(j => j.Id);

            builder.Property(j => j.Author)
                .HasMaxLength(255);
            
            builder.Property(j => j.Name)
                .HasMaxLength(255)
                .IsRequired();
            
            builder.Property(j => j.Description)
                .HasColumnType("TEXT")
                .IsRequired(false);
            
            builder.Property(j => j.CreationDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            builder.Property(j => j.Editable)
                .HasDefaultValue(true);

            builder.HasIndex(j => j.Author)
                .IsUnique(false)
                .HasFilter("Author IS NOT NULL");
            
            builder.HasIndex(j => j.Name)
                .IsUnique();
        });
    }
}