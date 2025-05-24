using AMAP.Business.Components;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using AMAP.UnitTests.Repository.Mock;
using NSubstitute;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Business
{
    public class ProductOfferBcTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ProductOfferBc _productOfferBc;

        public ProductOfferBcTests()
        {
            // Arrange - Setup mock repository
            _unitOfWork = MockProductOfferRepository.GetMockUnitOfWork();
            _productOfferBc = new ProductOfferBc(_unitOfWork);
        }

        [Fact]
        public void GetProductOfferById_ValidId_ReturnsProductOffer()
        {
            // Arrange
            var productOfferId = 1;

            // Act
            var result = _productOfferBc.GetProductOfferById(productOfferId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productOfferId, result.Id);
            Assert.Equal(1, result.ProductId);
            Assert.Equal(1, result.SubscriptionPeriodId);
            Assert.Equal(2.99, result.Price);
            Assert.Equal(100, result.Quantity);
            Assert.Equal(PaymentType.Money | PaymentType.MBway, result.PaymentType);
        }

        [Fact]
        public void GetProductOfferById_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var invalidId = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _productOfferBc.GetProductOfferById(invalidId));
            Assert.Equal("Invalid product offer ID. (Parameter 'id')", exception.Message);
        }

        [Fact]
        public void GetProductOfferById_NonExistingId_ReturnsNull()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = _productOfferBc.GetProductOfferById(nonExistingId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetProductOfferByProducer_ValidId_ReturnsProductOffer()
        {
            // Arrange
            var producerId = "producer-1";

            // Act
            var result = _productOfferBc.GetProductOfferByProducer(producerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("producer-1", result.Product.ProducerId);
        }

        [Fact]
        public void GetProductOfferByProducer_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            string emptyProducerId = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _productOfferBc.GetProductOfferByProducer(emptyProducerId));
            Assert.Equal("Invalid producer ID. (Parameter 'producerId')", exception.Message);
        }

        [Fact]
        public void GetProductOfferByProducer_NonExistingId_ReturnsNull()
        {
            // Arrange
            var nonExistingProducerId = "non-existing-producer";

            // Act
            var result = _productOfferBc.GetProductOfferByProducer(nonExistingProducerId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void CreateProductOffer_ValidOffer_ReturnsCreatedOffer()
        {
            // Arrange
            // Get the actual delivery date from the mock repository instead of using DateTime.Now.AddDays(7)
            var deliveryDates = _unitOfWork.DeliveryDate.GetAll(d => d.SubscriptionPeriodId == 1).ToList();
            var validDeliveryDate = deliveryDates.First().Date; // Use an actual valid date

            var newProductOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 3.99,
                Quantity = 50,
                DeliveryDate = validDeliveryDate, // Use the validated date
                PaymentType = PaymentType.Money
            };
            var producerId = "producer-1";

            // Act
            var result = _productOfferBc.CreateProductOffer(newProductOffer, producerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Id); // Should be assigned a new ID (count of existing + 1)
            Assert.Equal(1, result.ProductId);
            Assert.Equal(1, result.SubscriptionPeriodId);
            Assert.Equal(3.99, result.Price);
            Assert.Equal(50, result.Quantity);
            Assert.Equal(PaymentType.Money, result.PaymentType);

            // Verify repository methods were called
            _unitOfWork.ProductOffer.Received(1).Add(Arg.Is<ProductOffer>(po =>
                po.ProductId == 1 &&
                po.SubscriptionPeriodId == 1 &&
                po.Price == 3.99 &&
                po.Quantity == 50));
            _unitOfWork.Received(1).Save();
        }


        [Fact]
        public void CreateProductOffer_NullOffer_ThrowsArgumentException()
        {
            // Arrange
            ProductOffer nullOffer = null;
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _productOfferBc.CreateProductOffer(nullOffer, producerId));
            Assert.Equal("Invalid data.", exception.Message);
        }

        [Fact]
        public void CreateProductOffer_ProductNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 999, // Non-existing product
                SubscriptionPeriodId = 1,
                Price = 3.99,
                Quantity = 50,
                DeliveryDate = DateTime.Now.AddDays(7),
                PaymentType = PaymentType.Money
            };
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _productOfferBc.CreateProductOffer(productOffer, producerId));
            Assert.Equal("Product not found.", exception.Message);
        }

        [Fact]
        public void CreateProductOffer_EmptyProducerId_ThrowsInvalidOperationException()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 3.99,
                Quantity = 50,
                DeliveryDate = DateTime.Now.AddDays(7),
                PaymentType = PaymentType.Money
            };
            string emptyProducerId = "";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _productOfferBc.CreateProductOffer(productOffer, emptyProducerId));
            Assert.Equal("You cannot create an offer for a product from another producer.", exception.Message);
        }

        [Fact]
        public void CreateProductOffer_ProductFromAnotherProducer_ThrowsInvalidOperationException()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 2, // Product from producer-2
                SubscriptionPeriodId = 1,
                Price = 3.99,
                Quantity = 50,
                DeliveryDate = DateTime.Now.AddDays(7),
                PaymentType = PaymentType.Money
            };
            var producerId = "producer-1"; // Different producer

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _productOfferBc.CreateProductOffer(productOffer, producerId));
            Assert.Equal("You cannot create an offer for a product from another producer.", exception.Message);
        }

        [Fact]
        public void CreateProductOffer_SubscriptionPeriodNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 999, // Non-existing subscription period
                Price = 3.99,
                Quantity = 50,
                DeliveryDate = DateTime.Now.AddDays(7),
                PaymentType = PaymentType.Money
            };
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _productOfferBc.CreateProductOffer(productOffer, producerId));
            Assert.Equal("Subscription period not found.", exception.Message);
        }

        [Fact]
        public void CreateProductOffer_PastDeliveryDate_ThrowsInvalidOperationException()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 3.99,
                Quantity = 50,
                DeliveryDate = DateTime.Now.AddDays(-1), // Past date
                PaymentType = PaymentType.Money
            };
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _productOfferBc.CreateProductOffer(productOffer, producerId));
            Assert.Equal("Delivery date cannot be in the past.", exception.Message);
        }

        [Fact]
        public void CreateProductOffer_DuplicateOffer_ThrowsInvalidOperationException()
        {
            // Arrange
            // Get the actual delivery date from the mock repository
            var deliveryDates = _unitOfWork.DeliveryDate.GetAll(d => d.SubscriptionPeriodId == 1).ToList();
            var validDeliveryDate = deliveryDates.First().Date; // Use an actual valid date

            // Get an existing product offer to create a duplicate
            var existingOffer = _unitOfWork.ProductOffer.Get(po => po.ProductId == 1);

            // Create an offer with same product and delivery date as an existing one
            var duplicateProductOffer = new ProductOffer
            {
                ProductId = existingOffer.ProductId,
                SubscriptionPeriodId = existingOffer.SubscriptionPeriodId,
                Price = 3.99,
                Quantity = 50,
                DeliveryDate = existingOffer.DeliveryDate, // Use same delivery date as existing offer
                PaymentType = PaymentType.Money
            };
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _productOfferBc.CreateProductOffer(duplicateProductOffer, producerId));
            Assert.Equal("A product offer already exists for this product on the selected delivery date.", exception.Message);
        }


        [Fact]
        public void CreateProductOffer_DeliveryDateOutsideSubscriptionPeriod_ThrowsInvalidOperationException()
        {
            // Arrange
            var productOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 3.99,
                Quantity = 50,
                DeliveryDate = DateTime.Now.AddDays(100), // Date not in delivery dates
                PaymentType = PaymentType.Money
            };
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _productOfferBc.CreateProductOffer(productOffer, producerId));
            Assert.Equal("Delivery date outside subscription period.", exception.Message);
        }

        [Fact]
        public void CreateProductOffer_InvalidQuantity_ThrowsArgumentException()
        {
            // Arrange
            // Get valid delivery date from the mock repository
            var deliveryDates = _unitOfWork.DeliveryDate.GetAll(d => d.SubscriptionPeriodId == 1).ToList();
            var validDeliveryDate = deliveryDates.First().Date;

            var productOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 3.99,
                Quantity = 0, // Invalid quantity
                DeliveryDate = validDeliveryDate, // Use valid date
                PaymentType = PaymentType.Money
            };
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _productOfferBc.CreateProductOffer(productOffer, producerId));
            Assert.Equal("Product offer quantity must be greater than zero.", exception.Message);
        }

        [Fact]
        public void CreateProductOffer_InvalidPaymentType_ThrowsArgumentException()
        {
            // Arrange
            // Get valid delivery date from the mock repository
            var deliveryDates = _unitOfWork.DeliveryDate.GetAll(d => d.SubscriptionPeriodId == 1).ToList();
            var validDeliveryDate = deliveryDates.First().Date;

            var productOffer = new ProductOffer
            {
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 3.99,
                Quantity = 50,
                DeliveryDate = validDeliveryDate, // Use valid date
                PaymentType = 0 // Invalid payment type
            };
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _productOfferBc.CreateProductOffer(productOffer, producerId));
            Assert.Equal("Invalid payment type.", exception.Message);
        }

        [Fact]
        public void DeleteProductOffer_ValidId_DeletesOffer()
        {
            // Arrange
            var productOfferId = 3; 

            // Act
            _productOfferBc.DeleteProductOffer(productOfferId);

            // Assert
            _unitOfWork.ProductOffer.Received(1).Remove(Arg.Is<ProductOffer>(po => po.Id == productOfferId));
            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public void DeleteProductOffer_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var invalidId = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _productOfferBc.DeleteProductOffer(invalidId));
            Assert.Equal("Invalid product offer ID. (Parameter 'id')", exception.Message);
        }

        [Fact]
        public void DeleteProductOffer_NonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistingId = 999;

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _productOfferBc.DeleteProductOffer(nonExistingId));
            Assert.Equal("Product offer not found.", exception.Message);
        }

        [Fact]
        public void DeleteProductOffer_WithActiveSubscriptions_ThrowsInvalidOperationException()
        {
            // Arrange
            var productOfferId = 1; // Use an existing product offer ID

            _unitOfWork.Subscription.GetAll(Arg.Any<Expression<Func<Subscription, bool>>>(), Arg.Any<string>())
                .Returns(new List<Subscription> { new Subscription { ProductOfferId = productOfferId } });

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _productOfferBc.DeleteProductOffer(productOfferId));
            Assert.Equal("Cannot delete product offer because it has associated subscriptions.", exception.Message);
        }

    }
}
