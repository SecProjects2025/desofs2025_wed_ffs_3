using AMAP.Business.Components;
using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using AMAP.UnitTests.Business.Mock;
using AMAP_API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using System.Security.Claims;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace AMAP.UnitTests.Services
{
    public class PaymentControllerTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailSender _emailSender;
        private readonly ILogger<PaymentBc> _logger;
        private readonly PaymentBc _paymentBc;
        private readonly PaymentController _controller;
        private readonly ILogger<PaymentController> _loggerController;
        private readonly IPaymentBc _mockPaymentBc;

        public PaymentControllerTests()
        {
            _unitOfWork = MockUnitOfWork.GetMockUnitOfWork();

            // Mock IConfiguration for EmailSender
            var configuration = new Mock<IConfiguration>();
            var mailSettingsSection = new Mock<IConfigurationSection>();

            mailSettingsSection.Setup(x => x["Host"]).Returns("smtp.example.com");
            mailSettingsSection.Setup(x => x["Port"]).Returns("587");
            mailSettingsSection.Setup(x => x["Username"]).Returns("test@example.com");
            mailSettingsSection.Setup(x => x["Password"]).Returns("test-password");
            mailSettingsSection.Setup(x => x["FromEmail"]).Returns("test@example.com");
            mailSettingsSection.Setup(x => x["FromName"]).Returns("Test Sender");

            configuration.Setup(x => x.GetSection("MailSettings")).Returns(mailSettingsSection.Object);

            _emailSender = new EmailSender(configuration.Object);
            _logger = Substitute.For<ILogger<PaymentBc>>();
            _paymentBc = new PaymentBc(_unitOfWork, _logger);
            _loggerController = Substitute.For<ILogger<PaymentController>>();
            _mockPaymentBc = Substitute.For<IPaymentBc>();
            _controller = new PaymentController(_unitOfWork, _loggerController, _mockPaymentBc);

            // Set up a ClaimsPrincipal for the controller to use
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-coproducer-id"),
                new Claim(ClaimTypes.Role, "CoProducer")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Create and set up HttpContext with our ClaimsPrincipal
            _controller.ControllerContext = new ControllerContext
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
                AmountPaid = 100,
                PaymentDate = DateTime.Now
            };
            _mockPaymentBc.CreatePayment(Arg.Any<Payment>(), Arg.Any<string>()).Returns(payment);

            // Act
            var result = _controller.CreatePayment(payment);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Payment>(createdResult.Value);
            Assert.Equal(payment.SubscriptionId, returnValue.SubscriptionId);
        }

        [Fact]
        public void CreatePayment_NullPayment_ReturnsBadRequest()
        {
            // Act
            var result = _controller.CreatePayment(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public void CreatePayment_InvalidPaymentData_ReturnsBadRequest()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 1,
                AmountPaid = -100, // Invalid amount
                PaymentDate = DateTime.Now
            };
            _mockPaymentBc.CreatePayment(Arg.Any<Payment>(), Arg.Any<string>())
                .Returns(x => throw new ArgumentException("Invalid payment data"));

            // Act
            var result = _controller.CreatePayment(payment);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public void CreatePayment_SubscriptionNotFound_ReturnsNotFound()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 999, // Non-existent subscription
                AmountPaid = 100,
                PaymentDate = DateTime.Now
            };
            _mockPaymentBc.CreatePayment(Arg.Any<Payment>(), Arg.Any<string>())
                .Returns(x => throw new KeyNotFoundException("Subscription not found"));

            // Act
            var result = _controller.CreatePayment(payment);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public void CreatePayment_UnauthorizedAccess_ReturnsForbidden()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 1,
                AmountPaid = 100,
                PaymentDate = DateTime.Now
            };
            _mockPaymentBc.CreatePayment(Arg.Any<Payment>(), Arg.Any<string>())
                .Returns(x => throw new UnauthorizedAccessException());

            // Act
            var result = _controller.CreatePayment(payment);

            // Assert
            var forbiddenResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbiddenResult.StatusCode);
            Assert.NotNull(forbiddenResult.Value);
        }

        [Fact]
        public void CreatePayment_ServerError_ReturnsInternalServerError()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 1,
                AmountPaid = 100,
                PaymentDate = DateTime.Now
            };
            _mockPaymentBc.CreatePayment(Arg.Any<Payment>(), Arg.Any<string>())
                .Returns(x => throw new Exception("Server error"));

            // Act
            var result = _controller.CreatePayment(payment);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverErrorResult.StatusCode);
            Assert.NotNull(serverErrorResult.Value);
        }
    }
}
