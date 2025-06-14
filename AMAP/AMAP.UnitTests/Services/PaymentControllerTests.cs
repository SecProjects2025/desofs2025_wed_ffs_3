using AMAP.Business.Components;
using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using AMAP.UnitTests.Business.Mock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using System.Security.Claims;
using System.Linq.Expressions;
using AMAP.API.Controllers;

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

        #region ValidatePayment Tests

        [Fact]
        public void ValidatePayment_PaymentNotFound_ReturnsNotFound()
        {
            // Arrange
            int nonExistentPaymentId = 999;
            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            mockUnitOfWork.Payment.Get(Arg.Any<Expression<Func<Payment, bool>>>()).Returns((Payment)null);

            var controller = new PaymentController(mockUnitOfWork, _loggerController, _mockPaymentBc);
            SetupProducerContext(controller);

            // Act
            var result = controller.ValidatePayment(nonExistentPaymentId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains($"Payment with ID {nonExistentPaymentId} not found", notFoundResult.Value.ToString());
        }

        [Fact]
        public void ValidatePayment_AlreadyValidated_ReturnsBadRequest()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment
            {
                Id = paymentId,
                SubscriptionId = 1,
                AmountPaid = 100,
                PaymentDate = DateTime.Now,
                Validated = true
            };

            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            mockUnitOfWork.Payment.Get(Arg.Any<Expression<Func<Payment, bool>>>()).Returns(payment);

            var controller = new PaymentController(mockUnitOfWork, _loggerController, _mockPaymentBc);
            SetupProducerContext(controller);

            // Act
            var result = controller.ValidatePayment(paymentId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Payment is already validated.", badRequestResult.Value);
        }

        [Fact]
        public void ValidatePayment_ValidPayment_ReturnsOk()
        {
            // Arrange
            int paymentId = 1;
            string producerId = "test-producer-id";

            var payment = new Payment
            {
                Id = paymentId,
                SubscriptionId = 1,
                AmountPaid = 100,
                PaymentDate = DateTime.Now,
                Validated = false
            };

            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            mockUnitOfWork.Payment.Get(Arg.Any<Expression<Func<Payment, bool>>>()).Returns(payment);

            var controller = new PaymentController(mockUnitOfWork, _loggerController, _mockPaymentBc);
            SetupProducerContext(controller, producerId);

            // Act
            var result = controller.ValidatePayment(paymentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPayment = Assert.IsType<Payment>(okResult.Value);

            Assert.True(returnedPayment.Validated);
            Assert.NotNull(returnedPayment.ValidationDate);
            Assert.Equal(producerId, returnedPayment.ValidatedByProducerId);

            mockUnitOfWork.PaymentValidationHistory.Received(1).Add(Arg.Is<PaymentValidationHistory>(h =>
                h.PaymentId == paymentId && h.ProducerId == producerId));
            mockUnitOfWork.Received(1).Save();
        }

        [Fact]
        public void ValidatePayment_NoUserIdentity_ReturnsUnauthorized()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment
            {
                Id = paymentId,
                SubscriptionId = 1,
                AmountPaid = 100,
                PaymentDate = DateTime.Now,
                Validated = false
            };

            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            mockUnitOfWork.Payment.Get(Arg.Any<Expression<Func<Payment, bool>>>()).Returns(payment);

            var controller = new PaymentController(mockUnitOfWork, _loggerController, _mockPaymentBc);
            // Setup context without a user identity
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            // Act
            var result = controller.ValidatePayment(paymentId);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User identity not found or invalid.", unauthorizedResult.Value);
        }

        [Fact]
        public void ValidatePayment_Exception_ReturnsInternalServerError()
        {
            // Arrange
            int paymentId = 1;

            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            mockUnitOfWork.Payment.Get(Arg.Any<Expression<Func<Payment, bool>>>()).Returns(x =>
                throw new Exception("Database error"));

            var controller = new PaymentController(mockUnitOfWork, _loggerController, _mockPaymentBc);
            SetupProducerContext(controller);

            // Act
            var result = controller.ValidatePayment(paymentId);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverErrorResult.StatusCode);
            Assert.Contains("An error occurred while validating the payment", serverErrorResult.Value.ToString());
        }

        #endregion

        #region UpdatePayment Tests

        [Fact]
        public void UpdatePayment_PaymentNotFound_ReturnsNotFound()
        {
            // Arrange
            int nonExistentPaymentId = 999;
            var updatedPayment = new Payment
            {
                AmountPaid = 150,
                PaymentMethod = PaymentType.BankTransfer
            };

            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            mockUnitOfWork.Payment.Get(Arg.Any<Expression<Func<Payment, bool>>>()).Returns((Payment)null);

            var controller = new PaymentController(mockUnitOfWork, _loggerController, _mockPaymentBc);
            SetupManagerContext(controller);

            // Act
            var result = controller.UpdatePayment(nonExistentPaymentId, updatedPayment);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Payment not found", notFoundResult.Value);
        }

        [Fact]
        public void UpdatePayment_ValidPayment_ReturnsOk()
        {
            // Arrange
            int paymentId = 1;
            var existingPayment = new Payment
            {
                Id = paymentId,
                SubscriptionId = 1,
                AmountPaid = 100,
                PaymentDate = DateTime.Now,
                Validated = false,
                PaymentMethod = PaymentType.Money
            };

            var updatedPayment = new Payment
            {
                AmountPaid = 150,
                Validated = true,
                ValidationDate = DateTime.UtcNow,
                ValidatedByProducerId = "producer-id",
                PaymentMethod = PaymentType.BankTransfer
            };

            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            mockUnitOfWork.Payment.Get(Arg.Any<Expression<Func<Payment, bool>>>()).Returns(existingPayment);

            var controller = new PaymentController(mockUnitOfWork, _loggerController, _mockPaymentBc);
            SetupManagerContext(controller);

            // Act
            var result = controller.UpdatePayment(paymentId, updatedPayment);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Payment updated successfully", okResult.Value);

            // Check that the existing payment was updated with the new values
            Assert.Equal(updatedPayment.AmountPaid, existingPayment.AmountPaid);
            Assert.Equal(updatedPayment.Validated, existingPayment.Validated);
            Assert.Equal(updatedPayment.ValidationDate, existingPayment.ValidationDate);
            Assert.Equal(updatedPayment.ValidatedByProducerId, existingPayment.ValidatedByProducerId);
            Assert.Equal(updatedPayment.PaymentMethod, existingPayment.PaymentMethod);

            mockUnitOfWork.Payment.Received(1).Update(existingPayment);
            mockUnitOfWork.Received(1).Save();
        }

        [Fact]
        public void UpdatePayment_Exception_ReturnsInternalServerError()
        {
            // Arrange
            int paymentId = 1;
            var updatedPayment = new Payment
            {
                AmountPaid = 150,
                PaymentMethod = PaymentType.BankTransfer
            };

            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            mockUnitOfWork.Payment.Get(Arg.Any<Expression<Func<Payment, bool>>>()).Returns(x =>
                throw new Exception("Database error"));

            var controller = new PaymentController(mockUnitOfWork, _loggerController, _mockPaymentBc);
            SetupManagerContext(controller);

            // Act
            var result = controller.UpdatePayment(paymentId, updatedPayment);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverErrorResult.StatusCode);
            Assert.Contains("An error occurred while updating the payment", serverErrorResult.Value.ToString());
        }

        #endregion

        #region DeletePayment Tests

        [Fact]
        public void DeletePayment_PaymentNotFound_ReturnsNotFound()
        {
            // Arrange
            int nonExistentPaymentId = 999;

            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            mockUnitOfWork.Payment.Get(Arg.Any<Expression<Func<Payment, bool>>>()).Returns((Payment)null);

            var controller = new PaymentController(mockUnitOfWork, _loggerController, _mockPaymentBc);
            SetupManagerContext(controller);

            // Act
            var result = controller.DeletePayment(nonExistentPaymentId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Payment not found", notFoundResult.Value);
        }

        [Fact]
        public void DeletePayment_ValidPayment_ReturnsOk()
        {
            // Arrange
            int paymentId = 1;
            var existingPayment = new Payment
            {
                Id = paymentId,
                SubscriptionId = 1,
                AmountPaid = 100,
                PaymentDate = DateTime.Now
            };

            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            mockUnitOfWork.Payment.Get(Arg.Any<Expression<Func<Payment, bool>>>()).Returns(existingPayment);

            var controller = new PaymentController(mockUnitOfWork, _loggerController, _mockPaymentBc);
            SetupManagerContext(controller);

            // Act
            var result = controller.DeletePayment(paymentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Payment deleted successfully", okResult.Value);

            mockUnitOfWork.Payment.Received(1).Delete(existingPayment);
            mockUnitOfWork.Received(1).Save();
        }

        [Fact]
        public void DeletePayment_Exception_ReturnsInternalServerError()
        {
            // Arrange
            int paymentId = 1;

            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            mockUnitOfWork.Payment.Get(Arg.Any<Expression<Func<Payment, bool>>>()).Returns(x =>
                throw new Exception("Database error"));

            var controller = new PaymentController(mockUnitOfWork, _loggerController, _mockPaymentBc);
            SetupManagerContext(controller);

            // Act
            var result = controller.DeletePayment(paymentId);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverErrorResult.StatusCode);
            Assert.Contains("An error occurred while deleting the payment", serverErrorResult.Value.ToString());
        }

        #endregion

        #region Helper Methods

        private void SetupProducerContext(PaymentController controller, string producerId = "test-producer-id")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, producerId),
                new Claim(ClaimTypes.Role, "Producer")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        private void SetupManagerContext(PaymentController controller)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-manager-id"),
                new Claim(ClaimTypes.Role, "Manager")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        #endregion
    }
}