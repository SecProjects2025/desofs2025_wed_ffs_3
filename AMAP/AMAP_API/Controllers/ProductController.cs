using AMAP.Business.Interfaces;
using AMAP.Entities;
using AMAP.Entities.Models.DTO.Product;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AMAP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(UserType.Producer))]
    public class ProductController : BaseController
    {
        private readonly IProductBc _productBc;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductBc productBc, IWebHostEnvironment webHostEnvironment)
        {
            _productBc = productBc;
            _webHostEnvironment = webHostEnvironment;
        }


        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            var product = _productBc.GetProduct(id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            var dto = new
            {
                product.Name,
                product.Description,
                product.Photo,
                product.ReferencePrice,
                product.DeliveryUnits,
                Type = product.Composites != null && product.Composites.Any() ? "Composite" : "Simple",
                Composites = product.Composites?.Select(c => new ProductComposeDto
                {
                    SubProductName = c.SubProduct.Name,
                    Quantity = c.Quantity
                })
            };

            return Ok(dto);
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] Product product)
        {
            var producerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            try
            {
                var createdProduct = _productBc.CreateProduct(product, producerId);
                return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product product)
        {
            var producerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
            try
            {
                _productBc.UpdateProduct(id, product, producerId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var producerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
            try
            {
                _productBc.DeleteProduct(id, producerId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("import-from-file")]
        public IActionResult ImportProductsFromFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded" });
            }

            if (!file.ContentType.Contains("text"))
            {
                return BadRequest(new { message = "File must be text format" });
            }

            var producerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(producerId))
            {
                return Unauthorized(new { message = "Producer not authenticated" });
            }

            try
            {
                using var stream = file.OpenReadStream();
                var (importedProducts, errors) = _productBc.ImportProductsFromFile(stream, file.FileName, producerId);

                return Ok(new
                {
                    message = $"Import completed. {importedProducts.Count} products imported",
                    importedProducts = importedProducts.Select(p => new { p.Id, p.Name }),
                    errors
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                var errors = ex.Data.Contains("Errors") ? (List<string>)ex.Data["Errors"] : new List<string>();
                return BadRequest(new
                {
                    message = ex.Message,
                    errors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error processing file: {ex.Message}" });
            }
        }

        [HttpGet("export-to-file")]
        public IActionResult ExportProductsToFile()
        {
            var producerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(producerId))
            {
                return Unauthorized(new { message = "Producer not authenticated" });
            }

            try
            {
                var (fileContent, fileName, contentType) = _productBc.ExportProductsToFile(producerId, _webHostEnvironment.ContentRootPath);
                return File(fileContent, contentType, fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error exporting products: {ex.Message}" });
            }
        }
    }
}
