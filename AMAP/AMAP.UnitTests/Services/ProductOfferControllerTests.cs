using AMAP.Api.Controllers;
using AMAP.Business.Interfaces;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using AMAP.UnitTests.Business.Mock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AMAP.UnitTests.Services
{
    public class ProductOfferControllerTests
    {
        private readonly IProductOfferBc _productOfferBc;
        private readonly ProductOfferController _productOfferController;

        public ProductOfferControllerTests()
        {
            _productOfferBc = MockProductOfferBc.GetMockProductOfferBc();
            _productOfferController = new ProductOfferController(_productOfferBc);

            // Setup controller context with claims for authorized actions
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-producer-id"),
                new Claim(ClaimTypes.Role, "Producer") // For authorized endpoints
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _productOfferController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public void CreateProductOffer_ValidOffer_ReturnsCreated()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 15.99,
                Quantity = 20,
                DeliveryDate = DateTime.Now.AddDays(14),
                PaymentType = PaymentType.Money | PaymentType.MBway
            };

            // Act
            var result = _productOfferController.CreateProductOffer(productOffer);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);

            Assert.Equal(nameof(ProductOfferController.GetProductOfferById), createdResult.ActionName);
            Assert.Equal(100, createdResult.RouteValues["id"]);

            var createdOffer = Assert.IsType<ProductOffer>(createdResult.Value);
            Assert.Equal(15.99, createdOffer.Price);
            Assert.Equal(20, createdOffer.Quantity);
        }

        [Fact]
        public void CreateProductOffer_NullOffer_ReturnsBadRequest()
        {
            // Arrange
            ProductOffer productOffer = null;

            // Act
            var result = _productOfferController.CreateProductOffer(productOffer);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Invalid data.", badRequestResult.Value);
        }

        [Fact]
        public void CreateProductOffer_NoProducerId_ReturnsBadRequest()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 15.99,
                Quantity = 20,
                DeliveryDate = DateTime.Now.AddDays(14),
                PaymentType = PaymentType.Money 
            };

            // Clear the User claims
            _productOfferController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = _productOfferController.CreateProductOffer(productOffer);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Producer not authenticated.", badRequestResult.Value); 
        }


        [Fact]
        public void CreateProductOffer_ProductNotFound_ReturnsNotFound()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 0, // Invalid product ID
                SubscriptionPeriodId = 1,
                Price = 15.99,
                Quantity = 20,
                DeliveryDate = DateTime.Now.AddDays(14),
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _productOfferController.CreateProductOffer(productOffer);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Product not found.", notFoundResult.Value);
        }

        [Fact]
        public void CreateProductOffer_ProductFromAnotherProducer_ReturnsBadRequest()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 999, // Product from another producer
                SubscriptionPeriodId = 1,
                Price = 15.99,
                Quantity = 20,
                DeliveryDate = DateTime.Now.AddDays(14),
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _productOfferController.CreateProductOffer(productOffer);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("You cannot create an offer for a product from another producer.", badRequestResult.Value);
        }

        [Fact]
        public void CreateProductOffer_SubscriptionPeriodNotFound_ReturnsNotFound()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 999, // Non-existing subscription period
                Price = 15.99,
                Quantity = 20,
                DeliveryDate = DateTime.Now.AddDays(14),
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _productOfferController.CreateProductOffer(productOffer);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Subscription period not found.", notFoundResult.Value);
        }

        [Fact]
        public void CreateProductOffer_PastDeliveryDate_ReturnsBadRequest()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 15.99,
                Quantity = 20,
                DeliveryDate = DateTime.Now.AddDays(-1), // Past date
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _productOfferController.CreateProductOffer(productOffer);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Delivery date cannot be in the past.", badRequestResult.Value);
        }

        [Fact]
        public void CreateProductOffer_DuplicateOffer_ReturnsBadRequest()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 5, // Product ID that would cause duplication
                SubscriptionPeriodId = 1,
                Price = 15.99,
                Quantity = 20,
                DeliveryDate = DateTime.Now.AddDays(7), // This date causes duplication
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _productOfferController.CreateProductOffer(productOffer);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("A product offer already exists for this product on the selected delivery date.", badRequestResult.Value);
        }

        [Fact]
        public void CreateProductOffer_InvalidQuantity_ReturnsBadRequest()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 15.99,
                Quantity = 0, // Invalid quantity
                DeliveryDate = DateTime.Now.AddDays(14),
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _productOfferController.CreateProductOffer(productOffer);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Product offer quantity must be greater than zero.", badRequestResult.Value);
        }

        [Fact]
        public void CreateProductOffer_InvalidPaymentType_ReturnsBadRequest()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 15.99,
                Quantity = 20,
                DeliveryDate = DateTime.Now.AddDays(14),
                PaymentType = 0 // Invalid payment type
            };

            // Act
            var result = _productOfferController.CreateProductOffer(productOffer);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Invalid payment type.", badRequestResult.Value);
        }

        [Fact]
        public void CreateProductOffer_InternalServerError_Returns500()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 777, 
                SubscriptionPeriodId = 1,
                Price = 15.99,
                Quantity = 20,
                DeliveryDate = DateTime.Now.AddDays(14),
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _productOfferController.CreateProductOffer(productOffer);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }


        [Fact]
        public void GetProductOfferById_ValidId_ReturnsOk()
        {
            // Arrange
            var id = 1;

            // Act
            var result = _productOfferController.GetProductOfferById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            dynamic dto = okResult.Value;
            Assert.NotNull(dto);
            Assert.Equal(12.99, dto.Price);
            Assert.Equal(10, dto.Quantity);
        }

        [Fact]
        public void GetProductOfferById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = _productOfferController.GetProductOfferById(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Product offer not found.", notFoundResult.Value);
        }

        [Fact]
        public void GetProductOfferById_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = 0; 

            // Act
            var result = _productOfferController.GetProductOfferById(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Contains("must be greater than zero", badRequestResult.Value.ToString());
        }

        [Fact]
        public void GetProductOfferByProducer_ValidId_ReturnsOk()
        {
            // Arrange
            var producerId = "test-producer-id";

            // Act
            var result = _productOfferController.GetProductOfferByProducer(producerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            dynamic dto = okResult.Value;
            Assert.NotNull(dto);
            Assert.Equal(12.99, dto.Price);
            Assert.Equal(10, dto.Quantity);
        }

        [Fact]
        public void GetProductOfferByProducer_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = "non-existing-producer";

            // Act
            var result = _productOfferController.GetProductOfferByProducer(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Product offer not found.", notFoundResult.Value);
        }

        [Fact]
        public void GetProductOfferByProducer_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            string emptyProducerId = "";

            // Act
            var result = _productOfferController.GetProductOfferByProducer(emptyProducerId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void DeleteProductOffer_ValidId_ReturnsOk()
        {
            // Arrange
            var id = 1;

            // Act
            var result = _productOfferController.DeleteProductOffer(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Product offer deleted successfully.", okResult.Value);
        }

        [Fact]
        public void DeleteProductOffer_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = _productOfferController.DeleteProductOffer(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Product offer not found.", notFoundResult.Value);
        }

        [Fact]
        public void DeleteProductOffer_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = 0; 

            // Act
            var result = _productOfferController.DeleteProductOffer(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void DeleteProductOffer_WithActiveSubscriptions_ReturnsBadRequest()
        {
            // Arrange
            var idWithSubscriptions = 50;

            // Act
            var result = _productOfferController.DeleteProductOffer(idWithSubscriptions);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Cannot delete product offer with active subscriptions.", badRequestResult.Value);
        }
    }
}
