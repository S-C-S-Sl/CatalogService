using Asp.Versioning;
using CatalogService.Interactors;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    [Route("v{version:apiVersion}/products")]
    public sealed class ProductsController(IProductInteractor productInteractor) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
        {
            var products = await productInteractor.GetAllAsync(cancellationToken);

            return Ok(products);
        }

        [HttpGet("{id:guid}", Name = nameof(Get))]
        public async Task<ActionResult<ProductDto>> Get(Guid id, CancellationToken cancellationToken)
        {
            var product = await productInteractor.GetAsync(id, cancellationToken);
            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken cancellationToken)
        {
            var product = await productInteractor.CreateAsync(request, cancellationToken);

            return CreatedAtRoute(nameof(Get), new { id = product.Id }, product);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
        {
            var product = await productInteractor.UpdateAsync(id, request, cancellationToken);
            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var wasDeleted = await productInteractor.DeleteAsync(id, cancellationToken);
            if (!wasDeleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
