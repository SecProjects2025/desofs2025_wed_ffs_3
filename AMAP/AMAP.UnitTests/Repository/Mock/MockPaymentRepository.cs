using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using NSubstitute;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Repository.Mock
{
    public static class MockPaymentRepository
    {
        public static IUnitOfWork GetMockUnitOfWork()
        {
            var mockUnitOfWork = Substitute.For<IUnitOfWork>();

            // Set up Payment repository
            var mockPaymentRepo = Substitute.For<IPaymentRepository>();
            SetupPaymentRepository(mockPaymentRepo);
            mockUnitOfWork.Payment.Returns(mockPaymentRepo);

            // Set up Subscription repository
            var mockSubscriptionRepo = Substitute.For<ISubscriptionRepository>();
            SetupSubscriptionRepository(mockSubscriptionRepo);
            mockUnitOfWork.Subscription.Returns(mockSubscriptionRepo);

            return mockUnitOfWork;
        }

        private static void SetupPaymentRepository(IPaymentRepository mockPaymentRepo)
        {
            // Test data for payments
            var payments = new List<Payment>
            {
                new Payment
                {
                    Id = 1,
                    SubscriptionId = 1,
                    AmountPaid = 100.50,
                    PaymentDate = DateTime.UtcNow.AddDays(-5)
                },
                new Payment
                {
                    Id = 2,
                    SubscriptionId = 2,
                    AmountPaid = 75.25,
                    PaymentDate = DateTime.UtcNow.AddDays(-3)
                },
                new Payment
                {
                    Id = 3,
                    SubscriptionId = 3,
                    AmountPaid = 50.00,
                    PaymentDate = DateTime.UtcNow.AddDays(-1)
                }
            };

            // Mock Get method
            mockPaymentRepo.Get(Arg.Any<Expression<Func<Payment, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Payment, bool>>>();
                    var func = predicate.Compile();
                    return payments.FirstOrDefault(func);
                });

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

            // Mock Add method
            mockPaymentRepo.When(x => x.Add(Arg.Any<Payment>()))
                .Do(callInfo =>
                {
                    var payment = callInfo.Arg<Payment>();
                    payment.Id = payments.Count + 1;
                    payments.Add(payment);
                });
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
        }
    }
}
