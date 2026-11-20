using Asp.Versioning;
using CatalogService.Interactors;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    [Route("v{version:apiVersion}/products")]
    public sealed class ProductsController(IProductInteractor products) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken) =>
            Ok(await products.GetAllAsync(cancellationToken));

        [HttpGet("{id:guid}", Name = nameof(Get))]
        public async Task<ActionResult<ProductDto>> Get(Guid id, CancellationToken cancellationToken) =>
            await products.GetAsync(id, cancellationToken) is { } product ? Ok(product) : NotFound();

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken cancellationToken)
        {
            var product = await products.CreateAsync(request, cancellationToken);
            return CreatedAtRoute(nameof(Get), new { id = product.Id }, product);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken) =>
            await products.UpdateAsync(id, request, cancellationToken) is { } product ? Ok(product) : NotFound();

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
            await products.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }

}
