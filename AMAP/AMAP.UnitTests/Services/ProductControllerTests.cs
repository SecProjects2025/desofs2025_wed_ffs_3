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

        [Fact]
        public void ImportProductsFromFile_InvalidFileFormat_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(1000);
            mockFile.ContentType.Returns("text/plain");
            mockFile.FileName.Returns("products.csv"); // Not a .txt file

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("Test file content"));
            mockFile.OpenReadStream().Returns(memoryStream);

            // Act
            var result = _productController.ImportProductsFromFile(mockFile);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void ExportProductsToFile_ValidRequest_ReturnsFileResult()
        {
            // Arrange

            // Setup the export behavior to return a test file
            var fileContent = Encoding.UTF8.GetBytes("Name,Description,Photo,Price,Units\r\nProduct1,Desc1,photo1.jpg,10.99,kg");
            var fileName = "products_export.csv";
            var contentType = "text/csv";

            _productBc.ExportProductsToFile("test-producer-id", "/app/root")
                .Returns((fileContent, fileName, contentType));

            // Act
            var result = _productController.ExportProductsToFile();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(fileContent, fileResult.FileContents);
            Assert.Equal(fileName, fileResult.FileDownloadName);
            Assert.Equal(contentType, fileResult.ContentType);
        }
    }
}