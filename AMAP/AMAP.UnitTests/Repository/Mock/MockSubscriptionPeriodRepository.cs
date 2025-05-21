using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Repository.Mock
{
    public static class MockSubscriptionPeriodRepository
    {
        public static IUnitOfWork GetMockUnitOfWork()
        {
            var mockUnitOfWork = Substitute.For<IUnitOfWork>();

            // Set up SubscriptionPeriod repository
            var mockSubscriptionPeriodRepo = Substitute.For<ISubscriptionPeriodRepository>();
            SetupSubscriptionPeriodRepository(mockSubscriptionPeriodRepo);
            mockUnitOfWork.SubscriptionPeriod.Returns(mockSubscriptionPeriodRepo);

            // Set up DeliveryDate repository
            var mockDeliveryDateRepo = Substitute.For<IDeliveryDateRepository>();
            SetupDeliveryDateRepository(mockDeliveryDateRepo);
            mockUnitOfWork.DeliveryDate.Returns(mockDeliveryDateRepo);

            // Set up AppUser repository for email notifications
            var mockAppUserRepo = Substitute.For<IAppUserRepository>();
            SetupAppUserRepository(mockAppUserRepo);
            mockUnitOfWork.AppUser.Returns(mockAppUserRepo);

            return mockUnitOfWork;
        }

        private static void SetupSubscriptionPeriodRepository(ISubscriptionPeriodRepository mockSubscriptionPeriodRepo)
        {
            // Test data for subscription periods
            var subscriptionPeriods = new List<SubscriptionPeriod>
            {
                new SubscriptionPeriod
                {
                    Id = 1,
                    Duration = "Winter 2023",
                    StartDate = new DateTime(2023, 12, 1),
                    EndDate = new DateTime(2024, 2, 28),
                    DeliveryDates = new List<DeliveryDate>
                    {
                        new DeliveryDate { Id = 1, Date = new DateTime(2023, 12, 15), SubscriptionPeriodId = 1 },
                        new DeliveryDate { Id = 2, Date = new DateTime(2024, 1, 15), SubscriptionPeriodId = 1 },
                        new DeliveryDate { Id = 3, Date = new DateTime(2024, 2, 15), SubscriptionPeriodId = 1 }
                    }
                },
                new SubscriptionPeriod
                {
                    Id = 2,
                    Duration = "Spring 2024",
                    StartDate = new DateTime(2024, 3, 1),
                    EndDate = new DateTime(2024, 5, 31),
                    DeliveryDates = new List<DeliveryDate>
                    {
                        new DeliveryDate { Id = 4, Date = new DateTime(2024, 3, 15), SubscriptionPeriodId = 2 },
                        new DeliveryDate { Id = 5, Date = new DateTime(2024, 4, 15), SubscriptionPeriodId = 2 },
                        new DeliveryDate { Id = 6, Date = new DateTime(2024, 5, 15), SubscriptionPeriodId = 2 }
                    }
                }
            };

            // Mock Get method
            mockSubscriptionPeriodRepo.Get(Arg.Any<Expression<Func<SubscriptionPeriod, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<SubscriptionPeriod, bool>>>();
                    var includeProperties = callInfo.Arg<string>();

                    var func = predicate.Compile();
                    var result = subscriptionPeriods.FirstOrDefault(func);

                    // If includeProperties contains "DeliveryDates", we've already included them above
                    return result;
                });

            // Mock GetAll method
            mockSubscriptionPeriodRepo.GetAll(Arg.Any<Expression<Func<SubscriptionPeriod, bool>>>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<SubscriptionPeriod, bool>>>();
                    if (predicate != null)
                    {
                        var func = predicate.Compile();
                        return subscriptionPeriods.Where(func).ToList();
                    }
                    return subscriptionPeriods;
                });

            // Mock Add method
            mockSubscriptionPeriodRepo.When(x => x.Add(Arg.Any<SubscriptionPeriod>()))
                .Do(callInfo =>
                {
                    var subscriptionPeriod = callInfo.Arg<SubscriptionPeriod>();
                    subscriptionPeriod.Id = subscriptionPeriods.Count + 1;
                    subscriptionPeriods.Add(subscriptionPeriod);
                });

            // Mock Remove method
            mockSubscriptionPeriodRepo.When(x => x.Remove(Arg.Any<SubscriptionPeriod>()))
                .Do(callInfo =>
                {
                    var subscriptionPeriod = callInfo.Arg<SubscriptionPeriod>();
                    var existingPeriod = subscriptionPeriods.FirstOrDefault(p => p.Id == subscriptionPeriod.Id);
                    if (existingPeriod != null)
                    {
                        subscriptionPeriods.Remove(existingPeriod);
                    }
                });
        }

        private static void SetupDeliveryDateRepository(IDeliveryDateRepository mockDeliveryDateRepo)
        {
            // Test data for delivery dates (already included in subscription periods above)
            var deliveryDates = new List<DeliveryDate>
            {
                new DeliveryDate { Id = 1, Date = new DateTime(2023, 12, 15), SubscriptionPeriodId = 1 },
                new DeliveryDate { Id = 2, Date = new DateTime(2024, 1, 15), SubscriptionPeriodId = 1 },
                new DeliveryDate { Id = 3, Date = new DateTime(2024, 2, 15), SubscriptionPeriodId = 1 },
                new DeliveryDate { Id = 4, Date = new DateTime(2024, 3, 15), SubscriptionPeriodId = 2 },
                new DeliveryDate { Id = 5, Date = new DateTime(2024, 4, 15), SubscriptionPeriodId = 2 },
                new DeliveryDate { Id = 6, Date = new DateTime(2024, 5, 15), SubscriptionPeriodId = 2 }
            };

            // Mock Get method
            mockDeliveryDateRepo.Get(Arg.Any<Expression<Func<DeliveryDate, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<DeliveryDate, bool>>>();
                    var func = predicate.Compile();
                    return deliveryDates.FirstOrDefault(func);
                });

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

        private static void SetupAppUserRepository(IAppUserRepository mockAppUserRepo)
        {
            // Test data for users (producers for email notifications)
            var users = new List<AppUser>
            {
                new AppUser
                {
                    Id = "producer-1",
                    Name = "Producer One",
                    Email = "producer1@example.com",
                    UserType = UserType.Producer
                },
                new AppUser
                {
                    Id = "producer-2",
                    Name = "Producer Two",
                    Email = "producer2@example.com",
                    UserType = UserType.Producer
                },
                new AppUser
                {
                    Id = "coproducer-1",
                    Name = "CoProducer One",
                    Email = "coproducer1@example.com",
                    UserType = UserType.CoProducer
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
        }

        // Helper method to create a TestEmailSender for unit tests
        public static EmailSender GetMockEmailSender()
        {
            // Create mock configuration
            var inMemorySettings = new Dictionary<string, string> {
                {"MailSettings:Host", "smtp.example.com"},
                {"MailSettings:Port", "587"},
                {"MailSettings:Username", "test@example.com"},
                {"MailSettings:Password", "testpassword"},
                {"MailSettings:FromEmail", "test@example.com"},
                {"MailSettings:FromName", "Test Sender"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            return new TestEmailSender(configuration);
        }
    }

    // Test-specific EmailSender to avoid actual email sending during tests
    public class TestEmailSender : EmailSender
    {
        public List<(string Email, string Subject, string Body)> SentEmails { get; } = new();
        private bool _throwException = false;

        public TestEmailSender(IConfiguration configuration) : base(configuration) { }

        public void SetThrowException(bool value)
        {
            _throwException = value;
        }

        public override Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (_throwException)
                throw new Exception("Test exception: Email sending failed");

            SentEmails.Add((toEmail, subject, body));
            return Task.CompletedTask;
        }
    }
}

