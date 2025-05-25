using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using NSubstitute;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Repository.Mock
{
    public static class MockProductOfferRepository
    {
        public static IUnitOfWork GetMockUnitOfWork()
        {
            var mockUnitOfWork = Substitute.For<IUnitOfWork>();

            // Set up Product repository
            var mockProductRepo = Substitute.For<IProductRepository>();
            SetupProductRepository(mockProductRepo);
            mockUnitOfWork.Product.Returns(mockProductRepo);

            // Set up ProductOffer repository
            var mockProductOfferRepo = Substitute.For<IProductOfferRepository>();
            SetupProductOfferRepository(mockProductOfferRepo);
            mockUnitOfWork.ProductOffer.Returns(mockProductOfferRepo);

            // Set up SubscriptionPeriod repository
            var mockSubscriptionPeriodRepo = Substitute.For<ISubscriptionPeriodRepository>();
            SetupSubscriptionPeriodRepository(mockSubscriptionPeriodRepo);
            mockUnitOfWork.SubscriptionPeriod.Returns(mockSubscriptionPeriodRepo);

            // Set up DeliveryDate repository
            var mockDeliveryDateRepo = Substitute.For<IDeliveryDateRepository>();
            SetupDeliveryDateRepository(mockDeliveryDateRepo);
            mockUnitOfWork.DeliveryDate.Returns(mockDeliveryDateRepo);

            // Set up Subscription repository
            var mockSubscriptionRepo = Substitute.For<ISubscriptionRepository>();
            SetupSubscriptionRepository(mockSubscriptionRepo);
            mockUnitOfWork.Subscription.Returns(mockSubscriptionRepo);

            return mockUnitOfWork;
        }

        private static void SetupProductRepository(IProductRepository mockProductRepo)
        {
            // Test products data
            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Carrots",
                    Description = "Fresh organic carrots",
                    ProducerId = "producer-1",
                    ReferencePrice = 2.50,
                    DeliveryUnits = "kg",
                    IsDeleted = false
                },
                new Product
                {
                    Id = 2,
                    Name = "Potatoes",
                    Description = "Fresh organic potatoes",
                    ProducerId = "producer-2",
                    ReferencePrice = 1.75,
                    DeliveryUnits = "kg",
                    IsDeleted = false
                },
                new Product
                {
                    Id = 3,
                    Name = "Apples",
                    Description = "Fresh organic apples",
                    ProducerId = "producer-1",
                    ReferencePrice = 3.25,
                    DeliveryUnits = "kg",
                    IsDeleted = false
                }
            };

            // Mock Get method
            mockProductRepo.Get(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Product, bool>>>();
                    var func = predicate.Compile();
                    return products.FirstOrDefault(func);
                });
        }

        private static void SetupProductOfferRepository(IProductOfferRepository mockProductOfferRepo)
        {
            // Test product offers data
            var productOffers = new List<ProductOffer>
            {
                new ProductOffer
                {
                    Id = 1,
                    ProductId = 1,
                    SubscriptionPeriodId = 1,
                    Price = 2.99,
                    Quantity = 100,
                    DeliveryDate = DateTime.Now.AddDays(7),
                    PaymentType = PaymentType.Money | PaymentType.MBway,
                    Product = new Product
                    {
                        Id = 1,
                        ProducerId = "producer-1"
                    }
                },
                new ProductOffer
                {
                    Id = 2,
                    ProductId = 2,
                    SubscriptionPeriodId = 1,
                    Price = 1.99,
                    Quantity = 75,
                    DeliveryDate = DateTime.Now.AddDays(7),
                    PaymentType = PaymentType.BankTransfer,
                    Product = new Product
                    {
                        Id = 2,
                        ProducerId = "producer-2"
                    }
                },
                new ProductOffer
                {
                    Id = 3,
                    ProductId = 3,
                    SubscriptionPeriodId = 1,
                    Price = 3.49,
                    Quantity = 50,
                    DeliveryDate = DateTime.Now.AddDays(14),
                    PaymentType = PaymentType.Money,
                    Product = new Product
                    {
                        Id = 3,
                        ProducerId = "producer-1"
                    }
                }
            };

            // Mock Get method
            mockProductOfferRepo.Get(Arg.Any<Expression<Func<ProductOffer, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<ProductOffer, bool>>>();
                    var func = predicate.Compile();
                    return productOffers.FirstOrDefault(func);
                });

            // Mock GetAll method
            mockProductOfferRepo.GetAll(Arg.Any<Expression<Func<ProductOffer, bool>>>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<ProductOffer, bool>>>();
                    if (predicate != null)
                    {
                        var func = predicate.Compile();
                        return productOffers.Where(func).ToList();
                    }
                    return productOffers;
                });

            // Mock Add method
            mockProductOfferRepo.When(x => x.Add(Arg.Any<ProductOffer>()))
                .Do(callInfo =>
                {
                    var productOffer = callInfo.Arg<ProductOffer>();
                    productOffer.Id = productOffers.Count + 1;
                    productOffers.Add(productOffer);
                });

            // Mock Remove method
            mockProductOfferRepo.When(x => x.Remove(Arg.Any<ProductOffer>()))
                .Do(callInfo =>
                {
                    var productOffer = callInfo.Arg<ProductOffer>();
                    var existingOffer = productOffers.FirstOrDefault(po => po.Id == productOffer.Id);
                    if (existingOffer != null)
                    {
                        productOffers.Remove(existingOffer);
                    }
                });
        }

        private static void SetupSubscriptionPeriodRepository(ISubscriptionPeriodRepository mockSubscriptionPeriodRepo)
        {
            // Test subscription periods data
            var subscriptionPeriods = new List<SubscriptionPeriod>
            {
                new SubscriptionPeriod
                {
                    Id = 1,
                    Duration = "Spring 2023",
                    StartDate = new DateTime(2023, 3, 1),
                    EndDate = new DateTime(2023, 5, 31)
                }
            };

            // Mock Get method
            mockSubscriptionPeriodRepo.Get(Arg.Any<Expression<Func<SubscriptionPeriod, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<SubscriptionPeriod, bool>>>();
                    var func = predicate.Compile();
                    return subscriptionPeriods.FirstOrDefault(func);
                });
        }

        private static void SetupDeliveryDateRepository(IDeliveryDateRepository mockDeliveryDateRepo)
        {
            // Test delivery dates data - ensure dates align with test values
            var deliveryDates = new List<DeliveryDate>
            {
                new DeliveryDate
                {
                    Id = 1,
                    SubscriptionPeriodId = 1,
                    Date = DateTime.Now.AddDays(7) // Match the test value
                },
                new DeliveryDate
                {
                    Id = 2,
                    SubscriptionPeriodId = 1,
                    Date = DateTime.Now.AddDays(14)
                },
                new DeliveryDate
                {
                    Id = 3,
                    SubscriptionPeriodId = 1,
                    Date = DateTime.Now.AddDays(21)
                }
            };

            // Mock GetAll method
            mockDeliveryDateRepo.GetAll(Arg.Any<Expression<Func<DeliveryDate, bool>>>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<DeliveryDate, bool>>>();
                    if (predicate != null)
                    {
                        var func = predicate.Compile();
                        return deliveryDates.Where(func).ToList();
                    }
                    return deliveryDates;
                });
        }


        private static void SetupSubscriptionRepository(ISubscriptionRepository mockSubscriptionRepo)
        {
            // Test subscriptions data
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = 1,
                    ProductOfferId = 1,
                    CoProducerId = "coproducer-1",
                    ExpectedQuantity = 5,
                    PaymentType = PaymentType.Money
                },
                new Subscription
                {
                    Id = 2,
                    ProductOfferId = 50, // This productOfferId is used in tests for offers with subscriptions
                    CoProducerId = "coproducer-2",
                    ExpectedQuantity = 3,
                    PaymentType = PaymentType.BankTransfer
                }
            };

            // Mock GetAll method
            mockSubscriptionRepo.GetAll(Arg.Any<Expression<Func<Subscription, bool>>>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Subscription, bool>>>();
                    if (predicate != null)
                    {
                        var func = predicate.Compile();
                        return subscriptions.Where(func).ToList();
                    }
                    return subscriptions;
                });
        }
    }
}
