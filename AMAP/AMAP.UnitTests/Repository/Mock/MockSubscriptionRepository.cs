using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using NSubstitute;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Repository.Mock
{
    public static class MockSubscriptionRepository
    {
        public static IUnitOfWork GetMockUnitOfWork()
        {
            var mockUnitOfWork = Substitute.For<IUnitOfWork>();

            // Set up Subscription repository
            var mockSubscriptionRepo = Substitute.For<ISubscriptionRepository>();
            SetupSubscriptionRepository(mockSubscriptionRepo);
            mockUnitOfWork.Subscription.Returns(mockSubscriptionRepo);

            // Set up ProductOffer repository
            var mockProductOfferRepo = Substitute.For<IProductOfferRepository>();
            SetupProductOfferRepository(mockProductOfferRepo);
            mockUnitOfWork.ProductOffer.Returns(mockProductOfferRepo);

            // Set up AppUser repository (for CoProducers)
            var mockAppUserRepo = Substitute.For<IAppUserRepository>();
            SetupAppUserRepository(mockAppUserRepo);
            mockUnitOfWork.AppUser.Returns(mockAppUserRepo);

            return mockUnitOfWork;
        }

        private static void SetupSubscriptionRepository(ISubscriptionRepository mockSubscriptionRepo)
        {
            // Test data for subscriptions
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = 1,
                    CoProducerId = "coproducer-1",
                    ProductOfferId = 1,
                    ExpectedQuantity = 5,
                    PaymentType = PaymentType.Money
                },
                new Subscription
                {
                    Id = 2,
                    CoProducerId = "coproducer-1",
                    ProductOfferId = 2,
                    ExpectedQuantity = 3,
                    PaymentType = PaymentType.BankTransfer
                },
                new Subscription
                {
                    Id = 3,
                    CoProducerId = "coproducer-2",
                    ProductOfferId = 1,
                    ExpectedQuantity = 2,
                    PaymentType = PaymentType.MBway
                }
            };

            // Mock Get method
            mockSubscriptionRepo.Get(Arg.Any<Expression<Func<Subscription, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Subscription, bool>>>();
                    var func = predicate.Compile();
                    return subscriptions.FirstOrDefault(func);
                });

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

            // Mock Add method
            mockSubscriptionRepo.When(x => x.Add(Arg.Any<Subscription>()))
                .Do(callInfo =>
                {
                    var subscription = callInfo.Arg<Subscription>();
                    subscription.Id = subscriptions.Count + 1;
                    subscriptions.Add(subscription);
                });
        }

        private static void SetupProductOfferRepository(IProductOfferRepository mockProductOfferRepo)
        {
            // Test data for product offers
            var productOffers = new List<ProductOffer>
            {
                new ProductOffer
                {
                    Id = 1,
                    ProductId = 1,
                    SubscriptionPeriodId = 1,
                    Price = 2.99,
                    Quantity = 20, // Total quantity available
                    DeliveryDate = DateTime.Now.AddDays(7),
                    PaymentType = PaymentType.Money | PaymentType.MBway
                },
                new ProductOffer
                {
                    Id = 2,
                    ProductId = 2,
                    SubscriptionPeriodId = 1,
                    Price = 3.99,
                    Quantity = 15,
                    DeliveryDate = DateTime.Now.AddDays(14),
                    PaymentType = PaymentType.BankTransfer
                },
                new ProductOffer
                {
                    Id = 3,
                    ProductId = 3,
                    SubscriptionPeriodId = 1,
                    Price = 5.99,
                    Quantity = 10,
                    DeliveryDate = DateTime.Now.AddDays(21),
                    PaymentType = PaymentType.Money | PaymentType.BankTransfer
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
        }

        private static void SetupAppUserRepository(IAppUserRepository mockAppUserRepo)
        {
            // Test data for users
            var users = new List<AppUser>
            {
                new AppUser
                {
                    Id = "coproducer-1",
                    Name = "Test CoProducer 1",
                    UserType = UserType.CoProducer,
                    Email = "coproducer1@test.com"
                },
                new AppUser
                {
                    Id = "coproducer-2",
                    Name = "Test CoProducer 2",
                    UserType = UserType.CoProducer,
                    Email = "coproducer2@test.com"
                }
            };

            // Mock Get method
            mockAppUserRepo.Get(Arg.Any<Expression<Func<AppUser, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<AppUser, bool>>>();
                    var func = predicate.Compile();
                    return users.FirstOrDefault(func);
                });
        }
    }
}
