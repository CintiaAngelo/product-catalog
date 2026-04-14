using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;

    private const string AllProductsCacheKey = "products:all";
    private const string ProductCacheKeyPrefix = "products:";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    public ProductService(IProductRepository productRepository, ICacheService cacheService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var cached = await _cacheService.GetAsync<IEnumerable<ProductDto>>(AllProductsCacheKey);
        if (cached is not null)
            return cached;

        var products = await _productRepository.GetAllAsync();
        var productDtos = products.Select(MapToDto);

        await _cacheService.SetAsync(AllProductsCacheKey, productDtos, CacheExpiration);

        return productDtos;
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var cacheKey = $"{ProductCacheKeyPrefix}{id}";

        var cached = await _cacheService.GetAsync<ProductDto>(cacheKey);
        if (cached is not null)
            return cached;

        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            return null;

        var productDto = MapToDto(product);
        await _cacheService.SetAsync(cacheKey, productDto, CacheExpiration);

        return productDto;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
    {
        var cacheKey = $"{ProductCacheKeyPrefix}category:{category.ToLower()}";

        var cached = await _cacheService.GetAsync<IEnumerable<ProductDto>>(cacheKey);
        if (cached is not null)
            return cached;

        var products = await _productRepository.GetByCategoryAsync(category);
        var productDtos = products.Select(MapToDto);

        await _cacheService.SetAsync(cacheKey, productDtos, CacheExpiration);

        return productDtos;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        var product = Product.Create(
            createProductDto.Name,
            createProductDto.Description,
            createProductDto.Price,
            createProductDto.StockQuantity,
            createProductDto.Category
        );

        var createdProduct = await _productRepository.AddAsync(product);

        await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);

        return MapToDto(createdProduct);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product with ID {id} not found.");

        product.Update(
            updateProductDto.Name,
            updateProductDto.Description,
            updateProductDto.Price,
            updateProductDto.StockQuantity,
            updateProductDto.Category
        );

        var updatedProduct = await _productRepository.UpdateAsync(product);

        await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);

        return MapToDto(updatedProduct);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var exists = await _productRepository.ExistsAsync(id);
        if (!exists)
            throw new KeyNotFoundException($"Product with ID {id} not found.");

        await _productRepository.DeleteAsync(id);

        await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);
    }

    private static ProductDto MapToDto(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.StockQuantity,
        product.Category.ToString(),
        product.IsActive,
        product.CreatedAt,
        product.UpdatedAt
    );
}
