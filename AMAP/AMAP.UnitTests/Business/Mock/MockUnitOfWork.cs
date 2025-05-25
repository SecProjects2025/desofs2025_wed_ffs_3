using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Business.Mock
{
    public static class MockUnitOfWork
    {
        public static IUnitOfWork GetMockUnitOfWork()
        {
            var mock = Substitute.For<IUnitOfWork>();

            // Lista simulada de subscrições
            var subscriptions = new List<Subscription>
            {
                new Subscription { Id = 1, CoProducerId = "coproducer-1", ProductOfferId = 1, ExpectedQuantity = 5, PaymentType = PaymentType.Money },
                new Subscription { Id = 2, CoProducerId = "coproducer-1", ProductOfferId = 2, ExpectedQuantity = 3, PaymentType = PaymentType.Money }
            };
            int nextId = 3;

            // Mock Subscription repository
            var mockSubscriptionRepo = Substitute.For<ISubscriptionRepository>();
            mockSubscriptionRepo.Get(Arg.Any<Expression<Func<Subscription, bool>>>())
                .Returns(x =>
                {
                    var predicate = x.Arg<Expression<Func<Subscription, bool>>>();
                    var compiled = predicate.Compile();
                    return subscriptions.FirstOrDefault(compiled);
                });

            mockSubscriptionRepo.GetAll(Arg.Any<Expression<Func<Subscription, bool>>>())
                .Returns(x =>
                {
                    var predicate = x.Arg<Expression<Func<Subscription, bool>>>();
                    var compiled = predicate.Compile();
                    return subscriptions.Where(compiled);
                });

            mockSubscriptionRepo
                .When(x => x.Add(Arg.Any<Subscription>()))
                .Do(x =>
                {
                    var sub = x.Arg<Subscription>();
                    sub.Id = nextId++;
                    subscriptions.Add(sub);
                });

            mock.Subscription.Returns(mockSubscriptionRepo);

            // Mock ProductOffer repository
            var productOffers = new List<ProductOffer>
            {
                new ProductOffer { Id = 1, Quantity = 20, PaymentType = PaymentType.Money, SubscriptionPeriod = new SubscriptionPeriod { EndDate = DateTime.Now.AddDays(7) } },
                new ProductOffer { Id = 2, Quantity = 5, PaymentType = PaymentType.Money, SubscriptionPeriod = new SubscriptionPeriod { EndDate = DateTime.Now.AddDays(7) } },
                new ProductOffer { Id = 3, Quantity = 3, PaymentType = PaymentType.Money, SubscriptionPeriod = new SubscriptionPeriod { EndDate = DateTime.Now.AddDays(7) } },
                new ProductOffer { Id = 888, Quantity = 10, PaymentType = PaymentType.Money, SubscriptionPeriod = new SubscriptionPeriod { EndDate = DateTime.Now.AddDays(-1) } }
            };
            var mockProductOfferRepo = Substitute.For<IProductOfferRepository>();
            mockProductOfferRepo.Get(Arg.Any<Expression<Func<ProductOffer, bool>>>())
                .Returns(x =>
                {
                    var predicate = x.Arg<Expression<Func<ProductOffer, bool>>>();
                    var compiled = predicate.Compile();
                    return productOffers.FirstOrDefault(compiled);
                });
            mock.ProductOffer.Returns(mockProductOfferRepo);

            // Mock AppUser repository
            var mockAppUserRepo = Substitute.For<IAppUserRepository>();
            mockAppUserRepo.Get(Arg.Any<Expression<Func<AppUser, bool>>>())
                .Returns(x =>
                {
                    var predicate = x.Arg<Expression<Func<AppUser, bool>>>();
                    var compiled = predicate.Compile();
                    if (compiled(new AppUser { Id = "coproducer-1", Email = "test@example.com" }))
                        return new AppUser { Id = "coproducer-1", Email = "test@example.com" };
                    return null;
                });
            mock.AppUser.Returns(mockAppUserRepo);

            // Mock Payment repository
            var mockPaymentRepo = Substitute.For<IPaymentRepository>();
            mockPaymentRepo.Get(Arg.Any<Expression<Func<Payment, bool>>>())
                .Returns(new Payment { Id = 100 });
            mockPaymentRepo
                .When(x => x.Add(Arg.Any<Payment>()))
                .Do(x => { /* não faz nada, apenas evita erro */ });
            mock.Payment.Returns(mockPaymentRepo);

            // Mock PaymentValidationHistory repository
            var mockPaymentValidationHistoryRepo = Substitute.For<IPaymentValidationHistoryRepository>();
            mockPaymentValidationHistoryRepo
                .When(x => x.Add(Arg.Any<PaymentValidationHistory>()))
                .Do(x => { /* não faz nada, apenas evita erro */ });
            mock.PaymentValidationHistory.Returns(mockPaymentValidationHistoryRepo);

            // Simular Save
            // mock.Save();

            return mock;
        }
    }
}