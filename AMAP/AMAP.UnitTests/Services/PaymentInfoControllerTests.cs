using AMAP.Api.Controllers;
using AMAP.Business.Interfaces;
using AMAP.Entities.Models.DTO.PaymentInfo;
using AMAP.Entities.Utilities;
using AMAP.UnitTests.Business.Mock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace AMAP.UnitTests.Services
{
    public class PaymentsInfoControllerTests
    {
        private readonly IPaymentsInfoBc _paymentsInfoBc;
        private readonly PaymentsInfoController _paymentsInfoController;

        public PaymentsInfoControllerTests()
        {
            _paymentsInfoBc = MockPaymentsInfoBc.GetMockPaymentsInfoBc();
            _paymentsInfoController = new PaymentsInfoController(_paymentsInfoBc);

            // Reset all mock flags before each test
            ResetAllFlags();

            // Setup controller with claims for Manager role
            SetupControllerWithManagerRole();
        }

        private void ResetAllFlags()
        {
            MockPaymentsInfoFlag.IsCoProducerPaymentsEmpty = false;
            MockPaymentsInfoFlag.IsProducerPaymentsEmpty = false;
            MockPaymentsInfoFlag.IsProducerToReceiveEmpty = false;
            MockPaymentsInfoFlag.IsCoProducerToPayEmpty = false;
            MockPaymentsInfoFlag.IsProducerTotalsEmpty = false;
            MockPaymentsInfoFlag.IsCoProducerTotalsEmpty = false;
            MockPaymentsInfoFlag.IsProducerCCEmpty = false;
            MockPaymentsInfoFlag.IsCoProducerCCEmpty = false;
        }

        private void SetupControllerWithManagerRole()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "manager-id"),
                new Claim(ClaimTypes.Role, "Manager")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _paymentsInfoController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public void GetCoProducerPayments_WithData_ReturnsOk()
        {
            // Arrange

            // Act
            var result = _paymentsInfoController.GetCoProducerPayments();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var mappedResult = Assert.IsAssignableFrom<Dictionary<string, PaymentInfoDto>>(okResult.Value);
            Assert.Equal(2, mappedResult.Count);

            // Verify first co-producer data
            var coProducer1 = mappedResult["coproducer-1"];
            Assert.Equal("Co-Producer 1", coProducer1.User.Name);
            Assert.Equal(2, coProducer1.Subscriptions.Count());
            Assert.Equal(3, coProducer1.Payments.Count());

            // Verify subscription data for first co-producer
            var subscriptions = coProducer1.Subscriptions.ToList();
            Assert.Equal(101, subscriptions[0].ProductOfferId);
            Assert.Equal(5, subscriptions[0].ExpectedQuantity);
            Assert.Equal(PaymentType.Money, subscriptions[0].PaymentType);

            // Verify payment data for first co-producer
            var payments = coProducer1.Payments.ToList();
            Assert.Equal(50.0, payments[0].AmountPaid);
            Assert.Equal(1, payments[0].SubscriptionId);
        }

        [Fact]
        public void GetCoProducerPayments_NoData_ReturnsNotFound()
        {
            // Arrange
            MockPaymentsInfoFlag.IsCoProducerPaymentsEmpty = true;

            // Act
            var result = _paymentsInfoController.GetCoProducerPayments();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("No co-producer payments found.", notFoundResult.Value);
        }

        [Fact]
        public void GetProducerPayments_WithData_ReturnsOk()
        {
            // Arrange

            // Act
            var result = _paymentsInfoController.GetProducerPayments();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var mappedResult = Assert.IsAssignableFrom<Dictionary<string, PaymentInfoDto>>(okResult.Value);
            Assert.Equal(2, mappedResult.Count);

            // Verify first producer data
            var producer1 = mappedResult["producer-1"];
            Assert.Equal("Producer 1", producer1.User.Name);
            Assert.Single(producer1.Subscriptions);
            Assert.Single(producer1.Payments);
        }

        [Fact]
        public void GetProducerPayments_NoData_ReturnsNotFound()
        {
            // Arrange
            MockPaymentsInfoFlag.IsProducerPaymentsEmpty = true;

            // Act
            var result = _paymentsInfoController.GetProducerPayments();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("No producer payments found.", notFoundResult.Value);
        }

        [Fact]
        public void GetProducerToReceive_WithData_ReturnsOk()
        {
            // Arrange

            // Act
            var result = _paymentsInfoController.GetProducerToReceive();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var producerToReceive = Assert.IsAssignableFrom<Dictionary<string, double>>(okResult.Value);
            Assert.Equal(2, producerToReceive.Count);
            Assert.Equal(75.50, producerToReceive["producer-1"]);
            Assert.Equal(120.25, producerToReceive["producer-2"]);
        }

        [Fact]
        public void GetProducerToReceive_NoData_ReturnsNotFound()
        {
            // Arrange
            MockPaymentsInfoFlag.IsProducerToReceiveEmpty = true;

            // Act
            var result = _paymentsInfoController.GetProducerToReceive();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("No producer payments to receive found.", notFoundResult.Value);
        }

        [Fact]
        public void GetCoProducerToPay_WithData_ReturnsOk()
        {
            // Arrange

            // Act
            var result = _paymentsInfoController.GetCoProducerToPay();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var coProducerToPay = Assert.IsAssignableFrom<Dictionary<string, double>>(okResult.Value);
            Assert.Equal(2, coProducerToPay.Count);
            Assert.Equal(45.75, coProducerToPay["coproducer-1"]);
            Assert.Equal(60.25, coProducerToPay["coproducer-2"]);
        }

        [Fact]
        public void GetCoProducerToPay_NoData_ReturnsNotFound()
        {
            // Arrange
            MockPaymentsInfoFlag.IsCoProducerToPayEmpty = true;

            // Act
            var result = _paymentsInfoController.GetCoProducerToPay();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("No co-producer payments to pay found.", notFoundResult.Value);
        }

        [Fact]
        public void GetProducerPaymentsTotals_WithData_ReturnsOk()
        {
            // Arrange

            // Act
            var result = _paymentsInfoController.GetProducerPaymentsTotals();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Use System.Text.Json to convert the anonymous object to a testable format
            var resultJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var deserializedResult = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(resultJson);

            Assert.NotNull(deserializedResult);
            Assert.Equal(2, deserializedResult.Count);

            // Verify producer 1 data using the JsonElement properties
            Assert.True(deserializedResult.ContainsKey("producer-1"));
            var producer1 = deserializedResult["producer-1"];

            Assert.Equal("Producer 1", producer1.GetProperty("Name").GetString());
            Assert.Equal(100.0, producer1.GetProperty("ExpectedAmount").GetDouble());
            Assert.Equal(75.50, producer1.GetProperty("ReceiveAmount").GetDouble());
        }


        [Fact]
        public void GetProducerPaymentsTotals_NoData_ReturnsNotFound()
        {
            // Arrange
            MockPaymentsInfoFlag.IsProducerTotalsEmpty = true;

            // Act
            var result = _paymentsInfoController.GetProducerPaymentsTotals();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("No producer totals found.", notFoundResult.Value);
        }

        [Fact]
        public void GetCoProducerPaymentsTotals_WithData_ReturnsOk()
        {
            // Arrange

            // Act
            var result = _paymentsInfoController.GetCoProducerPaymentsTotals();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Use System.Text.Json to convert the anonymous object to a testable format
            var resultJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var deserializedResult = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(resultJson);

            Assert.NotNull(deserializedResult);
            Assert.Equal(2, deserializedResult.Count);

            // Verify co-producer 1 data using the JsonElement properties
            Assert.True(deserializedResult.ContainsKey("coproducer-1"));
            var coProducer1 = deserializedResult["coproducer-1"];

            Assert.Equal("Co-Producer 1", coProducer1.GetProperty("Name").GetString());
            Assert.Equal(150.0, coProducer1.GetProperty("ExpectedAmount").GetDouble());
            Assert.Equal(135.0, coProducer1.GetProperty("PaidAmount").GetDouble());
        }


        [Fact]
        public void GetCoProducerPaymentsTotals_NoData_ReturnsNotFound()
        {
            // Arrange
            MockPaymentsInfoFlag.IsCoProducerTotalsEmpty = true;

            // Act
            var result = _paymentsInfoController.GetCoProducerPaymentsTotals();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("No co-producer totals found.", notFoundResult.Value);
        }

        [Fact]
        public void GetProducerCC_WithData_ReturnsOk()
        {
            // Arrange

            // Act
            var result = _paymentsInfoController.GetProducerCC();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var mappedResult = Assert.IsAssignableFrom<Dictionary<string, PaymentInfoDto>>(okResult.Value);
            Assert.Equal(2, mappedResult.Count);

            // Verify first producer data
            var producer1 = mappedResult["producer-1"];
            Assert.Equal("Producer 1", producer1.User.Name);
            Assert.Single(producer1.Subscriptions);
            Assert.Single(producer1.Payments);
        }

        [Fact]
        public void GetProducerCC_NoData_ReturnsNotFound()
        {
            // Arrange
            MockPaymentsInfoFlag.IsProducerCCEmpty = true;

            // Act
            var result = _paymentsInfoController.GetProducerCC();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("No producer data found.", notFoundResult.Value);
        }

        [Fact]
        public void GetCoProducerCC_WithData_ReturnsOk()
        {
            // Arrange

            // Act
            var result = _paymentsInfoController.GetCoProducerCC();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var mappedResult = Assert.IsAssignableFrom<Dictionary<string, PaymentInfoDto>>(okResult.Value);
            Assert.Equal(2, mappedResult.Count);

            // Verify first co-producer data
            var coProducer1 = mappedResult["coproducer-1"];
            Assert.Equal("Co-Producer 1", coProducer1.User.Name);
            Assert.Equal(2, coProducer1.Subscriptions.Count());
            Assert.Equal(3, coProducer1.Payments.Count());
        }

        [Fact]
        public void GetCoProducerCC_NoData_ReturnsNotFound()
        {
            // Arrange
            MockPaymentsInfoFlag.IsCoProducerCCEmpty = true;

            // Act
            var result = _paymentsInfoController.GetCoProducerCC();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("No co-producer data found.", notFoundResult.Value);
        }
    }
}
