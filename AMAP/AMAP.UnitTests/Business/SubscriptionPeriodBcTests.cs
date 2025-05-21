using AMAP.Business.Components;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.UnitTests.Repository.Mock;
using NSubstitute;

namespace AMAP.UnitTests.Business
{
    public class SubscriptionPeriodBcTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TestEmailSender _emailSender;
        private readonly SubscriptionPeriodBc _subscriptionPeriodBc;

        public SubscriptionPeriodBcTests()
        {
            // Arrange - Setup mock repository
            _unitOfWork = MockSubscriptionPeriodRepository.GetMockUnitOfWork();
            _emailSender = (TestEmailSender)MockSubscriptionPeriodRepository.GetMockEmailSender();
            _subscriptionPeriodBc = new SubscriptionPeriodBc(_unitOfWork, _emailSender);
        }

        [Fact]
        public void GetSubscriptionPeriodById_ValidId_ReturnsSubscriptionPeriod()
        {
            // Arrange
            var periodId = 1;

            // Act
            var result = _subscriptionPeriodBc.GetSubscriptionPeriodById(periodId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(periodId, result.Id);
            Assert.Equal("Winter 2023", result.Duration);
            Assert.Equal(new DateTime(2023, 12, 1), result.StartDate);
            Assert.Equal(new DateTime(2024, 2, 28), result.EndDate);
            Assert.NotNull(result.DeliveryDates);
            Assert.Equal(3, result.DeliveryDates.Count());
        }

        [Fact]
        public void GetSubscriptionPeriodById_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var invalidId = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _subscriptionPeriodBc.GetSubscriptionPeriodById(invalidId));
            Assert.Equal("Invalid subscription period id. (Parameter 'id')", exception.Message);
        }

        [Fact]
        public void GetSubscriptionPeriodById_NonExistingId_ReturnsNull()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = _subscriptionPeriodBc.GetSubscriptionPeriodById(nonExistingId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetDeliveryDateById_ValidId_ReturnsDeliveryDate()
        {
            // Arrange
            var deliveryDateId = 1;

            // Act
            var result = _subscriptionPeriodBc.GetDeliveryDateById(deliveryDateId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(deliveryDateId, result.Id);
            Assert.Equal(new DateTime(2023, 12, 15), result.Date);
            Assert.Equal(1, result.SubscriptionPeriodId);
        }

        [Fact]
        public void GetDeliveryDateById_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var invalidId = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _subscriptionPeriodBc.GetDeliveryDateById(invalidId));
            Assert.Equal("Invalid delivery date id. (Parameter 'id')", exception.Message);
        }

        [Fact]
        public void GetDeliveryDateById_NonExistingId_ReturnsNull()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = _subscriptionPeriodBc.GetDeliveryDateById(nonExistingId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateSubscriptionPeriodAndDeliveryDateAsync_ValidData_ReturnsCreatedPeriod()
        {
            // Arrange
            var newPeriod = new SubscriptionPeriod
            {
                Duration = "Summer 2024",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 8, 31),
                DeliveryDates = new List<DeliveryDate>
                {
                    new DeliveryDate { Date = new DateTime(2024, 6, 15) },
                    new DeliveryDate { Date = new DateTime(2024, 7, 15) },
                    new DeliveryDate { Date = new DateTime(2024, 8, 15) }
                }
            };

            // Act
            var result = await _subscriptionPeriodBc.CreateSubscriptionPeriodAndDeliveryDateAsync(newPeriod);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Id); // Should be assigned a new ID
            Assert.Equal("Summer 2024", result.Duration);
            Assert.Equal(new DateTime(2024, 6, 1), result.StartDate);
            Assert.Equal(new DateTime(2024, 8, 31), result.EndDate);
            Assert.NotNull(result.DeliveryDates);
            Assert.Equal(3, result.DeliveryDates.Count());

            // Verify repository methods were called
            _unitOfWork.SubscriptionPeriod.Received(1).Add(Arg.Is<SubscriptionPeriod>(p =>
                p.Duration == "Summer 2024" &&
                p.StartDate == new DateTime(2024, 6, 1) &&
                p.EndDate == new DateTime(2024, 8, 31)));
            _unitOfWork.Received(1).Save();

            // Verify email notifications
            Assert.Equal(2, _emailSender.SentEmails.Count); // Two producers
            var emailToProducer1 = _emailSender.SentEmails.FirstOrDefault(e => e.Email == "producer1@example.com");
            var emailToProducer2 = _emailSender.SentEmails.FirstOrDefault(e => e.Email == "producer2@example.com");

            Assert.NotNull(emailToProducer1);
            Assert.Equal("New Subscription Period Created", emailToProducer1.Subject);
            Assert.Contains("Summer 2024", emailToProducer1.Body);

            Assert.NotNull(emailToProducer2);
            Assert.Equal("New Subscription Period Created", emailToProducer2.Subject);
        }

        [Fact]
        public async Task CreateSubscriptionPeriodAndDeliveryDateAsync_NullPeriod_ThrowsArgumentException()
        {
            // Arrange
            SubscriptionPeriod nullPeriod = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _subscriptionPeriodBc.CreateSubscriptionPeriodAndDeliveryDateAsync(nullPeriod));
            Assert.Equal("Invalid subscription period data.", exception.Message);
        }

        [Fact]
        public async Task CreateSubscriptionPeriodAndDeliveryDateAsync_InvalidDates_ThrowsArgumentException()
        {
            // Arrange
            var invalidPeriod = new SubscriptionPeriod
            {
                Duration = "Invalid Period",
                StartDate = new DateTime(2024, 8, 1),
                EndDate = new DateTime(2024, 6, 30), // End date before start date
                DeliveryDates = new List<DeliveryDate>
                {
                    new DeliveryDate { Date = new DateTime(2024, 7, 15) }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _subscriptionPeriodBc.CreateSubscriptionPeriodAndDeliveryDateAsync(invalidPeriod));
            Assert.Equal("Invalid subscription period dates.", exception.Message);
        }

        [Fact]
        public async Task CreateSubscriptionPeriodAndDeliveryDateAsync_DuplicatePeriod_ThrowsInvalidOperationException()
        {
            // Arrange
            var duplicatePeriod = new SubscriptionPeriod
            {
                Duration = "Duplicate Period",
                StartDate = new DateTime(2023, 12, 1), // Same as existing period
                EndDate = new DateTime(2024, 2, 28),   // Same as existing period
                DeliveryDates = new List<DeliveryDate>
                {
                    new DeliveryDate { Date = new DateTime(2024, 1, 15) }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _subscriptionPeriodBc.CreateSubscriptionPeriodAndDeliveryDateAsync(duplicatePeriod));
            Assert.Equal("A subscription period with the same start and end date already exists.", exception.Message);
        }

        [Fact]
        public async Task CreateSubscriptionPeriodAndDeliveryDateAsync_NoDeliveryDates_ThrowsArgumentException()
        {
            // Arrange
            var periodWithoutDates = new SubscriptionPeriod
            {
                Duration = "No Delivery Dates",
                StartDate = new DateTime(2024, 9, 1),
                EndDate = new DateTime(2024, 11, 30),
                DeliveryDates = new List<DeliveryDate>() // Empty list
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _subscriptionPeriodBc.CreateSubscriptionPeriodAndDeliveryDateAsync(periodWithoutDates));
            Assert.Equal("Delivery dates are required.", exception.Message);
        }

        [Fact]
        public async Task CreateSubscriptionPeriodAndDeliveryDateAsync_DuplicateDeliveryDates_ThrowsArgumentException()
        {
            // Arrange
            var periodWithDuplicateDates = new SubscriptionPeriod
            {
                Duration = "Duplicate Dates",
                StartDate = new DateTime(2024, 9, 1),
                EndDate = new DateTime(2024, 11, 30),
                DeliveryDates = new List<DeliveryDate>
                {
                    new DeliveryDate { Date = new DateTime(2024, 10, 15) },
                    new DeliveryDate { Date = new DateTime(2024, 10, 15) } // Duplicate date
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _subscriptionPeriodBc.CreateSubscriptionPeriodAndDeliveryDateAsync(periodWithDuplicateDates));
            Assert.Contains("Duplicate delivery dates found:", exception.Message);
        }

        [Fact]
        public async Task CreateSubscriptionPeriodAndDeliveryDateAsync_DeliveryDateOutsidePeriod_ThrowsArgumentException()
        {
            // Arrange
            var periodWithOutsideDate = new SubscriptionPeriod
            {
                Duration = "Outside Dates",
                StartDate = new DateTime(2024, 9, 1),
                EndDate = new DateTime(2024, 11, 30),
                DeliveryDates = new List<DeliveryDate>
                {
                    new DeliveryDate { Date = new DateTime(2024, 10, 15) },
                    new DeliveryDate { Date = new DateTime(2024, 12, 15) } // Outside period end date
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _subscriptionPeriodBc.CreateSubscriptionPeriodAndDeliveryDateAsync(periodWithOutsideDate));
            Assert.Contains("Delivery date", exception.Message);
            Assert.Contains("is not within the subscription period", exception.Message);
        }

        [Fact]
        public async Task CreateSubscriptionPeriodAndDeliveryDateAsync_EmailFailure_StillCreatesPeriod()
        {
            // Arrange
            var newPeriod = new SubscriptionPeriod
            {
                Duration = "Fall 2024",
                StartDate = new DateTime(2024, 9, 1),
                EndDate = new DateTime(2024, 11, 30),
                DeliveryDates = new List<DeliveryDate>
                {
                    new DeliveryDate { Date = new DateTime(2024, 9, 15) },
                    new DeliveryDate { Date = new DateTime(2024, 10, 15) },
                    new DeliveryDate { Date = new DateTime(2024, 11, 15) }
                }
            };

            // Setup email sender to throw exception
            _emailSender.SetThrowException(true);

            // Act - Should not throw even though emails fail
            var result = await _subscriptionPeriodBc.CreateSubscriptionPeriodAndDeliveryDateAsync(newPeriod);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Fall 2024", result.Duration);

            // Verify the subscription period was still created
            _unitOfWork.SubscriptionPeriod.Received(1).Add(Arg.Is<SubscriptionPeriod>(p =>
                p.Duration == "Fall 2024"));
            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public async Task DeleteSubscriptionPeriodAsync_ValidId_RemovesPeriod()
        {
            // Arrange
            var periodId = 1;

            // Act
            await _subscriptionPeriodBc.DeleteSubscriptionPeriodAsync(periodId);

            // Assert
            _unitOfWork.SubscriptionPeriod.Received(1).Remove(Arg.Is<SubscriptionPeriod>(p => p.Id == periodId));
            _unitOfWork.Received(1).Save();

            // Verify email notifications
            Assert.Equal(2, _emailSender.SentEmails.Count); // Two producers
            Assert.Contains(_emailSender.SentEmails, e => e.Email == "producer1@example.com" && e.Subject == "Subscription Period Deleted");
            Assert.Contains(_emailSender.SentEmails, e => e.Email == "producer2@example.com" && e.Subject == "Subscription Period Deleted");
        }

        [Fact]
        public async Task DeleteSubscriptionPeriodAsync_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var invalidId = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _subscriptionPeriodBc.DeleteSubscriptionPeriodAsync(invalidId));
            Assert.Equal("Invalid subscription period id. (Parameter 'id')", exception.Message);
        }

        [Fact]
        public async Task DeleteSubscriptionPeriodAsync_NonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistingId = 999;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _subscriptionPeriodBc.DeleteSubscriptionPeriodAsync(nonExistingId));
            Assert.Equal("This Subscription Period does not exist", exception.Message);
        }

        [Fact]
        public async Task DeleteSubscriptionPeriodAsync_EmailFailure_DoesNotStopDeletion()
        {
            // Arrange
            var periodId = 1;

            // Setup email sender to throw exception
            _emailSender.SetThrowException(true);

            // Act - Should not throw even though emails fail
            await _subscriptionPeriodBc.DeleteSubscriptionPeriodAsync(periodId);

            // Assert - Verify the period was still deleted
            _unitOfWork.SubscriptionPeriod.Received(1).Remove(Arg.Is<SubscriptionPeriod>(p => p.Id == periodId));
            _unitOfWork.Received(1).Save();
        }
    }
}

