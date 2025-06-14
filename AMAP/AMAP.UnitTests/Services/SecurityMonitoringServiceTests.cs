using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using AMAP.Entities;
using AMAP.Entities.Utilities;


namespace AMAP.UnitTests.Services
{
    public class SecurityMonitoringServiceTests
    {
        [Fact]
        public async Task RecordAuthActivityAsync_NullUserId_DoesNothing()
        {
            var logger = new Mock<ILogger<SecurityMonitoringService>>();
            var store = new Mock<IUserStore<AppUser>>();
            var userManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
            // Mock IConfiguration para EmailSender
            var config = new Mock<IConfiguration>();
            var section = new Mock<IConfigurationSection>();
            config.Setup(x => x.GetSection("MailSettings")).Returns(section.Object);
            section.Setup(x => x["Host"]).Returns("smtp.test.com");
            section.Setup(x => x["Port"]).Returns("25");
            section.Setup(x => x["Username"]).Returns("user");
            section.Setup(x => x["Password"]).Returns("pass");
            section.Setup(x => x["FromEmail"]).Returns("from@test.com");
            section.Setup(x => x["FromName"]).Returns("Test");
            var emailSender = new Mock<EmailSender>(config.Object);
            var service = new SecurityMonitoringService(logger.Object, userManager.Object, emailSender.Object);
            await service.RecordAuthActivityAsync(null, "1.2.3.4", "agent", AuthActivityType.Login);
            // No exception = pass
        }

        [Fact]
        public void CleanupOldData_DoesNotThrow()
        {
            var logger = new Mock<ILogger<SecurityMonitoringService>>();
            var store = new Mock<IUserStore<AppUser>>();
            var userManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
            var config = new Mock<IConfiguration>();
            var section = new Mock<IConfigurationSection>();
            config.Setup(x => x.GetSection("MailSettings")).Returns(section.Object);
            section.Setup(x => x["Host"]).Returns("smtp.test.com");
            section.Setup(x => x["Port"]).Returns("25");
            section.Setup(x => x["Username"]).Returns("user");
            section.Setup(x => x["Password"]).Returns("pass");
            section.Setup(x => x["FromEmail"]).Returns("from@test.com");
            section.Setup(x => x["FromName"]).Returns("Test");
            var emailSender = new Mock<EmailSender>(config.Object);
            var service = new SecurityMonitoringService(logger.Object, userManager.Object, emailSender.Object);
            service.CleanupOldData();
            // No exception = pass
        }
    }
}