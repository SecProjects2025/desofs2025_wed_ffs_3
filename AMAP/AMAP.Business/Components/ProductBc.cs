using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace AMAP.Business
{
    public class ProductBc : IProductBc
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductBc> _logger;

        public ProductBc(IUnitOfWork unitOfWork, ILogger<ProductBc> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public Product? GetProduct(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Product ID must be greater than zero.", nameof(id));

            var product = _unitOfWork.Product.Get(
                p => p.Id == id && !p.IsDeleted,
                "Composites.SubProduct"
            );

            // Log de acesso a dados de produto
            if (product != null)
            {
                _logger.LogInformation("Product data accessed - ID: {ProductId}, Name: {ProductName}, ProducerId: {ProducerId} at {Time}",
                    product.Id, product.Name, product.ProducerId, DateTime.UtcNow);
            }

            return product;
        }

        public Product CreateProduct(Product product, string producerId)
        {
            if (product == null)
                throw new ArgumentException("Invalid product data.", nameof(product));

            if (string.IsNullOrEmpty(producerId))
                throw new ArgumentException("Producer not authenticated.", nameof(producerId));

            product.ProducerId = producerId;

            var existingProduct = _unitOfWork.Product.Get(p => p.Name == product.Name && p.ProducerId == producerId && !p.IsDeleted);
            if (existingProduct != null)
                throw new InvalidOperationException("You already have a product with the same name.");

            var composites = product.Composites?.ToList() ?? [];
            product.Composites = null;

            _unitOfWork.Product.Add(product);
            _unitOfWork.Save();

            // Log de criação de produto sensível
            _logger.LogInformation("Product created - ID: {ProductId}, Name: {ProductName}, Producer: {ProducerId} at {Time}",
                product.Id, product.Name, producerId, DateTime.UtcNow);

            foreach (var composite in composites)
            {
                var subProduct = _unitOfWork.Product.Get(p => p.Id == composite.SubProductId && !p.IsDeleted);

                if (subProduct == null)
                    throw new InvalidOperationException($"Subproduct with ID {composite.SubProductId} does not exist.");

                if (subProduct.ProducerId != producerId)
                    throw new InvalidOperationException($"Subproduct with ID {composite.SubProductId} does not belong to the producer.");

                composite.ProductId = product.Id;
                _unitOfWork.Composite.Add(composite);
            }

            if (composites.Count != 0)
                _unitOfWork.Save();

            return product;
        }

        public void UpdateProduct(int id, Product product, string producerId)
        {
            if (product == null)
                throw new ArgumentException("Invalid product data.", nameof(product));

            if (string.IsNullOrEmpty(producerId))
                throw new ArgumentException("Producer not authenticated.", nameof(producerId));

            var existingProduct = _unitOfWork.Product.Get(
                p => p.Id == id && p.ProducerId == producerId && !p.IsDeleted,
                "Composites",
                tracked: true
            );

            if (existingProduct == null)
                throw new KeyNotFoundException("Product not found or you are not authorized to update it.");

            var duplicateProduct = _unitOfWork.Product.Get(p => p.Name == product.Name && p.Id != id && !p.IsDeleted);
            if (duplicateProduct != null)
                throw new InvalidOperationException("A product with the same name already exists for another producer.");

            var producerProduct = _unitOfWork.Product.Get(p => p.Name == product.Name && p.ProducerId == producerId && p.Id != id && !p.IsDeleted);
            if (producerProduct != null)
                throw new InvalidOperationException("You already have a product with the same name.");

            // Log de atualização de produto sensível - registo antes da modificação
            _logger.LogInformation("Product update initiated - ID: {ProductId}, OldName: {OldName}, NewName: {NewName}, Producer: {ProducerId} at {Time}",
                existingProduct.Id, existingProduct.Name, product.Name, producerId, DateTime.UtcNow);

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
                        throw new InvalidOperationException($"Invalid subproduct with ID {composite.SubProductId} or it does not belong to the producer.");
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

            // Log de atualização de produto sensível - após sucesso da operação
            _logger.LogInformation("Product updated successfully - ID: {ProductId}, Name: {ProductName}, Producer: {ProducerId} at {Time}",
                existingProduct.Id, existingProduct.Name, producerId, DateTime.UtcNow);
        }

        public void DeleteProduct(int id, string producerId)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid product id.", nameof(id));

            if (string.IsNullOrEmpty(producerId))
                throw new ArgumentException("Producer not authenticated.", nameof(producerId));

            var product = _unitOfWork.Product.Get(p => p.Id == id && p.ProducerId == producerId && !p.IsDeleted);

            if (product == null)
                throw new KeyNotFoundException("Product not found or you are not authorized to delete it.");

            // Log de deleção de produto sensível
            _logger.LogInformation("Product deletion initiated - ID: {ProductId}, Name: {ProductName}, Producer: {ProducerId} at {Time}",
                product.Id, product.Name, producerId, DateTime.UtcNow);

            var compositesToDelete = _unitOfWork.Composite.GetAll(c => c.ProductId == id);
            if (compositesToDelete.Any())
            {
                _unitOfWork.Composite.RemoveRange(compositesToDelete);
                _unitOfWork.Save();
            }

            product.IsDeleted = true;
            _unitOfWork.Product.Update(product);
            _unitOfWork.Save();

            // Log após deleção concluída
            _logger.LogInformation("Product marked as deleted - ID: {ProductId}, Producer: {ProducerId} at {Time}",
                id, producerId, DateTime.UtcNow);
        }

        public (List<Product> ImportedProducts, List<string> Errors) ImportProductsFromFile(Stream fileStream, string fileName, string producerId)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("No file was uploaded.", nameof(fileStream));

            if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("File must be text format.", nameof(fileName));

            if (string.IsNullOrEmpty(producerId))
                throw new ArgumentException("Producer not authenticated.", nameof(producerId));

            // Log tentativa de importação
            _logger.LogInformation("Product import initiated - File: {FileName}, Producer: {ProducerId} at {Time}",
                fileName, producerId, DateTime.UtcNow);

            var importedProducts = new List<Product>();
            var errors = new List<string>();
            var duplicateProductFound = false;
            const int MaxLinesToProcess = 10000; // Define a reasonable maximum limit
            int safeMaxLines = Math.Min(MaxLinesToProcess, 10000); // Ensuring we never exceed the maximum limit

            // First, validate the file content without saving
            using (var reader = new StreamReader(fileStream, leaveOpen: true))
            {
                string? line;
                int lineNumber = 0;
                int processedLines = 0;

                while (processedLines < safeMaxLines && (line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    processedLines++;

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

                // Check if we hit the limit
                if (lineNumber >= safeMaxLines && reader.ReadLine() != null)
                {
                    var message = $"File exceeds the maximum allowed lines ({safeMaxLines}).";
                    _logger.LogWarning("Product import failed - {Message}, Producer: {ProducerId} at {Time}",
                        message, producerId, DateTime.UtcNow);
                    throw new InvalidOperationException(message);
                }
            }

            if (duplicateProductFound)
            {
                var message = "Import failed. Found duplicate product names.";
                _logger.LogWarning("Product import failed - {Message}, Producer: {ProducerId} at {Time}",
                    message, producerId, DateTime.UtcNow);
                throw new InvalidOperationException(message) { Data = { { "Errors", errors } } };
            }

            // Prepare a safe file path first
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "ProductsUpload");
            Directory.CreateDirectory(uploadsPath);
            string canonicalUploadsPath = Path.GetFullPath(uploadsPath);

            // Sanitize the filename to prevent path traversal attacks
            string safeFileName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(safeFileName))
                safeFileName = $"import_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            // Generate a unique filename to prevent overwriting
            string uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";

            // Create the safe validated path
            string validatedFilePath = Path.Combine(canonicalUploadsPath, uniqueFileName);

            // We're sure this path is safe now because we constructed it from controlled input
            // and it's within our uploads directory

            fileStream.Position = 0;
            using (var file = new FileStream(validatedFilePath, FileMode.Create))
            {
                fileStream.CopyTo(file);
            }

            // Process the saved file with the validated path
            using (var reader = new StreamReader(validatedFilePath))
            {
                string? line;
                int lineNumber = 0;
                int processedLines = 0;

                while (processedLines < safeMaxLines && (line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    processedLines++;

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

                        // Log de produto importado
                        _logger.LogInformation("Product imported - ID: {ProductId}, Name: {ProductName}, Producer: {ProducerId} at {Time}",
                            product.Id, product.Name, producerId, DateTime.UtcNow);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Line {lineNumber}: Processing error - {ex.Message}");
                        _logger.LogError(ex, "Error importing product at line {Line}, Producer: {ProducerId}",
                            lineNumber, producerId);
                    }
                }
            }

            // Log resultado final da importação
            _logger.LogInformation("Product import completed - Imported: {Count}, Errors: {ErrorCount}, Producer: {ProducerId} at {Time}",
                importedProducts.Count, errors.Count, producerId, DateTime.UtcNow);

            return (importedProducts, errors);
        }

        public (byte[] FileContent, string FileName, string ContentType) ExportProductsToFile(string producerId, string contentRootPath)
        {
            if (string.IsNullOrEmpty(producerId))
                throw new ArgumentException("Producer ID cannot be empty.", nameof(producerId));

            var producer = _unitOfWork.AppUser.Get(p => p.Id == producerId);
            if (producer == null)
                throw new KeyNotFoundException("Producer not found.");

            // Log tentativa de exportação
            _logger.LogInformation("Product export initiated - Producer: {ProducerId}, ProducerName: {ProducerName} at {Time}",
                producerId, producer.Name, DateTime.UtcNow);

            var products = _unitOfWork.Product.GetAll(p => p.ProducerId == producerId && !p.IsDeleted).ToList();

            if (products == null || !products.Any())
                throw new InvalidOperationException("No products found to export.");

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

            // Save file to folder
            var exportPath = Path.Combine(contentRootPath, "ExportedProducts");
            Directory.CreateDirectory(exportPath);
            var fullPath = Path.Combine(exportPath, fileName);
            File.WriteAllBytes(fullPath, fileBytes);

            // Log sucesso na exportação
            _logger.LogInformation("Product export completed - Producer: {ProducerId}, ProductCount: {Count}, FileName: {FileName} at {Time}",
                producerId, products.Count, fileName, DateTime.UtcNow);

            return (fileBytes, fileName, contentType);
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