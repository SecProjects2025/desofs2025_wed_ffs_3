using AMAP.Business.Interfaces;
using AMAP.Entities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AMAP.UnitTests.Business.Mock
{
    public static class MockPaymentBc
    {
        public static IPaymentBc GetMockPaymentBc()
        {
            var mockPaymentBc = Substitute.For<IPaymentBc>();

            SetupCreatePayment(mockPaymentBc);

            return mockPaymentBc;
        }

        private static void SetupCreatePayment(IPaymentBc mockPaymentBc)
        {
            // Valid payment creation
            mockPaymentBc
                .CreatePayment(
                    Arg.Is<Payment>(p =>
                        p != null &&
                        p.SubscriptionId > 0 &&
                        p.AmountPaid > 0),
                    Arg.Is<string>(id => id == "test-coproducer-id")
                )
                .Returns(callInfo =>
                {
                    var p = callInfo.Arg<Payment>();
                    p.Id = 100; // Assign a new ID
                    return p;
                });

            // Null payment
            mockPaymentBc
                .CreatePayment(
                    Arg.Is<Payment>(p => p == null),
                    Arg.Any<string>()
                )
                .Throws(new ArgumentNullException("payment", "Payment cannot be null."));

            // Invalid payment data
            mockPaymentBc
                .CreatePayment(
                    Arg.Is<Payment>(p =>
                        p != null &&
                        (p.SubscriptionId <= 0 || p.AmountPaid <= 0)),
                    Arg.Any<string>()
                )
                .Throws(new ArgumentException("Invalid payment data."));

            // Subscription not found
            mockPaymentBc
                .CreatePayment(
                    Arg.Is<Payment>(p =>
                        p != null &&
                        p.SubscriptionId == 999),
                    Arg.Any<string>()
                )
                .Throws(new KeyNotFoundException("Subscription not found."));

            // Unauthorized access (subscription doesn't belong to the co-producer)
            mockPaymentBc
                .CreatePayment(
                    Arg.Is<Payment>(p =>
                        p != null &&
                        p.SubscriptionId == 888),
                    Arg.Any<string>()
                )
                .Throws(new UnauthorizedAccessException("You are not authorized to create payment for this subscription."));

            // Missing co-producer ID
            mockPaymentBc
                .CreatePayment(
                    Arg.Any<Payment>(),
                    Arg.Is<string>(id => string.IsNullOrEmpty(id))
                )
                .Throws(new UnauthorizedAccessException("CoProducer not authenticated."));
        }
    }
}
