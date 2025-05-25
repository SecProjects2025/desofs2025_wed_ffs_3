using AMAP_API.Controllers;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.UnitTests.Business.Mock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using System.Security.Claims;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Services
{
    public class PaymentControllerTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaymentController _paymentController;

        public PaymentControllerTests()
        {
            _unitOfWork = MockUnitOfWork.GetMockUnitOfWork();
            _paymentController = new PaymentController(_unitOfWork);

            // Set up a ClaimsPrincipal for the controller to use
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-coproducer-id"),
                new Claim(ClaimTypes.Role, "CoProducer")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Create and set up HttpContext with our ClaimsPrincipal
            _paymentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public void CreatePayment_ValidPayment_ReturnsCreated()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 1,
                AmountPaid = 50.0,
                PaymentDate = DateTime.Now
            };

            // Act
            var result = _paymentController.CreatePayment(payment);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            var returnValue = Assert.IsType<Payment>(createdResult.Value);
            Assert.Equal(100, returnValue.Id);
        }

        [Fact]
        public void CreatePayment_NullPayment_ReturnsBadRequest()
        {
            // Arrange
            Payment payment = null;

            // Act
            var result = _paymentController.CreatePayment(payment);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void CreatePayment_InvalidPaymentData_ReturnsBadRequest()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 1,
                AmountPaid = 0, // Invalid amount
                PaymentDate = DateTime.Now
            };

            // Act
            var result = _paymentController.CreatePayment(payment);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void CreatePayment_SubscriptionNotFound_ReturnsNotFound()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 999, // Non-existing Subscription ID
                AmountPaid = 50.0,
                PaymentDate = DateTime.Now
            };

            // Act
            var result = _paymentController.CreatePayment(payment);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public void CreatePayment_UnauthorizedAccess_ReturnsForbidden()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 888, // Subscription not belonging to the co-producer
                AmountPaid = 50.0,
                PaymentDate = DateTime.Now
            };

            // Act
            var result = _paymentController.CreatePayment(payment);

            // Assert
            var forbiddenResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbiddenResult.StatusCode);

            // Convert to dictionary to access the anonymous type properties
            var properties = Assert.IsAssignableFrom<IDictionary<string, object>>(
                new RouteValueDictionary(forbiddenResult.Value));

            Assert.True(properties.ContainsKey("message"));
            Assert.Contains("You are not authorized", properties["message"].ToString());
        }


        [Fact]
        public void CreatePayment_NoCoProducerId_ReturnsForbidden()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 1,
                AmountPaid = 50.0,
                PaymentDate = DateTime.Now
            };

            // Clear the User claims
            _paymentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = _paymentController.CreatePayment(payment);

            // Assert
            var forbiddenResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbiddenResult.StatusCode);
        }

        [Fact]
        public void CreatePayment_ServerError_ReturnsInternalServerError()
        {
            // Arrange - Create a substitute that will throw an unexpected exception
            var errorUnitOfWork = Substitute.For<IUnitOfWork>();
            var mockPaymentRepo = Substitute.For<IRepository<Payment>>();
            mockPaymentRepo.Get(Arg.Any<Expression<Func<Payment, bool>>>())
                .Returns(x => throw new Exception("Test exception"));
            errorUnitOfWork.Payment.Returns(mockPaymentRepo);

            var errorController = new PaymentController(errorUnitOfWork);

            // Set up claims
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "test-coproducer-id") };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            errorController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var payment = new Payment
            {
                SubscriptionId = 1,
                AmountPaid = 50.0,
                PaymentDate = DateTime.Now
            };

            // Act
            var result = errorController.CreatePayment(payment);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
            Assert.Contains("An error occurred", errorResult.Value.ToString());
        }
    }
}
