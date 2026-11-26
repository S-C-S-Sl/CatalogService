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
    public async Task GetAll_AnswersWithStatus200AndAllProducts()
    {
        IReadOnlyList<ProductDto> products = [ExampleProduct];
        productInteractor.GetAllAsync(Arg.Any<CancellationToken>()).Returns(products);

        var response = await productsController.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(products, okResult.Value);
    }

    [Fact]
    public async Task GetAll_AnswersWithStatus200AndAnEmptyListWhenNoProductsExist()
    {
        IReadOnlyList<ProductDto> noProducts = [];
        productInteractor.GetAllAsync(Arg.Any<CancellationToken>()).Returns(noProducts);

        var response = await productsController.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<ProductDto>>(okResult.Value));
    }

    [Fact]
    public async Task Get_AnswersWithStatus200AndTheProduct()
    {
        productInteractor.GetAsync(ProductId, Arg.Any<CancellationToken>()).Returns(ExampleProduct);

        var response = await productsController.Get(ProductId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(ExampleProduct, okResult.Value);
    }

    [Fact]
    public async Task Get_AnswersWithStatus404WhenProductDoesNotExist()
    {
        productInteractor.GetAsync(ProductId, Arg.Any<CancellationToken>()).Returns((ProductDto?)null);

        var response = await productsController.Get(ProductId, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundResult>(response.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Get_AsksTheInteractorForTheRequestedProduct()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        await productsController.Get(ProductId, cancellationTokenSource.Token);

        await productInteractor.Received(1).GetAsync(ProductId, cancellationTokenSource.Token);
    }

    [Fact]
    public async Task Create_AnswersWithStatus201AndTheCreatedProduct()
    {
        var request = new CreateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.CreateAsync(request, Arg.Any<CancellationToken>()).Returns(ExampleProduct);

        var response = await productsController.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(response.Result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(ExampleProduct, createdResult.Value);
    }

    [Fact]
    public async Task Create_PointsTheLocationToTheNewProduct()
    {
        var request = new CreateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.CreateAsync(request, Arg.Any<CancellationToken>()).Returns(ExampleProduct);

        var response = await productsController.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(response.Result);
        Assert.Equal(nameof(ProductsController.Get), createdResult.RouteName);
        Assert.Equal(ProductId, createdResult.RouteValues!["id"]);
    }

    [Fact]
    public async Task Create_PassesTheRequestToTheInteractor()
    {
        var request = new CreateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.CreateAsync(request, Arg.Any<CancellationToken>()).Returns(ExampleProduct);

        await productsController.Create(request, CancellationToken.None);

        await productInteractor.Received(1).CreateAsync(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_AnswersWithStatus200AndTheChangedProduct()
    {
        var request = new UpdateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.UpdateAsync(ProductId, request, Arg.Any<CancellationToken>()).Returns(ExampleProduct);

        var response = await productsController.Update(ProductId, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(ExampleProduct, okResult.Value);
    }

    [Fact]
    public async Task Update_AnswersWithStatus404WhenProductDoesNotExist()
    {
        var request = new UpdateProductRequest("Kaffee", "Gemahlen", 4.99m);
        productInteractor.UpdateAsync(ProductId, request, Arg.Any<CancellationToken>()).Returns((ProductDto?)null);

        var response = await productsController.Update(ProductId, request, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundResult>(response.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Update_PassesIdAndRequestToTheInteractor()
    {
        var request = new UpdateProductRequest("Kaffee", "Gemahlen", 4.99m);

        await productsController.Update(ProductId, request, CancellationToken.None);

        await productInteractor.Received(1).UpdateAsync(ProductId, request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_AnswersWithStatus204WhenProductExisted()
    {
        productInteractor.DeleteAsync(ProductId, Arg.Any<CancellationToken>()).Returns(true);

        var response = await productsController.Delete(ProductId, CancellationToken.None);

        var noContentResult = Assert.IsType<NoContentResult>(response);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    public async Task Delete_AnswersWithStatus404WhenProductDoesNotExist()
    {
        productInteractor.DeleteAsync(ProductId, Arg.Any<CancellationToken>()).Returns(false);

        var response = await productsController.Delete(ProductId, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundResult>(response);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }
}
