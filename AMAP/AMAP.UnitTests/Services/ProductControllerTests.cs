using AMAP.Api.Controllers;
using AMAP.Business.Interfaces;
using AMAP.Entities;
using AMAP.UnitTests.Business.Mock;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Security.Claims;
using System.Text;

namespace AMAP.UnitTests.Services
{
    public class ProductControllerTests
    {
        private readonly IProductBc _productBc;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ProductController _productController;

        public ProductControllerTests()
        {
            _productBc = MockProductBc.GetMockProductBc();
            _webHostEnvironment = MockProductBc.GetMockWebHostEnvironment();
            _productController = new ProductController(_productBc, _webHostEnvironment);

            // Set up a ClaimsPrincipal for the controller to use
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-producer-id")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Create and set up HttpContext with our ClaimsPrincipal
            _productController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public void GetProduct_ValidId_ReturnsOk()
        {
            // Arrange
            var productId = 1;  

            // Act
            var result = _productController.GetProduct(productId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);  
        }

        [Fact]
        public void GetProduct_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = 999; 

            // Act
            var result = _productController.GetProduct(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public void CreateProduct_ValidProduct_ReturnsCreated()
        {
            // Arrange
            var product = new Product
            {
                Name = "New Test Product",
                Description = "Test Description",
                Photo = "test.jpg",
                ReferencePrice = 10.99,
                DeliveryUnits = "kg"
            };

            // Act
            var result = _productController.CreateProduct(product);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public void CreateProduct_NullProduct_ReturnsBadRequest()
        {
            // Arrange
            Product product = new Product();

            _productBc.CreateProduct(product, "test-producer-id")
                .Throws(new ArgumentException("Invalid product data.", "product"));

            // Act
            var result = _productController.CreateProduct(product);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void CreateProduct_NullProducerId_ReturnsBadRequest()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "Test Description",
                Photo = "test.jpg",
                ReferencePrice = 10.99,
                DeliveryUnits = "kg"
            };

            // Clear the User claims
            _productController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = _productController.CreateProduct(product);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void CreateProduct_DuplicateProductName_ReturnsConflict()
        {
            // Arrange
            var product = new Product
            {
                Name = "Existing Product",
                Description = "Test Description",
                Photo = "test.jpg",
                ReferencePrice = 10.99,
                DeliveryUnits = "kg"
            };

            // Act
            var result = _productController.CreateProduct(product);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
            Assert.NotNull(conflictResult.Value);
        }

        [Fact]
        public void UpdateProduct_ValidData_ReturnsNoContent()
        {
            // Arrange
            var productId = 1;
            var product = new Product
            {
                Id = productId,
                Name = "Updated Product Name",
                Description = "Updated Description",
                Photo = "updated.jpg",
                ReferencePrice = 15.99,
                DeliveryUnits = "box"
            };

            // Act
            var result = _productController.UpdateProduct(productId, product);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public void UpdateProduct_ProductNotFound_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = 999;
            var product = new Product
            {
                Id = nonExistingId,
                Name = "Non-existing Product",
                Description = "This product does not exist",
                Photo = "nonexisting.jpg",
                ReferencePrice = 9.99,
                DeliveryUnits = "unit"
            };

            // Act
            var result = _productController.UpdateProduct(nonExistingId, product);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public void UpdateProduct_NullProduct_ReturnsBadRequest()
        {
            // Arrange
            var productId = 1;
            Product product = null;

            // Act
            var result = _productController.UpdateProduct(productId, product);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void UpdateProduct_NullProducerId_ReturnsBadRequest()
        {
            // Arrange
            var productId = 1;
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                Description = "Test Description",
                Photo = "test.jpg",
                ReferencePrice = 10.99,
                DeliveryUnits = "kg"
            };

            // Clear the User claims
            _productController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = _productController.UpdateProduct(productId, product);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void UpdateProduct_DuplicateProductName_ReturnsConflict()
        {
            // Arrange
            var productId = 2;
            var product = new Product
            {
                Id = productId,
                Name = "Duplicate Name",
                Description = "This will cause a conflict",
                Photo = "duplicate.jpg",
                ReferencePrice = 12.99,
                DeliveryUnits = "piece"
            };

            // Act
            var result = _productController.UpdateProduct(productId, product);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact]
        public void UpdateProduct_InvalidSubProduct_ReturnsConflict()
        {
            // Arrange
            var productId = 3;
            var product = new Product
            {
                Id = productId,
                Name = "Product With Invalid Subproduct",
                Description = "This has an invalid subproduct",
                Photo = "invalid.jpg",
                ReferencePrice = 14.99,
                DeliveryUnits = "unit",
                Composites = new List<Composite>
                {
                    new Composite { SubProductId = 9999, Quantity = 2 }
                }
            };

            // Act
            var result = _productController.UpdateProduct(productId, product);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact]
        public void DeleteProduct_ValidId_ReturnsNoContent()
        {
            // Arrange
            var productId = 1;

            // Act
            var result = _productController.DeleteProduct(productId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public void DeleteProduct_ProductNotFound_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = _productController.DeleteProduct(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public void DeleteProduct_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = 0; 

            // Act
            var result = _productController.DeleteProduct(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void DeleteProduct_NullProducerId_ReturnsBadRequest()
        {
            // Arrange
            var productId = 1;
            _productController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = _productController.DeleteProduct(productId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        #region ImportProductsFromFile Tests

        [Fact]
        public void ImportProductsFromFile_NullFile_ReturnsBadRequest()
        {
            // Act
            var result = _productController.ImportProductsFromFile(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var value = GetAnonymousValue(badRequestResult.Value);
            Assert.Equal("No file was uploaded", value["message"]);
        }

        [Fact]
        public void ImportProductsFromFile_EmptyFile_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(0);

            // Act
            var result = _productController.ImportProductsFromFile(mockFile);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var value = GetAnonymousValue(badRequestResult.Value);
            Assert.Equal("No file was uploaded", value["message"]);
        }

        [Fact]
        public void ImportProductsFromFile_FileTooLarge_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(6 * 1024 * 1024); // 6 MB (exceeds 5MB limit)
            mockFile.ContentType.Returns("text/plain");

            // Act
            var result = _productController.ImportProductsFromFile(mockFile);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var value = GetAnonymousValue(badRequestResult.Value);
            Assert.Contains("File size exceeds the maximum limit", value["message"].ToString());
        }

        [Fact]
        public void ImportProductsFromFile_InvalidFileType_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(1000);
            mockFile.ContentType.Returns("image/jpeg"); // Not a text file

            // Act
            var result = _productController.ImportProductsFromFile(mockFile);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var value = GetAnonymousValue(badRequestResult.Value);
            Assert.Equal("File must be text format", value["message"]);
        }

        [Fact]
        public void ImportProductsFromFile_NoProducerId_ReturnsUnauthorized()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(1000);
            mockFile.ContentType.Returns("text/plain");

            // Remove producer ID from claims
            _productController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = _productController.ImportProductsFromFile(mockFile);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            var value = GetAnonymousValue(unauthorizedResult.Value);
            Assert.Equal("Producer not authenticated", value["message"]);
        }

        [Fact]
        public void ImportProductsFromFile_ValidFile_ReturnsOkWithImportedData()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(1000);
            mockFile.ContentType.Returns("text/plain");
            mockFile.FileName.Returns("products.txt");

            var fileContent = "Name;Description;Photo;ReferencePrice;DeliveryUnits\nProduct1;Description1;photo1.jpg;10.99;kg";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            mockFile.OpenReadStream().Returns(memoryStream);

            // Setup the expected return value from the business component
            var importedProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Description = "Description1",
                             Photo = "photo1.jpg", ReferencePrice = 10.99, DeliveryUnits = "kg" }
            };
            var errors = new List<string>();
            _productBc.ImportProductsFromFile(Arg.Any<Stream>(), "products.txt", "test-producer-id")
                .Returns((importedProducts, errors));

            // Act
            var result = _productController.ImportProductsFromFile(mockFile);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var value = GetAnonymousValue(okResult.Value);
            Assert.Contains("Import completed", value["message"].ToString());
            Assert.Contains("1", value["message"].ToString()); // 1 product imported

            // Check that the imported products info is returned
            var returnedProducts = (IEnumerable<object>)value["importedProducts"];
            Assert.Single(returnedProducts);

            // Check the errors array is empty
            var returnedErrors = (IEnumerable<object>)value["errors"];
            Assert.Empty(returnedErrors);
        }

        [Fact]
        public void ImportProductsFromFile_PartiallySuccessful_ReturnsOkWithErrorsList()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(1000);
            mockFile.ContentType.Returns("text/plain");
            mockFile.FileName.Returns("products.txt");

            var fileContent = "Name;Description;Photo;ReferencePrice;DeliveryUnits\nProduct1;Description1;photo1.jpg;10.99;kg\nInvalid;;;";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            mockFile.OpenReadStream().Returns(memoryStream);

            // Setup the expected return value from the business component
            var importedProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Description = "Description1",
                             Photo = "photo1.jpg", ReferencePrice = 10.99, DeliveryUnits = "kg" }
            };
            var errors = new List<string> { "Line 2: Invalid format" };
            _productBc.ImportProductsFromFile(Arg.Any<Stream>(), "products.txt", "test-producer-id")
                .Returns((importedProducts, errors));

            // Act
            var result = _productController.ImportProductsFromFile(mockFile);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var value = GetAnonymousValue(okResult.Value);
            Assert.Contains("Import completed", value["message"].ToString());

            // Check that the imported products info is returned
            var returnedProducts = (IEnumerable<object>)value["importedProducts"];
            Assert.Single(returnedProducts);

            // Check the errors list contains the expected error
            var returnedErrors = (IEnumerable<string>)value["errors"];
            Assert.Single(returnedErrors);
            Assert.Contains("Line 2: Invalid format", returnedErrors);
        }

        [Fact]
        public void ImportProductsFromFile_ArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(1000);
            mockFile.ContentType.Returns("text/plain");
            mockFile.FileName.Returns("products.txt");

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
            mockFile.OpenReadStream().Returns(memoryStream);

            // Setup the business component to throw an ArgumentException
            _productBc.ImportProductsFromFile(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new ArgumentException("Invalid file format"));

            // Act
            var result = _productController.ImportProductsFromFile(mockFile);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var value = GetAnonymousValue(badRequestResult.Value);
            Assert.Equal("Invalid file format", value["message"]);
        }

        [Fact]
        public void ImportProductsFromFile_InvalidOperationException_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(1000);
            mockFile.ContentType.Returns("text/plain");
            mockFile.FileName.Returns("products.txt");

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
            mockFile.OpenReadStream().Returns(memoryStream);

            // Setup the business component to throw an InvalidOperationException
            var exceptionWithData = new InvalidOperationException("Import operation failed");
            exceptionWithData.Data["Errors"] = new List<string> { "Error 1", "Error 2" };
            _productBc.ImportProductsFromFile(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(exceptionWithData);

            // Act
            var result = _productController.ImportProductsFromFile(mockFile);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var value = GetAnonymousValue(badRequestResult.Value);
            Assert.Equal("Import operation failed", value["message"]);

            // Check that error details are included
            var returnedErrors = (IEnumerable<string>)value["errors"];
            Assert.Equal(2, returnedErrors.Count());
            Assert.Contains("Error 1", returnedErrors);
            Assert.Contains("Error 2", returnedErrors);
        }

        [Fact]
        public void ImportProductsFromFile_UnexpectedException_ReturnsInternalServerError()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(1000);
            mockFile.ContentType.Returns("text/plain");
            mockFile.FileName.Returns("products.txt");

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
            mockFile.OpenReadStream().Returns(memoryStream);

            // Setup the business component to throw a general exception
            _productBc.ImportProductsFromFile(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Unexpected error"));

            // Act
            var result = _productController.ImportProductsFromFile(mockFile);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var value = GetAnonymousValue(statusCodeResult.Value);
            Assert.Contains("Error processing file", value["message"].ToString());
        }

        #endregion

        #region ExportProductsToFile Tests

        [Fact]
        public void ExportProductsToFile_NoProducerId_ReturnsUnauthorized()
        {
            // Arrange
            _productController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = _productController.ExportProductsToFile();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            var value = GetAnonymousValue(unauthorizedResult.Value);
            Assert.Equal("Producer not authenticated", value["message"]);
        }

        [Fact]
        public void ExportProductsToFile_ValidRequest_ReturnsFileContent()
        {
            // Arrange
            var fileContent = Encoding.UTF8.GetBytes("Name;Description;Photo;ReferencePrice;DeliveryUnits");
            var fileName = "products_test-producer.txt";
            var contentType = "text/plain";

            _productBc.ExportProductsToFile("test-producer-id", _webHostEnvironment.ContentRootPath)
                .Returns((fileContent, fileName, contentType));

            // Act
            var result = _productController.ExportProductsToFile();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(fileContent, fileResult.FileContents);
            Assert.Equal(fileName, fileResult.FileDownloadName);
            Assert.Equal(contentType, fileResult.ContentType);
        }

        [Fact]
        public void ExportProductsToFile_ArgumentException_ReturnsBadRequest()
        {
            // Arrange
            _productBc.ExportProductsToFile(Arg.Any<string>(), Arg.Any<string>())
                .Throws(new ArgumentException("Invalid producer ID"));

            // Act
            var result = _productController.ExportProductsToFile();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var value = GetAnonymousValue(badRequestResult.Value);
            Assert.Equal("Invalid producer ID", value["message"]);
        }

        [Fact]
        public void ExportProductsToFile_KeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            _productBc.ExportProductsToFile(Arg.Any<string>(), Arg.Any<string>())
                .Throws(new KeyNotFoundException("Producer not found"));

            // Act
            var result = _productController.ExportProductsToFile();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            var value = GetAnonymousValue(notFoundResult.Value);
            Assert.Equal("Producer not found", value["message"]);
        }

        [Fact]
        public void ExportProductsToFile_InvalidOperationException_ReturnsNotFound()
        {
            // Arrange
            _productBc.ExportProductsToFile(Arg.Any<string>(), Arg.Any<string>())
                .Throws(new InvalidOperationException("No products found to export"));

            // Act
            var result = _productController.ExportProductsToFile();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            var value = GetAnonymousValue(notFoundResult.Value);
            Assert.Equal("No products found to export", value["message"]);
        }

        [Fact]
        public void ExportProductsToFile_UnexpectedException_ReturnsInternalServerError()
        {
            // Arrange
            _productBc.ExportProductsToFile(Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Database connection error"));

            // Act
            var result = _productController.ExportProductsToFile();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var value = GetAnonymousValue(statusCodeResult.Value);
            Assert.Contains("Error exporting products", value["message"].ToString());
        }

        #endregion

        // Helper method to safely extract values from anonymous objects
        private IDictionary<string, object> GetAnonymousValue(object anonymousObject)
        {
            var result = new Dictionary<string, object>();
            var properties = anonymousObject.GetType().GetProperties();

            foreach (var prop in properties)
            {
                result[prop.Name] = prop.GetValue(anonymousObject);
            }

            return result;
        }
    }
}