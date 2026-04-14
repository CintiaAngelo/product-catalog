using FluentAssertions;
using Moq;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Application.Services;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces;
using Xunit;

namespace ProductCatalog.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _cacheMock = new Mock<ICacheService>();
        _productService = new ProductService(_repositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_WhenCacheIsEmpty_ShouldFetchFromRepository()
    {
        // Arrange
        var products = new List<Product>
        {
            Product.Create("Laptop", "A laptop", 3000m, 10, ProductCategory.Electronics)
        };

        _cacheMock.Setup(c => c.GetAsync<IEnumerable<ProductDto>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<ProductDto>?)null);

        _repositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetAllProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ProductDto>>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsAsync_WhenCacheHasData_ShouldReturnCachedData()
    {
        // Arrange
        var cachedProducts = new List<ProductDto>
        {
            new(Guid.NewGuid(), "Laptop", "A laptop", 3000m, 10, "Electronics", true, DateTime.UtcNow, null)
        };

        _cacheMock.Setup(c => c.GetAsync<IEnumerable<ProductDto>>(It.IsAny<string>()))
            .ReturnsAsync(cachedProducts);

        // Act
        var result = await _productService.GetAllProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        var product = Product.Create("Laptop", "A laptop", 3000m, 10, ProductCategory.Electronics);

        _cacheMock.Setup(c => c.GetAsync<ProductDto>(It.IsAny<string>()))
            .ReturnsAsync((ProductDto?)null);

        _repositoryMock.Setup(r => r.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetProductByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync<ProductDto>(It.IsAny<string>()))
            .ReturnsAsync((ProductDto?)null);

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetProductByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateProductAsync_ShouldCreateAndInvalidateCache()
    {
        // Arrange
        var createDto = new CreateProductDto("Laptop", "A laptop", 3000m, 10, ProductCategory.Electronics);
        var product = Product.Create(createDto.Name, createDto.Description, createDto.Price, createDto.StockQuantity, createDto.Category);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.CreateProductAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Laptop");
        _cacheMock.Verify(c => c.RemoveByPrefixAsync("products:"), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Product?)null);

        var updateDto = new UpdateProductDto("Laptop", "A laptop", 3000m, 10, ProductCategory.Electronics);

        // Act
        var act = async () => await _productService.UpdateProductAsync(Guid.NewGuid(), updateDto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.ExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(false);

        // Act
        var act = async () => await _productService.DeleteProductAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
