using AMAP_API.Controllers;
using AMAP.Business.Interfaces;
using AMAP.Entities;
using AMAP.Entities.Models.DTO.Subscription;
using AMAP.Entities.Utilities;
using AMAP.UnitTests.Business.Mock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AMAP.Business.Components;
using AMAP.DataAccess.Repository.IRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace AMAP.UnitTests.Services
{
    public class SubscriptionControllerTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailSender _emailSender;
        private readonly ILogger<SubscriptionBc> _logger;
        private readonly SubscriptionBc _subscriptionBc;
        private readonly SubscriptionController _controller;
        private readonly ILogger<SubscriptionController> _loggerController;

        public SubscriptionControllerTests()
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
            _logger = Substitute.For<ILogger<SubscriptionBc>>();
            _subscriptionBc = new SubscriptionBc(_unitOfWork, _emailSender, _logger);
            _loggerController = Substitute.For<ILogger<SubscriptionController>>();
            _controller = new SubscriptionController(_subscriptionBc, _loggerController);

            // Set up a ClaimsPrincipal for the controller to use
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-coproducer-id"),
                new Claim(ClaimTypes.Role, "CoProducer"),
                new Claim("CoProducerId", "test-coproducer-id")
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
        public void CreateSubscription_ValidSubscription_ReturnsCreated()
        {
            // Arrange
            var subscription = new Subscription
            {
                CoProducerId = "test-coproducer-id",
                ProductOfferId = 1,
                ExpectedQuantity = 1,
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _controller.CreateSubscription(subscription);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Subscription>(createdResult.Value);
            Assert.Equal(subscription.ProductOfferId, returnValue.ProductOfferId);
            Assert.Equal(subscription.ExpectedQuantity, returnValue.ExpectedQuantity);
            Assert.Equal(subscription.PaymentType, returnValue.PaymentType);
        }

        [Fact]
        public void CreateSubscription_NullSubscription_ReturnsBadRequest()
        {
            // Arrange
            Subscription nullSubscription = null;

            // Act
            var result = _controller.CreateSubscription(nullSubscription);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid subscription data.", badRequestResult.Value);
        }

        [Fact]
        public void CreateSubscription_ZeroQuantity_ReturnsBadRequest()
        {
            // Arrange
            var subscription = new Subscription
            {
                CoProducerId = "test-coproducer-id",
                ProductOfferId = 1,
                ExpectedQuantity = 0,
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _controller.CreateSubscription(subscription);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Quantity must be greater than zero.", badRequestResult.Value);
        }

        [Fact]
        public void CreateSubscription_ProductOfferNotFound_ReturnsNotFound()
        {
            // Arrange
            var subscription = new Subscription
            {
                CoProducerId = "test-coproducer-id",
                ProductOfferId = 999, // Non-existing product offer
                ExpectedQuantity = 1,
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _controller.CreateSubscription(subscription);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Product offer not found.", notFoundResult.Value);
        }

        [Fact]
        public void CreateSubscription_SubscriptionPeriodEnded_ReturnsBadRequest()
        {
            // Arrange
            var subscription = new Subscription
            {
                CoProducerId = "test-coproducer-id",
                ProductOfferId = 888, // Product offer with ended period
                ExpectedQuantity = 1,
                PaymentType = PaymentType.Money
            };

            // Act
            var result = _controller.CreateSubscription(subscription);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Subscription period has ended.", badRequestResult.Value);
        }

        [Fact]
        public void GetSubscriptionById_ValidId_ReturnsOk()
        {
            // Arrange
            var subscriptionId = 1;

            // Act
            var result = _controller.GetSubscriptionById(subscriptionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Subscription>(okResult.Value);
            Assert.Equal(subscriptionId, returnValue.Id);
        }

        [Fact]
        public void GetSubscriptionById_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = 0;

            // Act
            var result = _controller.GetSubscriptionById(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid subscription ID.", badRequestResult.Value);
        }

        [Fact]
        public void GetSubscriptionById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = _controller.GetSubscriptionById(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Subscription not found.", notFoundResult.Value);
        }

        [Fact]
        public void GetSubscriptionsByCoProducer_WithSubscriptions_ReturnsOk()
        {
            // Act
            var result = _controller.GetSubscriptionsByCoProducer("coproducer-1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Subscription>>(okResult.Value);
            Assert.NotEmpty(returnValue);
        }

        [Fact]
        public void GetSubscriptionsByCoProducer_NoSubscriptions_ReturnsNotFound()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "no-subscription-coproducer"),
                new Claim(ClaimTypes.Role, "CoProducer"),
                new Claim("CoProducerId", "no-subscription-coproducer")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = _controller.GetSubscriptionsByCoProducer("no-subscription-coproducer");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No subscriptions found for this co-producer.", notFoundResult.Value);
        }
    }
}
