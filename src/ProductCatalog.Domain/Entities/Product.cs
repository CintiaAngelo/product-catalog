using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public ProductCategory Category { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Product() { }

    public static Product Create(
        string name,
        string description,
        decimal price,
        int stockQuantity,
        ProductCategory category)
    {
        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            Category = category
        };
    }

    public void Update(
        string name,
        string description,
        decimal price,
        int stockQuantity,
        ProductCategory category)
    {
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        Category = category;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}
