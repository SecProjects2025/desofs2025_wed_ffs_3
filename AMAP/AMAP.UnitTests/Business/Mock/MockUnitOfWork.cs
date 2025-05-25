using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Business.Mock
{
    public static class MockUnitOfWork
    {
        public static IUnitOfWork GetMockUnitOfWork()
        {
            var mock = Substitute.For<IUnitOfWork>();

            // Mock Subscription repository
            var mockSubscriptionRepo = Substitute.For<IRepository<Subscription>>();
            mockSubscriptionRepo.Get(Arg.Any<Expression<Func<Subscription, bool>>>())
                .Returns(new Subscription { Id = 1, CoProducerId = "test-coproducer-id" });
            mock.Subscription.Returns(mockSubscriptionRepo);

            // Mock Payment repository
            var mockPaymentRepo = Substitute.For<IRepository<Payment>>();
            mockPaymentRepo.Get(Arg.Any<Expression<Func<Payment, bool>>>())
                .Returns(new Payment { Id = 100 });
            mock.Payment.Returns(mockPaymentRepo);

            // Mock PaymentValidationHistory repository
            var mockPaymentValidationHistoryRepo = Substitute.For<IRepository<PaymentValidationHistory>>();
            mock.PaymentValidationHistory.Returns(mockPaymentValidationHistoryRepo);

            return mock;
        }
    }
}