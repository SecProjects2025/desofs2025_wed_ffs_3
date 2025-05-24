using AMAP.Business.Interfaces;
using NSubstitute;

namespace AMAP.UnitTests.Business.Mock
{
    public static class MockDeliveryBc
    {
        public static IDeliveryBc GetMockDeliveryBc()
        {
            var mockDeliveryBc = Substitute.For<IDeliveryBc>();

            SetupUpdateDeliveredQuantity(mockDeliveryBc);

            return mockDeliveryBc;
        }

        private static void SetupUpdateDeliveredQuantity(IDeliveryBc mockDeliveryBc)
        {
            // Valid update
            mockDeliveryBc
                .When(x => x.UpdateDeliveredQuantity(
                    Arg.Is<int>(id => id > 0 && id < 100),
                    Arg.Is<int>(qty => qty >= 0),
                    Arg.Is<string>(pid => pid == "test-producer-id")))
                .Do(x => { /* Success - no action needed */ });

            // Invalid subscription ID
            mockDeliveryBc
                .When(x => x.UpdateDeliveredQuantity(
                    Arg.Is<int>(id => id <= 0),
                    Arg.Any<int>(),
                    Arg.Any<string>()))
                .Throw(new ArgumentException("Invalid subscription ID.", "subscriptionId"));

            // Invalid delivered quantity
            mockDeliveryBc
                .When(x => x.UpdateDeliveredQuantity(
                    Arg.Any<int>(),
                    Arg.Is<int>(qty => qty < 0),
                    Arg.Any<string>()))
                .Throw(new ArgumentException("Delivered quantity cannot be negative.", "deliveredQuantity"));

            // Subscription not found
            mockDeliveryBc
                .When(x => x.UpdateDeliveredQuantity(
                    Arg.Is<int>(id => id >= 100 && id < 1000),
                    Arg.Any<int>(),
                    Arg.Any<string>()))
                .Throw(new KeyNotFoundException("Subscription not found."));

            // Unauthorized (subscription does not belong to producer's product offer)
            mockDeliveryBc
                .When(x => x.UpdateDeliveredQuantity(
                    Arg.Is<int>(id => id >= 1000),
                    Arg.Any<int>(),
                    Arg.Any<string>()))
                .Throw(new UnauthorizedAccessException("You are not authorized to update this subscription."));

            // Missing producer ID
            mockDeliveryBc
                .When(x => x.UpdateDeliveredQuantity(
                    Arg.Any<int>(),
                    Arg.Any<int>(),
                    Arg.Is<string>(pid => string.IsNullOrEmpty(pid))))
                .Throw(new UnauthorizedAccessException("Producer not authenticated."));
        }
    }
}
