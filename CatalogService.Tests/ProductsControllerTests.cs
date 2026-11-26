using CatalogService.Controllers;
using CatalogService.Interactors;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CatalogService.Tests;

public sealed class ProductsControllerTests
{
    private static readonly Guid ProductId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly ProductDto ExampleProduct = new(ProductId, "Kaffee", "Gemahlen", 4.99m);

    private readonly IProductInteractor productInteractor = Substitute.For<IProductInteractor>();
    private readonly ProductsController productsController;

    public ProductsControllerTests()
    {
        productsController = new ProductsController(productInteractor);
    }

    [Fact]
    public async Task GetAll_ReturnsAllProducts()
    {
        IReadOnlyList<ProductDto> products = [ExampleProduct];
        productInteractor.GetAllAsync(Arg.Any<CancellationToken>()).Returns(products);

        var result = await productsController.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(products, ok.Value);
    }

    [Fact]
    public async Task Get_ReturnsTheProduct()
    {
        productInteractor.GetAsync(ProductId, Arg.Any<CancellationToken>()).Returns(ExampleProduct);

        var result = await productsController.Get(ProductId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(ExampleProduct, ok.Value);
    }

    [Fact]
    public async Task Get_ReturnsNotFoundWhenProductDoesNotExist()
    {
        productInteractor.GetAsync(ProductId, Arg.Any<CancellationToken>()).Returns((ProductDto?)null);

        var result = await productsController.Get(ProductId, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedProductWithLocation()
    {
        var request = new CreateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.CreateAsync(request, Arg.Any<CancellationToken>()).Returns(ExampleProduct);

        var result = await productsController.Create(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtRouteResult>(result.Result);
        Assert.Equal(ProductId, created.RouteValues!["id"]);
        Assert.Equal(ExampleProduct, created.Value);
    }

    [Fact]
    public async Task Update_ReturnsTheChangedProduct()
    {
        var request = new UpdateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.UpdateAsync(ProductId, request, Arg.Any<CancellationToken>()).Returns(ExampleProduct);

        var result = await productsController.Update(ProductId, request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(ExampleProduct, ok.Value);
    }

    [Fact]
    public async Task Update_ReturnsNotFoundWhenProductDoesNotExist()
    {
        var request = new UpdateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.UpdateAsync(ProductId, request, Arg.Any<CancellationToken>()).Returns((ProductDto?)null);

        var result = await productsController.Update(ProductId, request, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContentWhenProductExisted()
    {
        productInteractor.DeleteAsync(ProductId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await productsController.Delete(ProductId, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFoundWhenProductDoesNotExist()
    {
        productInteractor.DeleteAsync(ProductId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await productsController.Delete(ProductId, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
