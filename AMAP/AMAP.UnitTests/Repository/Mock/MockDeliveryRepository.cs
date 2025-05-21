using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using NSubstitute;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Repository.Mock
{
    public static class MockDeliveryRepository
    {
        public static IUnitOfWork GetMockUnitOfWork()
        {
            var mockUnitOfWork = Substitute.For<IUnitOfWork>();

            // Set up Delivery repository
            var mockDeliveryRepo = Substitute.For<IDeliveryRepository>();
            SetupDeliveryRepository(mockDeliveryRepo);
            mockUnitOfWork.Delivery.Returns(mockDeliveryRepo);

            // Set up Subscription repository
            var mockSubscriptionRepo = Substitute.For<ISubscriptionRepository>();
            SetupSubscriptionRepository(mockSubscriptionRepo);
            mockUnitOfWork.Subscription.Returns(mockSubscriptionRepo);

            return mockUnitOfWork;
        }

        private static void SetupDeliveryRepository(IDeliveryRepository mockDeliveryRepo)
        {
            // Test data for deliveries
            var deliveries = new List<Delivery>
            {
                new Delivery
                {
                    Id = 1,
                    SubscriptionId = 1,
                    DeliveredQuantity = 3
                },
                new Delivery
                {
                    Id = 2,
                    SubscriptionId = 2,
                    DeliveredQuantity = 2
                }
                // Subscription 3 doesn't have a delivery yet - will be created in the test
            };

            // Mock Get method
            mockDeliveryRepo.Get(Arg.Any<Expression<Func<Delivery, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Delivery, bool>>>();
                    var func = predicate.Compile();
                    return deliveries.FirstOrDefault(func);
                });

            // Mock GetAll method
            mockDeliveryRepo.GetAll(Arg.Any<Expression<Func<Delivery, bool>>>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Delivery, bool>>>();
                    if (predicate != null)
                    {
                        var func = predicate.Compile();
                        return deliveries.Where(func).ToList();
                    }
                    return deliveries;
                });

            // Mock Add method
            mockDeliveryRepo.When(x => x.Add(Arg.Any<Delivery>()))
                .Do(callInfo =>
                {
                    var delivery = callInfo.Arg<Delivery>();
                    delivery.Id = deliveries.Count + 1;
                    deliveries.Add(delivery);
                });

            // Mock Update method
            mockDeliveryRepo.When(x => x.Update(Arg.Any<Delivery>()))
                .Do(callInfo =>
                {
                    var updatedDelivery = callInfo.Arg<Delivery>();
                    var existingDelivery = deliveries.FirstOrDefault(d => d.Id == updatedDelivery.Id);
                    if (existingDelivery != null)
                    {
                        existingDelivery.DeliveredQuantity = updatedDelivery.DeliveredQuantity;
                    }
                });
        }

        private static void SetupSubscriptionRepository(ISubscriptionRepository mockSubscriptionRepo)
        {
            // Test data for products
            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Product 1",
                    ProducerId = "producer-1"
                },
                new Product
                {
                    Id = 2,
                    Name = "Product 2",
                    ProducerId = "producer-2"
                }
            };

            // Test data for product offers
            var productOffers = new List<ProductOffer>
            {
                new ProductOffer
                {
                    Id = 1,
                    ProductId = 1,
                    Product = products[0]
                },
                new ProductOffer
                {
                    Id = 2,
                    ProductId = 2,
                    Product = products[1]
                }
            };

            // Test data for subscriptions with included ProductOffer and Product
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = 1,
                    CoProducerId = "coproducer-1",
                    ProductOfferId = 1,
                    ExpectedQuantity = 5,
                    PaymentType = PaymentType.Money,
                    ProductOffer = productOffers[0]
                },
                new Subscription
                {
                    Id = 2,
                    CoProducerId = "coproducer-1",
                    ProductOfferId = 2,
                    ExpectedQuantity = 3,
                    PaymentType = PaymentType.BankTransfer,
                    ProductOffer = productOffers[1]
                },
                new Subscription
                {
                    Id = 3,
                    CoProducerId = "coproducer-2",
                    ProductOfferId = 1,
                    ExpectedQuantity = 2,
                    PaymentType = PaymentType.MBway,
                    ProductOffer = productOffers[0]
                }
            };

            // Mock Get method with includeProperties
            mockSubscriptionRepo.Get(Arg.Any<Expression<Func<Subscription, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Subscription, bool>>>();
                    var includeProperties = callInfo.Arg<string>();

                    var func = predicate.Compile();
                    var subscription = subscriptions.FirstOrDefault(func);

                    return subscription;
                });
        }
    }
}

