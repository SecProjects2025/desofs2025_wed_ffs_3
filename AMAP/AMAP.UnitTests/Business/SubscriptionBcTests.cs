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
using System.Threading.Tasks;
using Xunit;

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
            Assert.Equal(3, result.Id); // Should be assigned a new ID
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
            var coProducerId = "coproducer-1";
            var newSubscription = new Subscription
            {
                ProductOfferId = 3, // Stock = 3
                ExpectedQuantity = 2,
                PaymentType = PaymentType.Money
            };
            // Adiciona subscrições para exceder o stock
            var repo = _unitOfWork.Subscription;
            repo.Add(new Subscription { CoProducerId = coProducerId, ProductOfferId = 3, ExpectedQuantity = 2, PaymentType = PaymentType.Money });
            repo.Add(new Subscription { CoProducerId = coProducerId, ProductOfferId = 3, ExpectedQuantity = 2, PaymentType = PaymentType.Money });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _subscriptionBc.CreateSubscription(newSubscription, coProducerId));
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

        #region AcceptTerms Tests

        [Fact]
        public async Task AcceptTerms_ValidSubscriptionWithConfirmedEmail_ReturnsTrue()
        {
            // Arrange
            int subscriptionId = 1;
            var subscription = new Subscription
            {
                Id = subscriptionId,
                CoProducerId = "coproducer-1",
                ProductOfferId = 1,
                ExpectedQuantity = 5,
                PaymentType = PaymentType.Money,
                EmailConfirmed = true,
                TermsAccepted = false,
                TermsAcceptedAt = default
            };

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.Subscription.Get(Arg.Any<Expression<Func<Subscription, bool>>>())
                .Returns(subscription);

            var subscriptionBc = new SubscriptionBc(unitOfWork, _emailSender, _logger);

            // Act
            var result = await subscriptionBc.AcceptTerms(subscriptionId);

            // Assert
            Assert.True(result);
            Assert.True(subscription.TermsAccepted);
            Assert.NotEqual(default, subscription.TermsAcceptedAt);
            unitOfWork.Received(1).Save();
        }

        [Fact]
        public async Task AcceptTerms_InvalidSubscriptionId_ThrowsArgumentException()
        {
            // Arrange
            int invalidId = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _subscriptionBc.AcceptTerms(invalidId));
            Assert.Equal("Invalid subscription ID.", exception.Message);
        }

        [Fact]
        public async Task AcceptTerms_NegativeSubscriptionId_ThrowsArgumentException()
        {
            // Arrange
            int negativeId = -5;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _subscriptionBc.AcceptTerms(negativeId));
            Assert.Equal("Invalid subscription ID.", exception.Message);
        }

        [Fact]
        public async Task AcceptTerms_NonExistingSubscription_ReturnsFalse()
        {
            // Arrange
            int nonExistingId = 999;

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.Subscription.Get(Arg.Any<Expression<Func<Subscription, bool>>>())
                .Returns((Subscription)null);

            var subscriptionBc = new SubscriptionBc(unitOfWork, _emailSender, _logger);

            // Act
            var result = await subscriptionBc.AcceptTerms(nonExistingId);

            // Assert
            Assert.False(result);
            unitOfWork.DidNotReceive().Save();
        }

        [Fact]
        public async Task AcceptTerms_EmailNotConfirmed_ThrowsInvalidOperationException()
        {
            // Arrange
            int subscriptionId = 1;
            var subscription = new Subscription
            {
                Id = subscriptionId,
                CoProducerId = "coproducer-1",
                ProductOfferId = 1,
                ExpectedQuantity = 5,
                PaymentType = PaymentType.Money,
                EmailConfirmed = false, // Email not confirmed
                TermsAccepted = false
            };

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.Subscription.Get(Arg.Any<Expression<Func<Subscription, bool>>>())
                .Returns(subscription);

            var subscriptionBc = new SubscriptionBc(unitOfWork, _emailSender, _logger);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await subscriptionBc.AcceptTerms(subscriptionId));
            Assert.Equal("Email must be confirmed before accepting terms.", exception.Message);
            unitOfWork.DidNotReceive().Save();
        }

        [Fact]
        public async Task AcceptTerms_TermsAlreadyAccepted_UpdatesTimestampAndReturnsTrue()
        {
            // Arrange
            int subscriptionId = 1;
            var oldTimestamp = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var subscription = new Subscription
            {
                Id = subscriptionId,
                CoProducerId = "coproducer-1",
                ProductOfferId = 1,
                ExpectedQuantity = 5,
                PaymentType = PaymentType.Money,
                EmailConfirmed = true,
                TermsAccepted = true, // Already accepted
                TermsAcceptedAt = oldTimestamp
            };

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.Subscription.Get(Arg.Any<Expression<Func<Subscription, bool>>>())
                .Returns(subscription);

            var subscriptionBc = new SubscriptionBc(unitOfWork, _emailSender, _logger);

            // Act
            var result = await subscriptionBc.AcceptTerms(subscriptionId);

            // Assert
            Assert.True(result);
            Assert.True(subscription.TermsAccepted);
            Assert.NotEqual(oldTimestamp, subscription.TermsAcceptedAt);
            unitOfWork.Received(1).Save();
        }

        [Fact]
        public async Task AcceptTerms_DatabaseError_ProperlyHandled()
        {
            // Arrange
            int subscriptionId = 1;
            var subscription = new Subscription
            {
                Id = subscriptionId,
                CoProducerId = "coproducer-1",
                ProductOfferId = 1,
                ExpectedQuantity = 5,
                PaymentType = PaymentType.Money,
                EmailConfirmed = true,
                TermsAccepted = false
            };

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.Subscription.Get(Arg.Any<Expression<Func<Subscription, bool>>>())
                .Returns(subscription);

            // Simulate database error during save
            unitOfWork.When(u => u.Save())
                .Do(_ => throw new Exception("Database error"));

            var subscriptionBc = new SubscriptionBc(unitOfWork, _emailSender, _logger);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => await subscriptionBc.AcceptTerms(subscriptionId));
        }

        #endregion
    }
}