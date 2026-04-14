using FluentAssertions;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Validators;
using ProductCatalog.Domain.Enums;
using Xunit;

namespace ProductCatalog.UnitTests.Validators;

public class ProductValidatorTests
{
    private readonly CreateProductValidator _createValidator = new();
    private readonly UpdateProductValidator _updateValidator = new();

    [Fact]
    public async Task CreateProductValidator_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new CreateProductDto("Laptop", "A nice laptop", 3000m, 10, ProductCategory.Electronics);

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Description", 100, 5)]
    [InlineData("Name", "", 100, 5)]
    public async Task CreateProductValidator_WithEmptyRequiredFields_ShouldFailValidation(
        string name, string description, decimal price, int stock)
    {
        // Arrange
        var dto = new CreateProductDto(name, description, price, stock, ProductCategory.Electronics);

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task CreateProductValidator_WithNegativePrice_ShouldFailValidation()
    {
        // Arrange
        var dto = new CreateProductDto("Laptop", "A laptop", -10m, 5, ProductCategory.Electronics);

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task CreateProductValidator_WithNegativeStock_ShouldFailValidation()
    {
        // Arrange
        var dto = new CreateProductDto("Laptop", "A laptop", 100m, -1, ProductCategory.Electronics);

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StockQuantity");
    }
}
