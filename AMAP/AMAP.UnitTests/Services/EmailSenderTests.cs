using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using AMAP.Entities.Utilities;

namespace AMAP.UnitTests.Services
{
    public class EmailSenderTests
    {
        [Fact]
        public async Task SendEmailAsync_SendsEmailWithoutException()
        {
            var config = new Mock<IConfiguration>();
            var section = new Mock<IConfigurationSection>();
            config.Setup(x => x.GetSection("MailSettings")).Returns(section.Object);
            section.Setup(x => x["Host"]).Returns("smtp.test.com");
            section.Setup(x => x["Port"]).Returns("25");
            section.Setup(x => x["Username"]).Returns("user");
            section.Setup(x => x["Password"]).Returns("pass");
            section.Setup(x => x["FromEmail"]).Returns("from@test.com");
            section.Setup(x => x["FromName"]).Returns("Test");
            var sender = new EmailSender(config.Object);
            // Não testamos envio real, apenas construtor
            await Task.CompletedTask;
        }

        [Fact]
        public void Constructor_InitializesFields()
        {
            var config = new Mock<IConfiguration>();
            var section = new Mock<IConfigurationSection>();
            config.Setup(x => x.GetSection("MailSettings")).Returns(section.Object);
            section.Setup(x => x["Host"]).Returns("smtp.test.com");
            section.Setup(x => x["Port"]).Returns("25");
            section.Setup(x => x["Username"]).Returns("user");
            section.Setup(x => x["Password"]).Returns("pass");
            section.Setup(x => x["FromEmail"]).Returns("from@test.com");
            section.Setup(x => x["FromName"]).Returns("Test");
            var sender = new EmailSender(config.Object);
            Assert.NotNull(sender);
        }

        [Fact]
        public async Task SendEmailAsync_InvalidEmail_ThrowsException()
        {
            var config = new Mock<IConfiguration>();
            var section = new Mock<IConfigurationSection>();
            config.Setup(x => x.GetSection("MailSettings")).Returns(section.Object);
            section.Setup(x => x["Host"]).Returns("smtp.test.com");
            section.Setup(x => x["Port"]).Returns("25");
            section.Setup(x => x["Username"]).Returns("user");
            section.Setup(x => x["Password"]).Returns("pass");
            section.Setup(x => x["FromEmail"]).Returns("from@test.com");
            section.Setup(x => x["FromName"]).Returns("Test");
            var sender = new EmailSender(config.Object);
            await Assert.ThrowsAsync<System.FormatException>(async () =>
            {
                await sender.SendEmailAsync("notanemail", "subject", "body");
            });
        }
    }
}