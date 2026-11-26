using CatalogService.Api.Controllers;
using CatalogService.Interactors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CatalogService.Api.Tests;

/// <summary>Untersucht die Antworten des Controllers selbst: Statuscode, Inhalt und Verweis auf den neuen Eintrag.</summary>
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
    public async Task GetAll_ProductsExist_ReturnsStatus200WithAllProducts()
    {
        var expectedStatusCode = StatusCodes.Status200OK;
        ProductDto[] expectedProducts = [ExampleProduct];
        productInteractor.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expectedProducts.ToAsyncEnumerable());

        var actualResponse = await productsController.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actualResponse.Result);
        Assert.Equal(expectedStatusCode, okResult.StatusCode);
        Assert.Equal(expectedProducts, Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value));
    }

    [Fact]
    public async Task GetAll_NoProductsExist_ReturnsStatus200WithEmptyList()
    {
        var expectedStatusCode = StatusCodes.Status200OK;
        ProductDto[] noProducts = [];
        productInteractor.GetAllAsync(Arg.Any<CancellationToken>()).Returns(noProducts.ToAsyncEnumerable());

        var actualResponse = await productsController.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actualResponse.Result);
        Assert.Equal(expectedStatusCode, okResult.StatusCode);
        Assert.Empty(Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value));
    }

    [Fact]
    public async Task Get_ProductExists_ReturnsStatus200WithProduct()
    {
        var expectedStatusCode = StatusCodes.Status200OK;
        var expectedProduct = ExampleProduct;
        productInteractor.GetAsync(ProductId, Arg.Any<CancellationToken>()).Returns(expectedProduct);

        var actualResponse = await productsController.Get(ProductId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actualResponse.Result);
        Assert.Equal(expectedStatusCode, okResult.StatusCode);
        Assert.Equal(expectedProduct, okResult.Value);
    }

    [Fact]
    public async Task Get_ProductDoesNotExist_ReturnsStatus404()
    {
        var expectedStatusCode = StatusCodes.Status404NotFound;
        productInteractor.GetAsync(ProductId, Arg.Any<CancellationToken>()).Returns((ProductDto?)null);

        var actualResponse = await productsController.Get(ProductId, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundResult>(actualResponse.Result);
        Assert.Equal(expectedStatusCode, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Get_ProductIdRequested_AsksInteractorForThatProduct()
    {
        var expectedId = ProductId;
        using var cancellationTokenSource = new CancellationTokenSource();

        await productsController.Get(expectedId, cancellationTokenSource.Token);

        await productInteractor.Received(1).GetAsync(expectedId, cancellationTokenSource.Token);
    }

    [Fact]
    public async Task Create_RequestIsValid_ReturnsStatus201WithCreatedProduct()
    {
        var expectedStatusCode = StatusCodes.Status201Created;
        var expectedProduct = ExampleProduct;
        var request = new CreateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.CreateAsync(request, Arg.Any<CancellationToken>()).Returns(expectedProduct);

        var actualResponse = await productsController.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(actualResponse.Result);
        Assert.Equal(expectedStatusCode, createdResult.StatusCode);
        Assert.Equal(expectedProduct, createdResult.Value);
    }

    [Fact]
    public async Task Create_RequestIsValid_PointsLocationToNewProduct()
    {
        var expectedRouteName = nameof(ProductsController.Get);
        var expectedId = ProductId;
        var request = new CreateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.CreateAsync(request, Arg.Any<CancellationToken>()).Returns(ExampleProduct);

        var actualResponse = await productsController.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(actualResponse.Result);
        Assert.Equal(expectedRouteName, createdResult.RouteName);
        Assert.Equal(expectedId, createdResult.RouteValues!["id"]);
    }

    [Fact]
    public async Task Create_RequestIsValid_PassesRequestToInteractor()
    {
        var expectedRequest = new CreateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.CreateAsync(expectedRequest, Arg.Any<CancellationToken>()).Returns(ExampleProduct);

        await productsController.Create(expectedRequest, CancellationToken.None);

        await productInteractor.Received(1).CreateAsync(expectedRequest, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_ProductExists_ReturnsStatus200WithChangedProduct()
    {
        var expectedStatusCode = StatusCodes.Status200OK;
        var expectedProduct = ExampleProduct;
        var request = new UpdateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.UpdateAsync(ProductId, request, Arg.Any<CancellationToken>()).Returns(expectedProduct);

        var actualResponse = await productsController.Update(ProductId, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actualResponse.Result);
        Assert.Equal(expectedStatusCode, okResult.StatusCode);
        Assert.Equal(expectedProduct, okResult.Value);
    }

    [Fact]
    public async Task Update_ProductDoesNotExist_ReturnsStatus404()
    {
        var expectedStatusCode = StatusCodes.Status404NotFound;
        var request = new UpdateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.UpdateAsync(ProductId, request, Arg.Any<CancellationToken>()).Returns((ProductDto?)null);

        var actualResponse = await productsController.Update(ProductId, request, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundResult>(actualResponse.Result);
        Assert.Equal(expectedStatusCode, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Update_RequestIsValid_PassesIdAndRequestToInteractor()
    {
        var expectedId = ProductId;
        var expectedRequest = new UpdateProductRequest("Kaffee", "Gemahlen", 4.99m);

        await productsController.Update(expectedId, expectedRequest, CancellationToken.None);

        await productInteractor.Received(1).UpdateAsync(expectedId, expectedRequest, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_ProductExisted_ReturnsStatus204()
    {
        var expectedStatusCode = StatusCodes.Status204NoContent;
        productInteractor.DeleteAsync(ProductId, Arg.Any<CancellationToken>()).Returns(true);

        var actualResponse = await productsController.Delete(ProductId, CancellationToken.None);

        var noContentResult = Assert.IsType<NoContentResult>(actualResponse);
        Assert.Equal(expectedStatusCode, noContentResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ProductDidNotExist_ReturnsStatus404()
    {
        var expectedStatusCode = StatusCodes.Status404NotFound;
        productInteractor.DeleteAsync(ProductId, Arg.Any<CancellationToken>()).Returns(false);

        var actualResponse = await productsController.Delete(ProductId, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundResult>(actualResponse);
        Assert.Equal(expectedStatusCode, notFoundResult.StatusCode);
    }
}
