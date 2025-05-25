using AMAP.Business.Interfaces;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AMAP.UnitTests.Business.Mock
{
    public static class MockSubscriptionBc
    {
        public static ISubscriptionBc GetMockSubscriptionBc()
        {
            var mockSubscriptionBc = Substitute.For<ISubscriptionBc>();

            SetupCreateSubscription(mockSubscriptionBc);
            SetupGetSubscriptionById(mockSubscriptionBc);
            SetupGetSubscriptionsByCoProducer(mockSubscriptionBc);

            return mockSubscriptionBc;
        }

        private static void SetupCreateSubscription(ISubscriptionBc mockSubscriptionBc)
        {
            // Valid subscription creation
            mockSubscriptionBc
                .CreateSubscription(
                    Arg.Is<Subscription>(s =>
                        s != null &&
                        s.ProductOfferId > 0 &&
                        s.ExpectedQuantity > 0),
                    Arg.Is<string>(id => id == "test-coproducer-id")
                )
                .Returns(callInfo =>
                {
                    var s = callInfo.Arg<Subscription>();
                    s.Id = 100; // Assign a new ID
                    s.CoProducerId = "test-coproducer-id";
                    return s;
                });

            // Invalid subscription data
            mockSubscriptionBc
                .CreateSubscription(
                    Arg.Is<Subscription>(s => s == null || s.ExpectedQuantity <= 0),
                    Arg.Any<string>()
                )
                .Throws(new ArgumentException("Invalid subscription data.", "subscription"));

            // Missing co-producer ID
            mockSubscriptionBc
                .CreateSubscription(
                    Arg.Any<Subscription>(),
                    Arg.Is<string>(id => string.IsNullOrEmpty(id))
                )
                .Throws(new UnauthorizedAccessException("CoProducer not authenticated."));

            // Product offer not found
            mockSubscriptionBc
                .CreateSubscription(
                    Arg.Is<Subscription>(s => s != null && s.ProductOfferId == 999),
                    Arg.Any<string>()
                )
                .Throws(new KeyNotFoundException("Product offer not found."));

            // Invalid operation (e.g., subscription period ended)
            mockSubscriptionBc
                .CreateSubscription(
                    Arg.Is<Subscription>(s => s != null && s.ProductOfferId == 888),
                    Arg.Any<string>()
                )
                .Throws(new InvalidOperationException("Subscription period has ended."));
        }

        private static void SetupGetSubscriptionById(ISubscriptionBc mockSubscriptionBc)
        {
            // Valid subscription (ID: 1)
            mockSubscriptionBc.GetSubscriptionById(1).Returns(new Subscription
            {
                Id = 1,
                CoProducerId = "test-coproducer-id",
                ProductOfferId = 101,
                ExpectedQuantity = 5,
                PaymentType = PaymentType.Money
            });

            // Valid subscription (ID: 2)
            mockSubscriptionBc.GetSubscriptionById(2).Returns(new Subscription
            {
                Id = 2,
                CoProducerId = "test-coproducer-id",
                ProductOfferId = 102,
                ExpectedQuantity = 3,
                PaymentType = PaymentType.BankTransfer
            });

            // Invalid ID (negative or zero)
            mockSubscriptionBc
                .GetSubscriptionById(Arg.Is<int>(id => id <= 0))
                .Throws(new ArgumentException("Subscription ID must be positive.", "id"));

            // Non-existing subscription
            mockSubscriptionBc.GetSubscriptionById(999).Returns((Subscription)null);
        }

        private static void SetupGetSubscriptionsByCoProducer(ISubscriptionBc mockSubscriptionBc)
        {
            // Valid co-producer with subscriptions
            mockSubscriptionBc
                .GetSubscriptionsByCoProducer("test-coproducer-id")
                .Returns(new List<Subscription>
                {
                    new Subscription
                    {
                        Id = 1,
                        CoProducerId = "test-coproducer-id",
                        ProductOfferId = 101,
                        ExpectedQuantity = 5,
                        PaymentType = PaymentType.Money
                    },
                    new Subscription
                    {
                        Id = 2,
                        CoProducerId = "test-coproducer-id",
                        ProductOfferId = 102,
                        ExpectedQuantity = 3,
                        PaymentType = PaymentType.BankTransfer
                    }
                });

            // Co-producer with no subscriptions
            mockSubscriptionBc
                .GetSubscriptionsByCoProducer("no-subscription-coproducer")
                .Returns(new List<Subscription>());

            // Invalid co-producer ID
            mockSubscriptionBc
                .GetSubscriptionsByCoProducer(Arg.Is<string>(id => string.IsNullOrEmpty(id)))
                .Throws(new ArgumentException("Invalid CoProducer.", "coProducerId"));
        }
    }
}
