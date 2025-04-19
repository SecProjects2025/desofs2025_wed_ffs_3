using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace AMAP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(UserType.Producer))]
    public class ProductController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }


        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            var product = _unitOfWork.Product.Get(p => p.Id == id && !p.IsDeleted, "Composites.SubProduct");

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Photo,
                product.ReferencePrice,
                product.DeliveryUnits,
                Type = product.Composites.Any() ? "Composite" : "Simple",
                Components = product.Composites.Select(c => new
                {
                    SubProductId = c.SubProduct.Id,
                    SubProductName = c.SubProduct.Name,
                    c.Quantity
                })
            });
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest(new { message = "Invalid product data" });
            }

            var producerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(producerId))
            {
                return Unauthorized(new { message = "Producer not authenticated" });
            }

            product.ProducerId = producerId;

            var existingProduct = _unitOfWork.Product.Get(p => p.Name == product.Name && p.ProducerId == producerId && !p.IsDeleted);
            if (existingProduct is not null)
            {
                return Conflict(new { message = "You already have a product with the same name" });
            }

            var composites = product.Composites?.ToList() ?? [];
            product.Composites = null; 

            _unitOfWork.Product.Add(product);
            _unitOfWork.Save(); 

            foreach (var composite in composites)
            {
                var subProduct = _unitOfWork.Product.Get(p => p.Id == composite.SubProductId && !p.IsDeleted);

                if (subProduct == null)
                {
                    return BadRequest(new { message = $"Subproduct with ID {composite.SubProductId} does not exist." });
                }

                if (subProduct.ProducerId != producerId)
                {
                    return BadRequest(new { message = $"Subproduct with ID {composite.SubProductId} does not belong to the producer." });
                }

                composite.ProductId = product.Id;

                _unitOfWork.Composite.Add(composite);
            }

            if (composites.Count != 0)
            {
                _unitOfWork.Save(); 
            }

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest(new { message = "Invalid product data" });
            }

            var producerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            if (string.IsNullOrEmpty(producerId))
            {
                return Unauthorized(new { message = "Producer not authenticated" });
            }

            var existingProduct = _unitOfWork.Product.Get(p => p.Id == id && p.ProducerId == producerId && !p.IsDeleted, "Composites", tracked: true);

            if (existingProduct == null)
            {
                return NotFound(new { message = "Product not found or you are not authorized to update it" });
            }

            var duplicateProduct = _unitOfWork.Product.Get(p => p.Name == product.Name && p.Id != id && !p.IsDeleted);

            if (duplicateProduct != null)
            {
                return Conflict(new { message = "A product with the same name already exists for another producer" });
            }

            var producerProduct = _unitOfWork.Product.Get(p => p.Name == product.Name && p.ProducerId == producerId && p.Id != id && !p.IsDeleted);

            if (producerProduct != null)
            {
                return Conflict(new { message = "You already have a product with the same name" });
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Photo = product.Photo;
            existingProduct.ReferencePrice = product.ReferencePrice;
            existingProduct.DeliveryUnits = product.DeliveryUnits;

            if (product.Composites != null && product.Composites.Count() != 0)
            {
                foreach (var composite in product.Composites)
                {
                    var subProduct = _unitOfWork.Product.Get(p => p.Id == composite.SubProductId && !p.IsDeleted);

                    if (subProduct == null || subProduct.ProducerId != producerId)
                    {
                        return BadRequest(new { message = $"Invalid subproduct with ID {composite.SubProductId} or it does not belong to the producer." });
                    }
                }

                foreach (var oldComposite in existingProduct.Composites.ToList())
                {
                    _unitOfWork.Composite.Remove(oldComposite);
                }

                foreach (var composite in product.Composites)
                {
                    composite.ProductId = existingProduct.Id;
                    _unitOfWork.Composite.Add(composite);
                }
            }
            else
            {
                foreach (var oldComposite in existingProduct.Composites.ToList())
                {
                    _unitOfWork.Composite.Remove(oldComposite);
                }
            }
            _unitOfWork.Product.Update(existingProduct);
            _unitOfWork.Save();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var producerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            if (string.IsNullOrEmpty(producerId))
            {
                return Unauthorized(new { message = "Producer not authenticated" });
            }

            var product = _unitOfWork.Product.Get(p => p.Id == id && p.ProducerId == producerId && !p.IsDeleted);

            if (product == null)
            {
                return NotFound(new { message = "Product not found or you are not authorized to delete it" });
            }

            var compositesToDelete = _unitOfWork.Composite.GetAll(c => c.ProductId == id);
            if (compositesToDelete.Any())
            {
                _unitOfWork.Composite.RemoveRange(compositesToDelete);
                _unitOfWork.Save();
            }

            product.IsDeleted = true;

            _unitOfWork.Product.Update(product);
            _unitOfWork.Save();

            return NoContent();
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
                var importedProducts = new List<Product>();
                var errors = new List<string>();
                var duplicateProductFound = false;

                // First, validate the file content without saving
                using (var stream = file.OpenReadStream())
                using (var reader = new StreamReader(stream))
                {
                    string? line;
                    int lineNumber = 0;

                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;

                        // Skip empty lines
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        // Skip header line
                        if (lineNumber == 1 && (line.Contains("Nome;") || line.Contains("Name;")))
                            continue;

                        // Parse line
                        var parts = line.Split(';');

                        if (parts.Length < 5)
                        {
                            errors.Add($"Line {lineNumber}: Invalid format. Expected: Name;Description;Photo;ReferencePrice;DeliveryUnits");
                            continue;
                        }

                        // Parse price
                        var priceString = parts[3].Trim().Replace(',', '.');
                        if (!double.TryParse(priceString, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        {
                            errors.Add($"Line {lineNumber}: Invalid price value '{parts[3]}'. Use format 0.00");
                            continue;
                        }

                        // Check for duplicate products
                        var productName = parts[0].Trim();
                        var existingProduct = _unitOfWork.Product.Get(p =>
                            p.Name == productName &&
                            p.ProducerId == producerId &&
                            !p.IsDeleted);

                        if (existingProduct != null)
                        {
                            errors.Add($"Line {lineNumber}: Product with name '{productName}' already exists");
                            duplicateProductFound = true;
                        }
                    }
                }

                // If duplicates found, return error without saving the file
                if (duplicateProductFound)
                {
                    return BadRequest(new
                    {
                        message = "Import failed. Found duplicate product names.",
                        errors
                    });
                }

                // No duplicates found, proceed with saving and processing
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "ProductsUpload");
                Directory.CreateDirectory(uploadsPath);
                var filePath = Path.Combine(uploadsPath, file.FileName);

                // Now save the file and process it
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Process file and add products
                using (var reader = new StreamReader(filePath))
                {
                    string? line;
                    int lineNumber = 0;

                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;

                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        if (lineNumber == 1 && (line.Contains("Nome;") || line.Contains("Name;")))
                            continue;

                        try
                        {
                            var parts = line.Split(';');

                            // Already validated format and price in previous step
                            var priceString = parts[3].Trim().Replace(',', '.');
                            double.TryParse(priceString, NumberStyles.Any, CultureInfo.InvariantCulture, out double price);

                            // Create product object
                            var product = new Product
                            {
                                Name = parts[0].Trim(),
                                Description = parts[1].Trim(),
                                Photo = parts[2].Trim(),
                                ReferencePrice = price,
                                DeliveryUnits = parts[4].Trim(),
                                ProducerId = producerId,
                                IsDeleted = false
                            };

                            // Save product
                            _unitOfWork.Product.Add(product);
                            _unitOfWork.Save();
                            importedProducts.Add(product);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Line {lineNumber}: Processing error - {ex.Message}");
                        }
                    }
                }

                return Ok(new
                {
                    message = $"Import completed. {importedProducts.Count} products imported",
                    importedProducts = importedProducts.Select(p => new { p.Id, p.Name }),
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
                var producer = _unitOfWork.AppUser.Get(p => p.Id == producerId);
                if (producer == null)
                {
                    return NotFound(new { message = "Producer not found" });
                }

                var products = _unitOfWork.Product.GetAll(p => p.ProducerId == producerId && !p.IsDeleted).ToList();

                if (products == null || !products.Any())
                {
                    return NotFound(new { message = "No products found to export" });
                }

                // Create CSV content with ; separator
                var sb = new StringBuilder();
                sb.AppendLine("Name;Description;Photo;ReferencePrice;DeliveryUnits");

                foreach (var product in products)
                {
                    sb.AppendLine($"{EscapeTxt(product.Name)};{EscapeTxt(product.Description)};{product.Photo};{product.ReferencePrice.ToString(CultureInfo.InvariantCulture)};{product.DeliveryUnits}");
                }

                // Clean producer name for filename
                var rawName = string.IsNullOrWhiteSpace(producer.Name) ? "unknown" : producer.Name;
                var cleanName = RemoveInvalidFileNameChars(rawName.Replace(" ", "_"));

                var fileName = $"products_{cleanName}.txt";
                var contentType = "text/plain";
                var fileBytes = Encoding.UTF8.GetBytes(sb.ToString());

                // Save file to /dtmb folder
                var dtmbPath = Path.Combine(_webHostEnvironment.ContentRootPath, "ExportedProducts");
                Directory.CreateDirectory(dtmbPath);
                var fullPath = Path.Combine(dtmbPath, fileName);
                System.IO.File.WriteAllBytes(fullPath, fileBytes);

                // Return file to user
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error exporting products: {ex.Message}" });
            }
        }

        private string EscapeTxt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";
            return value.Replace(";", ",").Replace(Environment.NewLine, " ").Trim();
        }

        private string RemoveInvalidFileNameChars(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(name.Where(ch => !invalidChars.Contains(ch)).ToArray());
        }
    }
}
