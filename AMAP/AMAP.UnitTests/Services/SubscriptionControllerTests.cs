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
    public class SubscriptionControllerTests
    {
        private readonly ISubscriptionBc _subscriptionBc;
        private readonly SubscriptionController _subscriptionController;

        public SubscriptionControllerTests()
        {
            _subscriptionBc = MockSubscriptionBc.GetMockSubscriptionBc();
            _subscriptionController = new SubscriptionController(_subscriptionBc);

            // Set up a ClaimsPrincipal for the controller to use
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-coproducer-id"),
                new Claim(ClaimTypes.Role, "CoProducer")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Create and set up HttpContext with our ClaimsPrincipal
            _subscriptionController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public void CreateSubscription_ValidSubscription_ReturnsCreated()
        {
            // Arrange
            var subscription = new Subscription
            {
                ProductOfferId = 101,
                ExpectedQuantity = 5,
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _subscriptionController.CreateSubscription(subscription);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            var returnValue = Assert.IsType<Subscription>(createdResult.Value);
            Assert.Equal(100, returnValue.Id);
            Assert.Equal("test-coproducer-id", returnValue.CoProducerId);
        }

        [Fact]
        public void CreateSubscription_InvalidSubscription_ReturnsBadRequest()
        {
            // Arrange
            var subscription = new Subscription
            {
                ProductOfferId = 101,
                ExpectedQuantity = 0, // Invalid quantity
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _subscriptionController.CreateSubscription(subscription);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void CreateSubscription_NoCoProducerId_ReturnsUnauthorized()
        {
            // Arrange
            var subscription = new Subscription
            {
                ProductOfferId = 101,
                ExpectedQuantity = 5,
                PaymentType = PaymentType.Money
            };

            // Clear the User claims
            _subscriptionController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = _subscriptionController.CreateSubscription(subscription);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public void CreateSubscription_ProductOfferNotFound_ReturnsNotFound()
        {
            // Arrange
            var subscription = new Subscription
            {
                ProductOfferId = 999, // Non-existing ProductOffer ID
                ExpectedQuantity = 5,
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _subscriptionController.CreateSubscription(subscription);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public void CreateSubscription_SubscriptionPeriodEnded_ReturnsBadRequest()
        {
            // Arrange
            var subscription = new Subscription
            {
                ProductOfferId = 888, // ProductOffer with ended subscription period
                ExpectedQuantity = 5,
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _subscriptionController.CreateSubscription(subscription);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void GetSubscriptionById_ValidId_ReturnsOk()
        {
            // Arrange
            var subscriptionId = 1;

            // Act
            var result = _subscriptionController.GetSubscriptionById(subscriptionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returnValue = Assert.IsType<SubscriptionDto>(okResult.Value);
            Assert.Equal(101, returnValue.ProductOfferId);
            Assert.Equal(5, returnValue.ExpectedQuantity);
            Assert.Equal(PaymentType.Money, returnValue.PaymentType);
        }

        [Fact]
        public void GetSubscriptionById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = _subscriptionController.GetSubscriptionById(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Subscription not found.", notFoundResult.Value);
        }

        [Fact]
        public void GetSubscriptionById_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = -1;

            // Act
            var result = _subscriptionController.GetSubscriptionById(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void GetSubscriptionsByCoProducer_WithSubscriptions_ReturnsOk()
        {
            // Arrange
            // Default setup has a valid co-producer with subscriptions

            // Act
            var result = _subscriptionController.GetSubscriptionsByCoProducer();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var subscriptions = Assert.IsType<List<SubscriptionDto>>(okResult.Value);
            Assert.Equal(2, subscriptions.Count);
        }

        [Fact]
        public void GetSubscriptionsByCoProducer_NoCoProducerId_ReturnsUnauthorized()
        {
            // Arrange
            // Clear the User claims
            _subscriptionController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = _subscriptionController.GetSubscriptionsByCoProducer();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            Assert.Equal("Invalid CoProducer.", unauthorizedResult.Value);
        }

        [Fact]
        public void GetSubscriptionsByCoProducer_NoSubscriptions_ReturnsNotFound()
        {
            // Arrange
            // Set up a co-producer with no subscriptions
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "no-subscription-coproducer")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _subscriptionController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = _subscriptionController.GetSubscriptionsByCoProducer();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("No subscriptions found.", notFoundResult.Value);
        }
    }
}
