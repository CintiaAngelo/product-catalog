using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    ProductCategory Category
);

public record UpdateProductDto(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    ProductCategory Category
);
