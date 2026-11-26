using System.Reflection;
using Asp.Versioning;
using CatalogService.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace CatalogService.Api.Tests;

/// <summary>Sichert die Schnittstelle des Controllers: Version, Pfad und HTTP-Methode jeder Aktion.</summary>
public sealed class ProductsControllerContractTests
{
    [Fact]
    public void Controller_IsAnApiController()
    {
        var attribute = typeof(ProductsController).GetCustomAttribute<ApiControllerAttribute>();

        Assert.NotNull(attribute);
    }

    [Fact]
    public void Controller_IsOfferedInApiVersion1()
    {
        var attribute = typeof(ProductsController).GetCustomAttribute<ApiVersionAttribute>();

        Assert.NotNull(attribute);
        var version = Assert.Single(attribute.Versions);
        Assert.Equal(1, version.MajorVersion);
        Assert.Equal(0, version.MinorVersion);
    }

    [Fact]
    public void Controller_IsReachableUnderTheVersionedProductsPath()
    {
        var attribute = typeof(ProductsController).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("v{version:apiVersion}/products", attribute.Template);
    }

    [Theory]
    [InlineData(nameof(ProductsController.GetAll), "GET", null)]
    [InlineData(nameof(ProductsController.Get), "GET", "{id:guid}")]
    [InlineData(nameof(ProductsController.Create), "POST", null)]
    [InlineData(nameof(ProductsController.Update), "PUT", "{id:guid}")]
    [InlineData(nameof(ProductsController.Delete), "DELETE", "{id:guid}")]
    public void Action_IsReachableThroughTheExpectedHttpMethodAndPath(string actionName, string httpMethod, string? pathTemplate)
    {
        var action = typeof(ProductsController).GetMethod(actionName);

        Assert.NotNull(action);
        var attribute = Assert.Single(action.GetCustomAttributes<HttpMethodAttribute>());
        Assert.Equal(httpMethod, Assert.Single(attribute.HttpMethods));
        Assert.Equal(pathTemplate, attribute.Template);
    }
}
