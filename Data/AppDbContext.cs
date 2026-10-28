using CustomerOperations.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerOperations.Api.Data;

public class AppDbContext : DbContext
{
    public const string CustomerNumberSequence = "customer_number_seq";

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // New customers get CUS-1011, CUS-1012, ... (seed data uses 1001-1010)
        modelBuilder.HasSequence<long>(CustomerNumberSequence).StartsAt(1011).IncrementsBy(1);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id).HasMaxLength(20);
            entity.Property(c => c.FullName).HasMaxLength(80).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(254).IsRequired();
            entity.Property(c => c.EmailNormalized).HasMaxLength(254).IsRequired();
            entity.Property(c => c.Phone).HasMaxLength(10).IsRequired();
            entity.Property(c => c.City).HasMaxLength(60).IsRequired();
            entity.Property(c => c.Status).HasConversion<string>().HasMaxLength(10);
            entity.Property(c => c.RiskLevel).HasConversion<string>().HasMaxLength(10);

            entity.HasIndex(c => c.EmailNormalized).IsUnique();
            entity.HasIndex(c => c.City);
            entity.HasIndex(c => c.Status);
            entity.HasIndex(c => c.UpdatedAt);

            entity.HasData(SeedData.Customers);
        });
    }
}
