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
    public void ProductsController_ClassAttributes_IsMarkedAsApiController()
    {
        var actualAttribute = typeof(ProductsController).GetCustomAttribute<ApiControllerAttribute>();

        Assert.NotNull(actualAttribute);
    }

    [Fact]
    public void ProductsController_ClassAttributes_IsOfferedInApiVersion1()
    {
        var expectedMajorVersion = 1;
        var expectedMinorVersion = 0;

        var actualAttribute = typeof(ProductsController).GetCustomAttribute<ApiVersionAttribute>();

        Assert.NotNull(actualAttribute);
        var actualVersion = Assert.Single(actualAttribute.Versions);
        Assert.Equal(expectedMajorVersion, actualVersion.MajorVersion);
        Assert.Equal(expectedMinorVersion, actualVersion.MinorVersion);
    }

    [Fact]
    public void ProductsController_ClassAttributes_UsesTheVersionedProductsPath()
    {
        var expectedTemplate = "v{version:apiVersion}/products";

        var actualAttribute = typeof(ProductsController).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(actualAttribute);
        Assert.Equal(expectedTemplate, actualAttribute.Template);
    }

    [Theory]
    [InlineData(nameof(ProductsController.GetAll), "GET", null)]
    [InlineData(nameof(ProductsController.Get), "GET", "{id:guid}")]
    [InlineData(nameof(ProductsController.Create), "POST", null)]
    [InlineData(nameof(ProductsController.Update), "PUT", "{id:guid}")]
    [InlineData(nameof(ProductsController.Delete), "DELETE", "{id:guid}")]
    public void Action_HttpAttribute_ExposesExpectedHttpMethodAndPath(string actionName, string expectedHttpMethod, string? expectedTemplate)
    {
        var action = typeof(ProductsController).GetMethod(actionName);

        Assert.NotNull(action);
        var actualAttribute = Assert.Single(action.GetCustomAttributes<HttpMethodAttribute>());
        Assert.Equal(expectedHttpMethod, Assert.Single(actualAttribute.HttpMethods));
        Assert.Equal(expectedTemplate, actualAttribute.Template);
    }
}
