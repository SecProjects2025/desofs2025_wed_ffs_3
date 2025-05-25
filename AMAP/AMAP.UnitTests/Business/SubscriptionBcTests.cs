using AMAP.Business.Components;
using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using AMAP.UnitTests.Business.Mock;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Business
{
    public class SubscriptionBcTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailSender _emailSender;
        private readonly ILogger<SubscriptionBc> _logger;
        private readonly SubscriptionBc _subscriptionBc;

        public SubscriptionBcTests()
        {
            _unitOfWork = MockUnitOfWork.GetMockUnitOfWork();

            // Mock IConfiguration for EmailSender
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x["EmailSettings:From"]).Returns("test@example.com");
            configuration.Setup(x => x["EmailSettings:Password"]).Returns("test-password");
            configuration.Setup(x => x["EmailSettings:Host"]).Returns("smtp.example.com");
            configuration.Setup(x => x["EmailSettings:Port"]).Returns("587");

            _emailSender = new EmailSender(configuration.Object);
            _logger = Substitute.For<ILogger<SubscriptionBc>>();
            _subscriptionBc = new SubscriptionBc(_unitOfWork, _emailSender, _logger);
        }

        [Fact]
        public void GetSubscriptionById_ValidId_ReturnsSubscription()
        {
            // Arrange
            var subscriptionId = 1;

            // Act
            var result = _subscriptionBc.GetSubscriptionById(subscriptionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(subscriptionId, result.Id);
            Assert.Equal("coproducer-1", result.CoProducerId);
            Assert.Equal(1, result.ProductOfferId);
            Assert.Equal(5, result.ExpectedQuantity);
            Assert.Equal(PaymentType.Money, result.PaymentType);
        }

        [Fact]
        public void GetSubscriptionById_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var invalidId = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _subscriptionBc.GetSubscriptionById(invalidId));
            Assert.Equal("Invalid subscription ID (Parameter 'id')", exception.Message);
        }

        [Fact]
        public void GetSubscriptionById_NonExistingId_ReturnsNull()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = _subscriptionBc.GetSubscriptionById(nonExistingId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSubscriptionsByCoProducer_ValidId_ReturnsSubscriptions()
        {
            // Arrange
            var coProducerId = "coproducer-1";

            // Act
            var result = _subscriptionBc.GetSubscriptionsByCoProducer(coProducerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, subscription => Assert.Equal(coProducerId, subscription.CoProducerId));
        }

        [Fact]
        public void GetSubscriptionsByCoProducer_EmptyId_ThrowsArgumentException()
        {
            // Arrange
            var emptyProducerId = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _subscriptionBc.GetSubscriptionsByCoProducer(emptyProducerId));
            Assert.Equal("CoProducer ID cannot be empty. (Parameter 'coProducerId')", exception.Message);
        }

        [Fact]
        public void GetSubscriptionsByCoProducer_NonExistingId_ReturnsEmptyList()
        {
            // Arrange
            var nonExistingProducerId = "non-existing-producer";

            // Act
            var result = _subscriptionBc.GetSubscriptionsByCoProducer(nonExistingProducerId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void CreateSubscription_ValidSubscription_ReturnsCreatedSubscription()
        {
            // Arrange
            var newSubscription = new Subscription
            {
                ProductOfferId = 3, // Choose an offer with available stock
                ExpectedQuantity = 2,
                PaymentType = PaymentType.Money
            };
            var coProducerId = "coproducer-1";

            // Act
            var result = _subscriptionBc.CreateSubscription(newSubscription, coProducerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Id); // Should be assigned a new ID
            Assert.Equal(coProducerId, result.CoProducerId);
            Assert.Equal(3, result.ProductOfferId);
            Assert.Equal(2, result.ExpectedQuantity);
            Assert.Equal(PaymentType.Money, result.PaymentType);

            // Verify repository methods were called
            _unitOfWork.Subscription.Received(1).Add(Arg.Is<Subscription>(s =>
                s.CoProducerId == coProducerId &&
                s.ProductOfferId == 3 &&
                s.ExpectedQuantity == 2));
            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public void CreateSubscription_NullSubscription_ThrowsArgumentException()
        {
            // Arrange
            Subscription nullSubscription = null;
            var coProducerId = "coproducer-1";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _subscriptionBc.CreateSubscription(nullSubscription, coProducerId));
            Assert.Equal("Invalid data.", exception.Message);
        }

        [Fact]
        public void CreateSubscription_NullCoProducerId_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var subscription = new Subscription
            {
                ProductOfferId = 1,
                ExpectedQuantity = 2,
                PaymentType = PaymentType.Money
            };
            string nullCoProducerId = null;

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(() =>
                _subscriptionBc.CreateSubscription(subscription, nullCoProducerId));
            Assert.Equal("Invalid CoProducer.", exception.Message);
        }

        [Fact]
        public void CreateSubscription_NonExistingProductOffer_ThrowsKeyNotFoundException()
        {
            // Arrange
            var subscription = new Subscription
            {
                ProductOfferId = 999, // Non-existing product offer
                ExpectedQuantity = 2,
                PaymentType = PaymentType.Money
            };
            var coProducerId = "coproducer-1";

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() =>
                _subscriptionBc.CreateSubscription(subscription, coProducerId));
            Assert.Equal("Product offer not found.", exception.Message);
        }

        [Fact]
        public void CreateSubscription_ZeroQuantity_ThrowsArgumentException()
        {
            // Arrange
            var subscription = new Subscription
            {
                ProductOfferId = 1,
                ExpectedQuantity = 0, // Invalid quantity
                PaymentType = PaymentType.Money
            };
            var coProducerId = "coproducer-1";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _subscriptionBc.CreateSubscription(subscription, coProducerId));
            Assert.Equal("Quantity must be greater than zero.", exception.Message);
        }

        [Fact]
        public void CreateSubscription_NegativeQuantity_ThrowsArgumentException()
        {
            // Arrange
            var subscription = new Subscription
            {
                ProductOfferId = 1,
                ExpectedQuantity = -5, // Invalid quantity
                PaymentType = PaymentType.Money
            };
            var coProducerId = "coproducer-1";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _subscriptionBc.CreateSubscription(subscription, coProducerId));
            Assert.Equal("Quantity must be greater than zero.", exception.Message);
        }

        [Fact]
        public void CreateSubscription_ExceedingAvailableStock_ThrowsInvalidOperationException()
        {
            // Arrange
            // Product offer ID 1 has 20 quantity, but 7 already ordered (5+2 from existing subscriptions)
            var subscription = new Subscription
            {
                ProductOfferId = 1,
                ExpectedQuantity = 14, // This would make total 21, exceeding the 20 available
                PaymentType = PaymentType.Money
            };
            var coProducerId = "coproducer-1";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _subscriptionBc.CreateSubscription(subscription, coProducerId));
            Assert.Equal("Requested quantity exceeds available stock.", exception.Message);
        }

        [Fact]
        public void CreateSubscription_InvalidPaymentType_ThrowsArgumentException()
        {
            // Arrange
            var subscription = new Subscription
            {
                ProductOfferId = 1,
                ExpectedQuantity = 2,
                PaymentType = 0 // Invalid payment type (None)
            };
            var coProducerId = "coproducer-1";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _subscriptionBc.CreateSubscription(subscription, coProducerId));
            Assert.Equal("Invalid payment type.", exception.Message);
        }

        [Fact]
        public void CreateSubscription_UnsupportedPaymentType_ThrowsInvalidOperationException()
        {
            // Arrange
            // Product offer ID 1 supports Money and MBway, but not BankTransfer
            var subscription = new Subscription
            {
                ProductOfferId = 1,
                ExpectedQuantity = 2,
                PaymentType = PaymentType.BankTransfer // Not supported by offer
            };
            var coProducerId = "coproducer-1";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _subscriptionBc.CreateSubscription(subscription, coProducerId));
            Assert.Equal("Selected payment type is not supported by the product offer.", exception.Message);
        }
    }
}
