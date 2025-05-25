using AMAP.Business.Components;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.UnitTests.Repository.Mock;
using Microsoft.Extensions.Logging;
using NSubstitute;
namespace AMAP.UnitTests.Business
{
    public class PaymentBcTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaymentBc _paymentBc;
        private readonly ILogger<PaymentBc> _logger;

        public PaymentBcTests()
        {
            // Arrange - Setup mock repository
            _unitOfWork = MockPaymentRepository.GetMockUnitOfWork();
            _logger = Substitute.For<ILogger<PaymentBc>>();
            _paymentBc = new PaymentBc(_unitOfWork, _logger);
        }

        [Fact]
        public void CreatePayment_ValidPayment_ReturnsCreatedPayment()
        {
            // Arrange
            var newPayment = new Payment
            {
                SubscriptionId = 1, // Subscription belongs to coproducer-1
                AmountPaid = 125.75
            };
            var coProducerId = "coproducer-1"; // Owner of the subscription

            // Act
            var result = _paymentBc.CreatePayment(newPayment, coProducerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Id); // Should be assigned a new ID
            Assert.Equal(1, result.SubscriptionId);
            Assert.Equal(125.75, result.AmountPaid);
            Assert.NotEqual(default(DateTime), result.PaymentDate); // Payment date should be set

            // Verify repository methods were called
            _unitOfWork.Payment.Received(1).Add(Arg.Is<Payment>(p =>
                p.SubscriptionId == 1 &&
                p.AmountPaid == 125.75 &&
                p.PaymentDate != default(DateTime)));
            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public void CreatePayment_NullPayment_ThrowsArgumentNullException()
        {
            // Arrange
            Payment nullPayment = null;
            var coProducerId = "coproducer-1";

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                _paymentBc.CreatePayment(nullPayment, coProducerId));
            Assert.Equal("payment", exception.ParamName);
            Assert.Contains("Invalid payment data", exception.Message);
        }

        [Fact]
        public void CreatePayment_EmptyCoProducerId_ThrowsArgumentException()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 1,
                AmountPaid = 100.00
            };
            var emptyCoProducerId = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _paymentBc.CreatePayment(payment, emptyCoProducerId));
            Assert.Equal("currentCoProducerId", exception.ParamName);
            Assert.Contains("CoProducer ID is required", exception.Message);
        }

        [Fact]
        public void CreatePayment_NullCoProducerId_ThrowsArgumentException()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 1,
                AmountPaid = 100.00
            };
            string nullCoProducerId = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _paymentBc.CreatePayment(payment, nullCoProducerId));
            Assert.Equal("currentCoProducerId", exception.ParamName);
            Assert.Contains("CoProducer ID is required", exception.Message);
        }

        [Fact]
        public void CreatePayment_NonExistingSubscription_ThrowsKeyNotFoundException()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 999, // Non-existing subscription
                AmountPaid = 100.00
            };
            var coProducerId = "coproducer-1";

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() =>
                _paymentBc.CreatePayment(payment, coProducerId));
            Assert.Contains("Subscription with ID 999 not found", exception.Message);
        }

        [Fact]
        public void CreatePayment_UnauthorizedCoProducer_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var payment = new Payment
            {
                SubscriptionId = 3, // Belongs to coproducer-2
                AmountPaid = 100.00
            };
            var wrongCoProducerId = "coproducer-1"; // Not the owner of the subscription

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(() =>
                _paymentBc.CreatePayment(payment, wrongCoProducerId));
            Assert.Equal("You can only make payments for your own subscriptions.", exception.Message);
        }

        [Fact]
        public void CreatePayment_SetsPaymentDateToCurrentTime()
        {
            // Arrange
            var beforeTest = DateTime.UtcNow.AddSeconds(-1);
            var payment = new Payment
            {
                SubscriptionId = 1, // Belongs to coproducer-1
                AmountPaid = 50.00,
                PaymentDate = new DateTime(2000, 1, 1) // This should be overwritten
            };
            var coProducerId = "coproducer-1";

            // Act
            var result = _paymentBc.CreatePayment(payment, coProducerId);
            var afterTest = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.NotEqual(new DateTime(2000, 1, 1), result.PaymentDate);
            Assert.True(result.PaymentDate >= beforeTest);
            Assert.True(result.PaymentDate <= afterTest);
        }
    }
}
