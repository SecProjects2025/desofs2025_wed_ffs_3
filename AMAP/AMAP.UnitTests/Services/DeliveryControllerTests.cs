using AMAP.Api.Controllers;
using AMAP.Business.Interfaces;
using AMAP.UnitTests.Business.Mock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace AMAP.UnitTests.Services
{
    public class DeliveryControllerTests
    {
        private readonly IDeliveryBc _deliveryBc;
        private readonly DeliveryController _deliveryController;

        public DeliveryControllerTests()
        {
            _deliveryBc = MockDeliveryBc.GetMockDeliveryBc();
            _deliveryController = new DeliveryController(_deliveryBc);

            // Set up a ClaimsPrincipal for the controller to use
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-producer-id"),
                new Claim(ClaimTypes.Role, "Producer")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Create and set up HttpContext with our ClaimsPrincipal
            _deliveryController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public void UpdateDeliveredQuantity_ValidData_ReturnsOk()
        {
            // Arrange
            var subscriptionId = 1;
            var deliveredQuantity = 10;

            // Act
            var result = _deliveryController.UpdateDeliveredQuantity(subscriptionId, deliveredQuantity);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Delivered quantity updated successfully.", okResult.Value);
        }

        [Fact]
        public void UpdateDeliveredQuantity_InvalidSubscriptionId_ReturnsBadRequest()
        {
            // Arrange
            var subscriptionId = 0; // Invalid ID
            var deliveredQuantity = 10;

            // Act
            var result = _deliveryController.UpdateDeliveredQuantity(subscriptionId, deliveredQuantity);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Contains("Invalid subscription ID", badRequestResult.Value.ToString());
        }

        [Fact]
        public void UpdateDeliveredQuantity_NegativeQuantity_ReturnsBadRequest()
        {
            // Arrange
            var subscriptionId = 1;
            var deliveredQuantity = -5; // Negative quantity

            // Act
            var result = _deliveryController.UpdateDeliveredQuantity(subscriptionId, deliveredQuantity);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Contains("cannot be negative", badRequestResult.Value.ToString());
        }

        [Fact]
        public void UpdateDeliveredQuantity_SubscriptionNotFound_ReturnsNotFound()
        {
            // Arrange
            var subscriptionId = 500; // Non-existing subscription
            var deliveredQuantity = 10;

            // Act
            var result = _deliveryController.UpdateDeliveredQuantity(subscriptionId, deliveredQuantity);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Contains("Subscription not found", notFoundResult.Value.ToString());
        }

        [Fact]
        public void UpdateDeliveredQuantity_Unauthorized_ReturnsForbid()
        {
            // Arrange
            var subscriptionId = 1000; // Unauthorized subscription
            var deliveredQuantity = 10;

            // Act
            var result = _deliveryController.UpdateDeliveredQuantity(subscriptionId, deliveredQuantity);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void UpdateDeliveredQuantity_NoProducerId_ReturnsForbid()
        {
            // Arrange
            var subscriptionId = 1;
            var deliveredQuantity = 10;

            // Clear the User claims
            _deliveryController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = _deliveryController.UpdateDeliveredQuantity(subscriptionId, deliveredQuantity);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void UpdateDeliveredQuantity_ServerError_ReturnsInternalServerError()
        {
            // Arrange - Create a substitute that will throw an unexpected exception
            var errorDeliveryBc = Substitute.For<IDeliveryBc>();
            errorDeliveryBc
                .When(x => x.UpdateDeliveredQuantity(
                    Arg.Any<int>(),
                    Arg.Any<int>(),
                    Arg.Any<string>()))
                .Throw(new Exception("Unexpected server error"));

            var errorController = new DeliveryController(errorDeliveryBc);

            // Set up claims
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "test-producer-id") };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            errorController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = errorController.UpdateDeliveredQuantity(1, 10);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
            Assert.Contains("An error occurred", errorResult.Value.ToString());
        }
    }
}
