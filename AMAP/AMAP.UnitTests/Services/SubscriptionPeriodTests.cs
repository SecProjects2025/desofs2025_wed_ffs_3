using AMAP.Api.Controllers;
using AMAP.Business.Interfaces;
using AMAP.Entities;
using AMAP.UnitTests.Business.Mock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AMAP.UnitTests.Services
{
    public class SubscriptionPeriodControllerTests
    {
        private readonly ISubscriptionPeriodBc _subscriptionPeriodBc;
        private readonly SubscriptionPeriodController _subscriptionPeriodController;

        public SubscriptionPeriodControllerTests()
        {
            _subscriptionPeriodBc = MockSubscriptionPeriodBc.GetMockSubscriptionPeriodBc();
            _subscriptionPeriodController = new SubscriptionPeriodController(_subscriptionPeriodBc);

            // Setup controller context with claims for authorized actions
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Role, "Manager") // For authorized endpoints
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _subscriptionPeriodController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public void GetSubscriptionPeriodById_ValidId_ReturnsOk()
        {
            // Arrange
            var id = 1;

            // Act
            var result = _subscriptionPeriodController.GetSubscriptionPeriodById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Verify the returned DTO has the correct properties
            dynamic dto = okResult.Value;
            Assert.Equal("Monthly", dto.Duration);
            Assert.NotNull(dto.DeliveryDates);
        }

        [Fact]
        public void GetSubscriptionPeriodById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = _subscriptionPeriodController.GetSubscriptionPeriodById(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public void GetSubscriptionPeriodById_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = 0; 

            // Act
            var result = _subscriptionPeriodController.GetSubscriptionPeriodById(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void GetSubscriptionPeriodById_WithNoDeliveryDates_ReturnsOkWithEmptyDates()
        {
            // Arrange
            var id = 2; // ID with no delivery dates

            // Act
            var result = _subscriptionPeriodController.GetSubscriptionPeriodById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            dynamic dto = okResult.Value;
            Assert.Equal("Weekly", dto.Duration);
            Assert.Empty(dto.DeliveryDates);
        }

        [Fact]
        public void GetDeliveryDateById_ValidId_ReturnsOk()
        {
            // Arrange
            var id = 1;

            // Act
            var result = _subscriptionPeriodController.GetDeliveryDateById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            dynamic dto = okResult.Value;
            Assert.NotNull(dto.Date);
        }

        [Fact]
        public void GetDeliveryDateById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = _subscriptionPeriodController.GetDeliveryDateById(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public void GetDeliveryDateById_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = 0; // Invalid ID (must be > 0)

            // Act
            var result = _subscriptionPeriodController.GetDeliveryDateById(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateSubscriptionPeriodAndDeliveryDate_ValidData_ReturnsCreated()
        {
            // Arrange
            var subscriptionPeriod = new SubscriptionPeriod
            {
                Duration = "Bi-Weekly",
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.AddDays(14).Date,
                DeliveryDates = new List<DeliveryDate>
                {
                    new DeliveryDate { Date = DateTime.Now.AddDays(7) }
                }
            };

            // Act
            var result = await _subscriptionPeriodController.CreateSubscriptionPeriodAndDeliveryDate(subscriptionPeriod);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);

            Assert.Equal(nameof(SubscriptionPeriodController.GetSubscriptionPeriodById), createdResult.ActionName);
            Assert.Equal(100, createdResult.RouteValues["id"]);

            var createdPeriod = Assert.IsType<SubscriptionPeriod>(createdResult.Value);
            Assert.Equal("Bi-Weekly", createdPeriod.Duration);
            Assert.NotEmpty(createdPeriod.DeliveryDates);
        }

        [Fact]
        public async Task CreateSubscriptionPeriodAndDeliveryDate_InvalidDateRange_ReturnsBadRequest()
        {
            // Arrange
            var subscriptionPeriod = new SubscriptionPeriod
            {
                Duration = "Monthly",
                StartDate = DateTime.Now.AddDays(10).Date,
                EndDate = DateTime.Now.Date, // End date before start date
                DeliveryDates = new List<DeliveryDate>
                {
                    new DeliveryDate { Date = DateTime.Now.AddDays(5) }
                }
            };

            // Act
            var result = await _subscriptionPeriodController.CreateSubscriptionPeriodAndDeliveryDate(subscriptionPeriod);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateSubscriptionPeriodAndDeliveryDate_MissingDuration_ReturnsBadRequest()
        {
            // Arrange
            var subscriptionPeriod = new SubscriptionPeriod
            {
                Duration = "", // Empty duration
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.AddMonths(1).Date,
                DeliveryDates = new List<DeliveryDate>
                {
                    new DeliveryDate { Date = DateTime.Now.AddDays(7) }
                }
            };

            // Act
            var result = await _subscriptionPeriodController.CreateSubscriptionPeriodAndDeliveryDate(subscriptionPeriod);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task DeleteSubscriptionPeriod_ValidId_ReturnsOk()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await _subscriptionPeriodController.DeleteSubscriptionPeriod(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteSubscriptionPeriod_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = await _subscriptionPeriodController.DeleteSubscriptionPeriod(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteSubscriptionPeriod_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = 0;

            // Act
            var result = await _subscriptionPeriodController.DeleteSubscriptionPeriod(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
    }
}
