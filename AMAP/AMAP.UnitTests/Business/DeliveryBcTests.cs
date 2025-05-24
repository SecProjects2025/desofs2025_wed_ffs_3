using AMAP.Business.Components;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.UnitTests.Repository.Mock;
using NSubstitute;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Business
{
    public class DeliveryBcTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DeliveryBc _deliveryBc;

        public DeliveryBcTests()
        {
            // Arrange - Setup mock repository
            _unitOfWork = MockDeliveryRepository.GetMockUnitOfWork();
            _deliveryBc = new DeliveryBc(_unitOfWork);
        }

        [Fact]
        public void UpdateDeliveredQuantity_UpdateExisting_SuccessfullyUpdates()
        {
            // Arrange
            var subscriptionId = 1; // Has an existing delivery with ID 1
            var newQuantity = 7;
            var producerId = "producer-1"; // Has permission for this subscription

            // Act
            _deliveryBc.UpdateDeliveredQuantity(subscriptionId, newQuantity, producerId);

            // Assert
            _unitOfWork.Delivery.Received(1).Update(Arg.Is<Delivery>(d =>
                d.SubscriptionId == subscriptionId &&
                d.DeliveredQuantity == newQuantity));
            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public void UpdateDeliveredQuantity_CreateNew_SuccessfullyCreates()
        {
            // Arrange
            var subscriptionId = 3; // Doesn't have a delivery yet
            var quantity = 4;
            var producerId = "producer-1"; // Has permission for this subscription

            // Act
            _deliveryBc.UpdateDeliveredQuantity(subscriptionId, quantity, producerId);

            // Assert
            _unitOfWork.Delivery.Received(1).Add(Arg.Is<Delivery>(d =>
                d.SubscriptionId == subscriptionId &&
                d.DeliveredQuantity == quantity));
            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public void UpdateDeliveredQuantity_NegativeQuantity_ThrowsArgumentException()
        {
            // Arrange
            var subscriptionId = 1;
            var negativeQuantity = -5;
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _deliveryBc.UpdateDeliveredQuantity(subscriptionId, negativeQuantity, producerId));
            Assert.Equal("Delivered quantity cannot be negative.", exception.Message);
        }

        [Fact]
        public void UpdateDeliveredQuantity_NonExistingSubscription_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistingSubscriptionId = 999;
            var quantity = 5;
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() =>
                _deliveryBc.UpdateDeliveredQuantity(nonExistingSubscriptionId, quantity, producerId));
            Assert.Equal("Subscription not found.", exception.Message);
        }

        [Fact]
        public void UpdateDeliveredQuantity_UnauthorizedProducer_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var subscriptionId = 1; // Belongs to producer-1's product
            var quantity = 5;
            var wrongProducerId = "producer-2"; // Different producer

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(() =>
                _deliveryBc.UpdateDeliveredQuantity(subscriptionId, quantity, wrongProducerId));
            Assert.Equal("You are not allowed to update this subscription.", exception.Message);
        }

        [Fact]
        public void UpdateDeliveredQuantity_NullProductOffer_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var subscriptionId = 1;
            var quantity = 5;
            var producerId = "producer-1";

            // Modify the mock to return a subscription with null ProductOffer
            var subscriptionWithNullOffer = new Subscription
            {
                Id = subscriptionId,
                ProductOffer = null
            };

            _unitOfWork.Subscription.Get(
                Arg.Any<Expression<Func<Subscription, bool>>>(),
                Arg.Any<string>(),
                Arg.Any<bool>())
                .Returns(subscriptionWithNullOffer);

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(() =>
                _deliveryBc.UpdateDeliveredQuantity(subscriptionId, quantity, producerId));
            Assert.Equal("You are not allowed to update this subscription.", exception.Message);
        }

        [Fact]
        public void UpdateDeliveredQuantity_ZeroQuantity_SuccessfullyUpdates()
        {
            // Arrange
            var subscriptionId = 1;
            var zeroQuantity = 0; // Valid value - might mean "no delivery"
            var producerId = "producer-1";

            // Act
            _deliveryBc.UpdateDeliveredQuantity(subscriptionId, zeroQuantity, producerId);

            // Assert
            _unitOfWork.Delivery.Received(1).Update(Arg.Is<Delivery>(d =>
                d.SubscriptionId == subscriptionId &&
                d.DeliveredQuantity == zeroQuantity));
            _unitOfWork.Received(1).Save();
        }
    }
}

