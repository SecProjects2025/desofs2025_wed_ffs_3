using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using NSubstitute;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Repository.Mock
{
    public static class MockPaymentsInfoRepository
    {
        public static IUnitOfWork GetMockUnitOfWork()
        {
            var mockUnitOfWork = Substitute.For<IUnitOfWork>();

            // Set up AppUser repository
            var mockAppUserRepo = Substitute.For<IAppUserRepository>();
            SetupAppUserRepository(mockAppUserRepo);
            mockUnitOfWork.AppUser.Returns(mockAppUserRepo);

            // Set up Product repository
            var mockProductRepo = Substitute.For<IProductRepository>();
            SetupProductRepository(mockProductRepo);
            mockUnitOfWork.Product.Returns(mockProductRepo);

            // Set up ProductOffer repository
            var mockProductOfferRepo = Substitute.For<IProductOfferRepository>();
            SetupProductOfferRepository(mockProductOfferRepo);
            mockUnitOfWork.ProductOffer.Returns(mockProductOfferRepo);

            // Set up Subscription repository
            var mockSubscriptionRepo = Substitute.For<ISubscriptionRepository>();
            SetupSubscriptionRepository(mockSubscriptionRepo);
            mockUnitOfWork.Subscription.Returns(mockSubscriptionRepo);

            // Set up Payment repository
            var mockPaymentRepo = Substitute.For<IPaymentRepository>();
            SetupPaymentRepository(mockPaymentRepo);
            mockUnitOfWork.Payment.Returns(mockPaymentRepo);

            // Set up Delivery repository
            var mockDeliveryRepo = Substitute.For<IDeliveryRepository>();
            SetupDeliveryRepository(mockDeliveryRepo);
            mockUnitOfWork.Delivery.Returns(mockDeliveryRepo);

            return mockUnitOfWork;
        }

        private static void SetupAppUserRepository(IAppUserRepository mockAppUserRepo)
        {
            // Test data for AppUsers
            var users = new List<AppUser>
            {
                new AppUser
                {
                    Id = "producer-1",
                    Name = "Producer One",
                    UserType = UserType.Producer
                },
                new AppUser
                {
                    Id = "producer-2",
                    Name = "Producer Two",
                    UserType = UserType.Producer
                },
                new AppUser
                {
                    Id = "coproducer-1",
                    Name = "CoProducer One",
                    UserType = UserType.CoProducer
                },
                new AppUser
                {
                    Id = "coproducer-2",
                    Name = "CoProducer Two",
                    UserType = UserType.CoProducer
                },
                new AppUser
                {
                    Id = "manager-1",
                    Name = "Manager",
                    UserType = UserType.Manager
                }
            };

            // Mock GetAll method
            mockAppUserRepo.GetAll(Arg.Any<Expression<Func<AppUser, bool>>>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<AppUser, bool>>>();
                    if (predicate != null)
                    {
                        var func = predicate.Compile();
                        return users.Where(func).ToList();
                    }
                    return users;
                });

            // Mock Get method
            mockAppUserRepo.Get(Arg.Any<Expression<Func<AppUser, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<AppUser, bool>>>();
                    var func = predicate.Compile();
                    return users.FirstOrDefault(func);
                });
        }

        private static void SetupProductRepository(IProductRepository mockProductRepo)
        {
            // Test data for Products
            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Carrots",
                    ProducerId = "producer-1",
                    ReferencePrice = 2.5
                },
                new Product
                {
                    Id = 2,
                    Name = "Potatoes",
                    ProducerId = "producer-1",
                    ReferencePrice = 1.5
                },
                new Product
                {
                    Id = 3,
                    Name = "Apples",
                    ProducerId = "producer-2",
                    ReferencePrice = 3.0
                }
            };

            // Mock GetAll method
            mockProductRepo.GetAll(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Product, bool>>>();
                    if (predicate != null)
                    {
                        var func = predicate.Compile();
                        return products.Where(func).ToList();
                    }
                    return products;
                });
        }

        private static void SetupProductOfferRepository(IProductOfferRepository mockProductOfferRepo)
        {
            // Test data for ProductOffers
            var productOffers = new List<ProductOffer>
            {
                new ProductOffer
                {
                    Id = 1,
                    ProductId = 1,
                    Price = 2.5,
                    Quantity = 100,
                },
                new ProductOffer
                {
                    Id = 2,
                    ProductId = 2,
                    Price = 1.5,
                    Quantity = 50,
                },
                new ProductOffer
                {
                    Id = 3,
                    ProductId = 3,
                    Price = 3.0,
                    Quantity = 75,
                }
            };

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

            // Mock Get method
            mockProductOfferRepo.Get(Arg.Any<Expression<Func<ProductOffer, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<ProductOffer, bool>>>();
                    var func = predicate.Compile();
                    return productOffers.FirstOrDefault(func);
                });
        }

        private static void SetupSubscriptionRepository(ISubscriptionRepository mockSubscriptionRepo)
        {
            // Test data for Subscriptions
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = 1,
                    ProductOfferId = 1,
                    CoProducerId = "coproducer-1",
                    ExpectedQuantity = 5
                },
                new Subscription
                {
                    Id = 2,
                    ProductOfferId = 1,
                    CoProducerId = "coproducer-2",
                    ExpectedQuantity = 3
                },
                new Subscription
                {
                    Id = 3,
                    ProductOfferId = 2,
                    CoProducerId = "coproducer-1",
                    ExpectedQuantity = 4
                },
                new Subscription
                {
                    Id = 4,
                    ProductOfferId = 3,
                    CoProducerId = "coproducer-2",
                    ExpectedQuantity = 2
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

        private static void SetupPaymentRepository(IPaymentRepository mockPaymentRepo)
        {
            // Test data for Payments
            var payments = new List<Payment>
            {
                new Payment
                {
                    Id = 1,
                    SubscriptionId = 1,
                    AmountPaid = 7.5,
                    PaymentDate = DateTime.Now.AddDays(-5)
                },
                new Payment
                {
                    Id = 2,
                    SubscriptionId = 2,
                    AmountPaid = 3.75,
                    PaymentDate = DateTime.Now.AddDays(-3)
                },
                new Payment
                {
                    Id = 3,
                    SubscriptionId = 3,
                    AmountPaid = 4.0,
                    PaymentDate = DateTime.Now.AddDays(-1)
                },
                // No payment for subscription ID 4
            };

            // Mock GetAll method
            mockPaymentRepo.GetAll(Arg.Any<Expression<Func<Payment, bool>>>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Payment, bool>>>();
                    if (predicate != null)
                    {
                        var func = predicate.Compile();
                        return payments.Where(func).ToList();
                    }
                    return payments;
                });
        }

        private static void SetupDeliveryRepository(IDeliveryRepository mockDeliveryRepo)
        {
            // Test data for Deliveries
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
                },
                // No delivery for subscription ID 3 and 4
            };

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
        }
    }
}

