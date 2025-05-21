using AMAP.Business.Interfaces;
using AMAP.Entities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AMAP.UnitTests.Business.Mock
{
    public static class MockSubscriptionPeriodBc
    {
        public static ISubscriptionPeriodBc GetMockSubscriptionPeriodBc()
        {
            var mockSubscriptionPeriodBc = Substitute.For<ISubscriptionPeriodBc>();

            SetupGetSubscriptionPeriodById(mockSubscriptionPeriodBc);
            SetupGetDeliveryDateById(mockSubscriptionPeriodBc);
            SetupCreateSubscriptionPeriodAndDeliveryDateAsync(mockSubscriptionPeriodBc);
            SetupDeleteSubscriptionPeriodAsync(mockSubscriptionPeriodBc);

            return mockSubscriptionPeriodBc;
        }

        private static void SetupGetSubscriptionPeriodById(ISubscriptionPeriodBc mockSubscriptionPeriodBc)
        {
            // Valid subscription period (ID: 1)
            var deliveryDates = new List<DeliveryDate>
            {
                new DeliveryDate { Id = 1, Date = DateTime.Now.AddDays(7) },
                new DeliveryDate { Id = 2, Date = DateTime.Now.AddDays(14) },
                new DeliveryDate { Id = 3, Date = DateTime.Now.AddDays(21) }
            };

            mockSubscriptionPeriodBc.GetSubscriptionPeriodById(1).Returns(new SubscriptionPeriod
            {
                Id = 1,
                Duration = "Monthly",
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.AddMonths(1).Date,
                DeliveryDates = deliveryDates
            });

            // Subscription period with no delivery dates (ID: 2)
            mockSubscriptionPeriodBc.GetSubscriptionPeriodById(2).Returns(new SubscriptionPeriod
            {
                Id = 2,
                Duration = "Weekly",
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.AddDays(7).Date,
                DeliveryDates = new List<DeliveryDate>()
            });

            // Non-existing subscription period (ID: 999)
            mockSubscriptionPeriodBc.GetSubscriptionPeriodById(999).Returns((SubscriptionPeriod)null);

            // Invalid ID argument
            mockSubscriptionPeriodBc
                .GetSubscriptionPeriodById(Arg.Is<int>(id => id <= 0))
                .Throws(new ArgumentException("Subscription period ID must be greater than zero.", "id"));
        }

        private static void SetupGetDeliveryDateById(ISubscriptionPeriodBc mockSubscriptionPeriodBc)
        {
            // Valid delivery date (ID: 1)
            mockSubscriptionPeriodBc.GetDeliveryDateById(1).Returns(new DeliveryDate
            {
                Id = 1,
                Date = DateTime.Now.AddDays(7),
                SubscriptionPeriodId = 1
            });

            // Non-existing delivery date (ID: 999)
            mockSubscriptionPeriodBc.GetDeliveryDateById(999).Returns((DeliveryDate)null);

            // Invalid ID argument
            mockSubscriptionPeriodBc
                .GetDeliveryDateById(Arg.Is<int>(id => id <= 0))
                .Throws(new ArgumentException("Delivery date ID must be greater than zero.", "id"));
        }

        private static void SetupCreateSubscriptionPeriodAndDeliveryDateAsync(ISubscriptionPeriodBc mockSubscriptionPeriodBc)
        {
            // Valid subscription period creation
            mockSubscriptionPeriodBc
                .CreateSubscriptionPeriodAndDeliveryDateAsync(
                    Arg.Is<SubscriptionPeriod>(sp =>
                        sp != null &&
                        !string.IsNullOrEmpty(sp.Duration) &&
                        sp.EndDate > sp.StartDate)
                )
                .Returns(callInfo =>
                {
                    var sp = callInfo.Arg<SubscriptionPeriod>();
                    sp.Id = 100;  // Assign a new ID

                    // Assign IDs to delivery dates if they exist
                    if (sp.DeliveryDates != null && sp.DeliveryDates.Any())
                    {
                        int dateId = 500;
                        foreach (var date in sp.DeliveryDates)
                        {
                            date.Id = dateId++;
                            date.SubscriptionPeriodId = sp.Id;
                        }
                    }

                    return Task.FromResult(sp);
                });

            // Invalid subscription period (null)
            mockSubscriptionPeriodBc
                .CreateSubscriptionPeriodAndDeliveryDateAsync(
                    Arg.Is<SubscriptionPeriod>(sp => sp == null)
                )
                .Throws(new ArgumentException("Subscription period cannot be null.", "subscriptionPeriod"));

            // Invalid date range (end date before start date)
            mockSubscriptionPeriodBc
                .CreateSubscriptionPeriodAndDeliveryDateAsync(
                    Arg.Is<SubscriptionPeriod>(sp => sp != null && sp.EndDate <= sp.StartDate)
                )
                .Throws(new InvalidOperationException("End date must be after start date."));

            // Missing duration
            mockSubscriptionPeriodBc
                .CreateSubscriptionPeriodAndDeliveryDateAsync(
                    Arg.Is<SubscriptionPeriod>(sp => sp != null && string.IsNullOrEmpty(sp.Duration))
                )
                .Throws(new ArgumentException("Duration is required.", "Duration"));
        }

        private static void SetupDeleteSubscriptionPeriodAsync(ISubscriptionPeriodBc mockSubscriptionPeriodBc)
        {
            // Successful delete
            mockSubscriptionPeriodBc
                .DeleteSubscriptionPeriodAsync(
                    Arg.Is<int>(id => id > 0 && id != 999)
                )
                .Returns(Task.CompletedTask);

            // Non-existing subscription period
            mockSubscriptionPeriodBc
                .DeleteSubscriptionPeriodAsync(999)
                .Throws(new KeyNotFoundException("Subscription period not found."));

            // Invalid ID argument
            mockSubscriptionPeriodBc
                .DeleteSubscriptionPeriodAsync(Arg.Is<int>(id => id <= 0))
                .Throws(new ArgumentException("Subscription period ID must be greater than zero.", "id"));
        }
    }
}
