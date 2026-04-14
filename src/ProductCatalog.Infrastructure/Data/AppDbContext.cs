using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2);

            entity.Property(e => e.Category)
                .HasConversion<string>();
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            Product.Create("Laptop Pro 15", "High-performance laptop for developers", 4999.99m, 25, Domain.Enums.ProductCategory.Electronics),
            Product.Create("Clean Code Book", "A Handbook of Agile Software Craftsmanship by Robert C. Martin", 79.90m, 100, Domain.Enums.ProductCategory.Books),
            Product.Create("Running Shoes X", "Lightweight running shoes for all terrains", 299.99m, 50, Domain.Enums.ProductCategory.Sports)
        );
    }
}
