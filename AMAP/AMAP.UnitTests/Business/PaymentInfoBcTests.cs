using AMAP.Business.Components;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.UnitTests.Repository.Mock;
using NSubstitute;
namespace AMAP.UnitTests.Business
{
    public class PaymentsInfoBcTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaymentsInfoBc _paymentsInfoBc;

        public PaymentsInfoBcTests()
        {
            // Arrange - Setup mock repository
            _unitOfWork = MockPaymentsInfoRepository.GetMockUnitOfWork();
            _paymentsInfoBc = new PaymentsInfoBc(_unitOfWork);
        }

        [Fact]
        public void GetProducerPayments_ReturnsValidData()
        {
            // Arrange is done in constructor

            // Act
            var result = _paymentsInfoBc.GetProducerPayments();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Two producers with subscriptions

            // Check producer-1 data
            Assert.True(result.ContainsKey("producer-1"));
            var producer1Data = result["producer-1"];
            Assert.Equal("Producer One", producer1Data.Item1.Name);
            Assert.Equal(3, producer1Data.Item2.Count); // 3 subscriptions for producer-1's products

            // Check specific subscription for producer-1
            Assert.True(producer1Data.Item2.ContainsKey(1)); // Subscription ID 1
            var subscription1 = producer1Data.Item2[1];
            Assert.Equal(1, subscription1.Item1.Id); // Subscription ID
            Assert.Equal("coproducer-1", subscription1.Item1.CoProducerId); // CoProducer ID
            Assert.Equal(5, subscription1.Item1.ExpectedQuantity); // Expected quantity

            // Check payments for subscription 1
            var paymentsForSub1 = subscription1.Item2;
            Assert.Single(paymentsForSub1); // One payment for subscription 1
            Assert.Equal(7.5, paymentsForSub1.First().AmountPaid);

            // Check producer-2 data
            Assert.True(result.ContainsKey("producer-2"));
            var producer2Data = result["producer-2"];
            Assert.Equal("Producer Two", producer2Data.Item1.Name);
            Assert.Single(producer2Data.Item2); // 1 subscription for producer-2's products
        }

        [Fact]
        public void GetCoProducerPayments_ReturnsValidData()
        {
            // Arrange is done in constructor

            // Act
            var result = _paymentsInfoBc.GetCoProducerPayments();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Two coproducers with subscriptions

            // Check coproducer-1 data
            Assert.True(result.ContainsKey("coproducer-1"));
            var coproducer1Data = result["coproducer-1"];
            Assert.Equal("CoProducer One", coproducer1Data.Item1.Name);
            Assert.Equal(2, coproducer1Data.Item2.Count); // 2 subscriptions for coproducer-1

            // Check specific subscription for coproducer-1
            Assert.True(coproducer1Data.Item2.ContainsKey(1)); // Subscription ID 1
            var subscription1 = coproducer1Data.Item2[1];
            Assert.Equal(1, subscription1.Item1.Id); // Subscription ID
            Assert.Equal(1, subscription1.Item1.ProductOfferId); // ProductOffer ID
            Assert.Equal(5, subscription1.Item1.ExpectedQuantity); // Expected quantity

            // Check payments for subscription 1
            var paymentsForSub1 = subscription1.Item2;
            Assert.Single(paymentsForSub1); // One payment for subscription 1
            Assert.Equal(7.5, paymentsForSub1.First().AmountPaid);

            // Check coproducer-2 data
            Assert.True(result.ContainsKey("coproducer-2"));
            var coproducer2Data = result["coproducer-2"];
            Assert.Equal("CoProducer Two", coproducer2Data.Item1.Name);
            Assert.Equal(2, coproducer2Data.Item2.Count); // 2 subscriptions for coproducer-2
        }

        [Fact]
        public void GetProducerTotals_ReturnsCorrectTotals()
        {
            // Arrange is done in constructor

            // Act
            var result = _paymentsInfoBc.GetProducerTotals();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Two producers with subscriptions

            // Check producer-1 totals
            Assert.True(result.ContainsKey("producer-1"));
            var producer1Totals = result["producer-1"];

            // Expected total for producer-1:
            // Subscription 1: 3 (delivered) * 2.5 (price) = 7.5
            // Subscription 2: 2 (delivered) * 2.5 (price) = 5.0
            // Subscription 3: 4 (expected) * 1.5 (price) = 6.0
            // Total: 18.5
            Assert.Equal(18.5, producer1Totals.Item1); // Subscription total

            // Payment total for producer-1:
            // Subscription 1: 7.5
            // Subscription 2: 3.75
            // Subscription 3: 4.0
            // Total: 15.25
            Assert.Equal(15.25, producer1Totals.Item2); // Payment total

            // Check producer-2 totals
            Assert.True(result.ContainsKey("producer-2"));
            var producer2Totals = result["producer-2"];

            // Expected total for producer-2:
            // Subscription 4: 2 (expected) * 3.0 (price) = 6.0
            // Total: 6.0
            Assert.Equal(6.0, producer2Totals.Item1); // Subscription total

            // Payment total for producer-2: 0 (no payments for subscription 4)
            Assert.Equal(0.0, producer2Totals.Item2); // Payment total
        }

        [Fact]
        public void GetCoProducerTotals_ReturnsCorrectTotals()
        {
            // Arrange is done in constructor

            // Act
            var result = _paymentsInfoBc.GetCoProducerTotals();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Two coproducers with subscriptions

            // Check coproducer-1 totals
            Assert.True(result.ContainsKey("coproducer-1"));
            var coproducer1Totals = result["coproducer-1"];

            // Expected total for coproducer-1:
            // Subscription 1: 3 (delivered) * 2.5 (price) = 7.5
            // Subscription 3: 4 (expected) * 1.5 (price) = 6.0
            // Total: 13.5
            Assert.Equal(13.5, coproducer1Totals.Item1); // Subscription total

            // Payment total for coproducer-1:
            // Subscription 1: 7.5
            // Subscription 3: 4.0
            // Total: 11.5
            Assert.Equal(11.5, coproducer1Totals.Item2); // Payment total

            // Check coproducer-2 totals
            Assert.True(result.ContainsKey("coproducer-2"));
            var coproducer2Totals = result["coproducer-2"];

            // Expected total for coproducer-2:
            // Subscription 2: 2 (delivered) * 2.5 (price) = 5.0
            // Subscription 4: 2 (expected) * 3.0 (price) = 6.0
            // Total: 11.0
            Assert.Equal(11.0, coproducer2Totals.Item1); // Subscription total

            // Payment total for coproducer-2:
            // Subscription 2: 3.75
            // Subscription 4: 0
            // Total: 3.75
            Assert.Equal(3.75, coproducer2Totals.Item2); // Payment total
        }

        [Fact]
        public void GetProducerToReceive_ReturnsCorrectAmounts()
        {
            // Arrange is done in constructor

            // Act
            var result = _paymentsInfoBc.GetProducerToReceive();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Two producers with subscriptions

            // Check producer-1 amount to receive
            Assert.True(result.ContainsKey("producer-1"));
            // Subscription total: 18.5, Payment total: 15.25, To receive: 3.25
            Assert.Equal(3.25, result["producer-1"]);

            // Check producer-2 amount to receive
            Assert.True(result.ContainsKey("producer-2"));
            // Subscription total: 6.0, Payment total: 0.0, To receive: 6.0
            Assert.Equal(6.0, result["producer-2"]);
        }

        [Fact]
        public void GetCoProducerToPay_ReturnsCorrectAmounts()
        {
            // Arrange is done in constructor

            // Act
            var result = _paymentsInfoBc.GetCoProducerToPay();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Two coproducers with subscriptions

            // Check coproducer-1 amount to pay
            Assert.True(result.ContainsKey("coproducer-1"));
            // Subscription total: 13.5, Payment total: 11.5, To pay: 2.0
            Assert.Equal(2.0, result["coproducer-1"]);

            // Check coproducer-2 amount to pay
            Assert.True(result.ContainsKey("coproducer-2"));
            // Subscription total: 11.0, Payment total: 3.75, To pay: 7.25
            Assert.Equal(7.25, result["coproducer-2"]);
        }

        [Fact]
        public void GetProducerCC_ReturnsValidData()
        {
            // Arrange is done in constructor

            // Act
            var result = _paymentsInfoBc.GetProducerCC();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Two producers with subscriptions

            // Check producer-1 data
            Assert.True(result.ContainsKey("producer-1"));
            var producer1Data = result["producer-1"];
            Assert.Equal("Producer One", producer1Data.Item1.Name);
            Assert.Equal(3, producer1Data.Item2.Count()); // 3 subscriptions for producer-1's products
            Assert.Equal(3, producer1Data.Item3.Count()); // 3 payments for producer-1's subscriptions

            // Check producer-2 data
            Assert.True(result.ContainsKey("producer-2"));
            var producer2Data = result["producer-2"];
            Assert.Equal("Producer Two", producer2Data.Item1.Name);
            Assert.Single(producer2Data.Item2); // 1 subscription for producer-2's products
            Assert.Empty(producer2Data.Item3); // No payments for producer-2's subscriptions
        }

        [Fact]
        public void GetCoProducerCC_ReturnsValidData()
        {
            // Arrange is done in constructor

            // Act
            var result = _paymentsInfoBc.GetCoProducerCC();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Two coproducers with subscriptions

            // Check coproducer-1 data
            Assert.True(result.ContainsKey("coproducer-1"));
            var coproducer1Data = result["coproducer-1"];
            Assert.Equal("CoProducer One", coproducer1Data.Item1.Name);
            Assert.Equal(2, coproducer1Data.Item2.Count()); // 2 subscriptions for coproducer-1
            Assert.Equal(2, coproducer1Data.Item3.Count()); // 2 payments for coproducer-1's subscriptions

            // Check coproducer-2 data
            Assert.True(result.ContainsKey("coproducer-2"));
            var coproducer2Data = result["coproducer-2"];
            Assert.Equal("CoProducer Two", coproducer2Data.Item1.Name);
            Assert.Equal(2, coproducer2Data.Item2.Count()); // 2 subscriptions for coproducer-2
            Assert.Single(coproducer2Data.Item3); // 1 payment for coproducer-2's subscriptions
        }

        [Fact]
        public void GetProducerPayments_EmptyRepository_ReturnsEmptyDictionary()
        {
            // Arrange
            var emptyUnitOfWork = Substitute.For<IUnitOfWork>();
            var emptyAppUserRepo = Substitute.For<IAppUserRepository>();
            emptyAppUserRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<AppUser, bool>>>(), Arg.Any<string>())
                .Returns(new List<AppUser>());
            emptyUnitOfWork.AppUser.Returns(emptyAppUserRepo);

            var paymentsInfoBc = new PaymentsInfoBc(emptyUnitOfWork);

            // Act
            var result = paymentsInfoBc.GetProducerPayments();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetCoProducerPayments_EmptyRepository_ReturnsEmptyDictionary()
        {
            // Arrange
            var emptyUnitOfWork = Substitute.For<IUnitOfWork>();
            var emptyAppUserRepo = Substitute.For<IAppUserRepository>();
            emptyAppUserRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<AppUser, bool>>>(), Arg.Any<string>())
                .Returns(new List<AppUser>());
            emptyUnitOfWork.AppUser.Returns(emptyAppUserRepo);

            var paymentsInfoBc = new PaymentsInfoBc(emptyUnitOfWork);

            // Act
            var result = paymentsInfoBc.GetCoProducerPayments();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetProducerTotals_ProducerWithNoSubscriptions_ExcludesFromResult()
        {
            // Arrange
            var mockUnitOfWork = Substitute.For<IUnitOfWork>();

            // Set up AppUser repository
            var mockAppUserRepo = Substitute.For<IAppUserRepository>();
            var users = new List<AppUser>
            {
                new AppUser { Id = "producer-3", Name = "Producer With No Subscriptions", UserType = Entities.Utilities.UserType.Producer }
            };
            mockAppUserRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<AppUser, bool>>>(), Arg.Any<string>())
                .Returns(users);
            mockUnitOfWork.AppUser.Returns(mockAppUserRepo);

            // Set up Product repository
            var mockProductRepo = Substitute.For<IProductRepository>();
            mockProductRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Product, bool>>>(), Arg.Any<string>())
                .Returns(new List<Product>());
            mockUnitOfWork.Product.Returns(mockProductRepo);

            // Set up other necessary repositories
            mockUnitOfWork.ProductOffer.Returns(Substitute.For<IProductOfferRepository>());
            mockUnitOfWork.Subscription.Returns(Substitute.For<ISubscriptionRepository>());
            mockUnitOfWork.Payment.Returns(Substitute.For<IPaymentRepository>());
            mockUnitOfWork.Delivery.Returns(Substitute.For<IDeliveryRepository>());

            var paymentsInfoBc = new PaymentsInfoBc(mockUnitOfWork);

            // Act
            var result = paymentsInfoBc.GetProducerTotals();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Producer with no subscriptions should be excluded
        }

        [Fact]
        public void GetCoProducerTotals_CoProducerWithNoSubscriptions_ExcludesFromResult()
        {
            // Arrange
            var mockUnitOfWork = Substitute.For<IUnitOfWork>();

            // Set up AppUser repository
            var mockAppUserRepo = Substitute.For<IAppUserRepository>();
            var users = new List<AppUser>
            {
                new AppUser { Id = "coproducer-3", Name = "CoProducer With No Subscriptions", UserType = Entities.Utilities.UserType.CoProducer }
            };
            mockAppUserRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<AppUser, bool>>>(), Arg.Any<string>())
                .Returns(users);
            mockUnitOfWork.AppUser.Returns(mockAppUserRepo);

            // Set up Subscription repository
            var mockSubscriptionRepo = Substitute.For<ISubscriptionRepository>();
            mockSubscriptionRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Subscription, bool>>>(), Arg.Any<string>())
                .Returns(new List<Subscription>());
            mockUnitOfWork.Subscription.Returns(mockSubscriptionRepo);

            // Set up other necessary repositories
            mockUnitOfWork.Payment.Returns(Substitute.For<IPaymentRepository>());
            mockUnitOfWork.Delivery.Returns(Substitute.For<IDeliveryRepository>());
            mockUnitOfWork.ProductOffer.Returns(Substitute.For<IProductOfferRepository>());

            var paymentsInfoBc = new PaymentsInfoBc(mockUnitOfWork);

            // Act
            var result = paymentsInfoBc.GetCoProducerTotals();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // CoProducer with no subscriptions should be excluded
        }
    }
}

