using AMAP.Entities.Models.DTO.PaymentInfo;
using System.Net;
using System.Net.Http.Json;

namespace AMAP.IntegrationTests.Services;

public class PaymentsInfoControllerTests : IntegrationTestBase
{
    //[Fact]
    //public async Task GetCoProducerPayments_AuthenticatedManager_ReturnsSuccessStatusCode()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testmanager-id", "Manager");

    //    // Act
    //    var response = await client.GetAsync("/api/PaymentsInfo/GetCoProducerPayments");

    //    // Assert
    //    Assert.True(
    //        response.StatusCode == HttpStatusCode.OK ||
    //        response.StatusCode == HttpStatusCode.NotFound,
    //        $"Expected OK or NotFound, got {response.StatusCode}"
    //    );

    //    if (response.StatusCode == HttpStatusCode.OK)
    //    {
    //        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, PaymentInfoDto>>();
    //        Assert.NotNull(result);
    //    }
    //}

    [Fact]
    public async Task GetCoProducerPayments_NonManagerRole_ReturnsForbidden()
    {
        // Arrange
        var client = GetAuthenticatedClient("testproducer-id", "Producer"); // Producer role, not Manager

        // Act
        var response = await client.GetAsync("/api/PaymentsInfo/GetCoProducerPayments");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected Forbidden or Unauthorized, got {response.StatusCode}"
        );
    }

    //[Fact]
    //public async Task GetProducerPayments_AuthenticatedManager_ReturnsSuccessStatusCode()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testmanager-id", "Manager");

    //    // Act
    //    var response = await client.GetAsync("/api/PaymentsInfo/GetProducerPayments");

    //    // Assert
    //    Assert.True(
    //        response.StatusCode == HttpStatusCode.OK ||
    //        response.StatusCode == HttpStatusCode.NotFound,
    //        $"Expected OK or NotFound, got {response.StatusCode}"
    //    );

    //    if (response.StatusCode == HttpStatusCode.OK)
    //    {
    //        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, PaymentInfoDto>>();
    //        Assert.NotNull(result);
    //    }
    //}

    [Fact]
    public async Task GetProducerPayments_NonManagerRole_ReturnsForbidden()
    {
        // Arrange
        var client = GetAuthenticatedClient("testcoproducer-id", "CoProducer"); // CoProducer role, not Manager

        // Act
        var response = await client.GetAsync("/api/PaymentsInfo/GetProducerPayments");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    //[Fact]
    //public async Task GetProducerToReceive_AuthenticatedManager_ReturnsSuccessStatusCode()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testmanager-id", "Manager");

    //    // Act
    //    var response = await client.GetAsync("/api/PaymentsInfo/GetProducerToReceive");

    //    // Assert
    //    Assert.True(
    //        response.StatusCode == HttpStatusCode.OK ||
    //        response.StatusCode == HttpStatusCode.NotFound,
    //        $"Expected OK or NotFound, got {response.StatusCode}"
    //    );

    //    if (response.StatusCode == HttpStatusCode.OK)
    //    {
    //        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, double>>();
    //        Assert.NotNull(result);
    //    }
    //}

    //[Fact]
    //public async Task GetCoProducerToPay_AuthenticatedManager_ReturnsSuccessStatusCode()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testmanager-id", "Manager");

    //    // Act
    //    var response = await client.GetAsync("/api/PaymentsInfo/GetCoProducerToPay");

    //    // Assert
    //    Assert.True(
    //        response.StatusCode == HttpStatusCode.OK ||
    //        response.StatusCode == HttpStatusCode.NotFound,
    //        $"Expected OK or NotFound, got {response.StatusCode}"
    //    );

    //    if (response.StatusCode == HttpStatusCode.OK)
    //    {
    //        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, double>>();
    //        Assert.NotNull(result);
    //    }
    //}

    //[Fact]
    //public async Task GetProducerPaymentsTotals_AuthenticatedManager_ReturnsSuccessStatusCode()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testmanager-id", "Manager");

    //    // Act
    //    var response = await client.GetAsync("/api/PaymentsInfo/GetProducerTotals");

    //    // Assert
    //    Assert.True(
    //        response.StatusCode == HttpStatusCode.OK ||
    //        response.StatusCode == HttpStatusCode.NotFound,
    //        $"Expected OK or NotFound, got {response.StatusCode}"
    //    );

    //    if (response.StatusCode == HttpStatusCode.OK)
    //    {
    //        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
    //        Assert.NotNull(result);
    //    }
    //}

    //[Fact]
    //public async Task GetCoProducerPaymentsTotals_AuthenticatedManager_ReturnsSuccessStatusCode()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testmanager-id", "Manager");

    //    // Act
    //    var response = await client.GetAsync("/api/PaymentsInfo/GetCoProducerTotals");

    //    // Assert
    //    Assert.True(
    //        response.StatusCode == HttpStatusCode.OK ||
    //        response.StatusCode == HttpStatusCode.NotFound,
    //        $"Expected OK or NotFound, got {response.StatusCode}"
    //    );

    //    if (response.StatusCode == HttpStatusCode.OK)
    //    {
    //        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
    //        Assert.NotNull(result);
    //    }
    //}

    /*[Fact]
    public async Task GetProducerCC_AuthenticatedManager_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = GetAuthenticatedClient("testmanager-id", "Manager");

        // Act
        var response = await client.GetAsync("/api/PaymentsInfo/GetProducerCC");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound, got {response.StatusCode}"
        );

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, PaymentInfoDto>>();
            Assert.NotNull(result);
        }
    }*/

    //[Fact]
    //public async Task GetCoProducerCC_AuthenticatedManager_ReturnsSuccessStatusCode()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testmanager-id", "Manager");

    //    // Act
    //    var response = await client.GetAsync("/api/PaymentsInfo/GetCoProducerCC");

    //    // Assert
    //    Assert.True(
    //        response.StatusCode == HttpStatusCode.OK ||
    //        response.StatusCode == HttpStatusCode.NotFound,
    //        $"Expected OK or NotFound, got {response.StatusCode}"
    //    );

    //    if (response.StatusCode == HttpStatusCode.OK)
    //    {
    //        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, PaymentInfoDto>>();
    //        Assert.NotNull(result);

    //        // Verify PaymentInfoDto structure
    //        foreach (var item in result.Values)
    //        {
    //            Assert.NotNull(item.User);
    //            Assert.NotNull(item.Subscriptions);
    //            Assert.NotNull(item.Payments);
    //        }
    //    }
    //}

    [Fact]
    public async Task GetCoProducerCC_NonManagerRole_ReturnsForbidden()
    {
        // Arrange
        var client = GetAuthenticatedClient("testproducer-id", "Producer"); // Producer role, not Manager

        // Act
        var response = await client.GetAsync("/api/PaymentsInfo/GetCoProducerCC");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Additional tests that simulate empty results
    //[Fact]
    //public async Task GetCoProducerPayments_EmptyResults_ReturnsNotFound()
    //{
    //    // This test assumes there's a way to ensure no co-producer payments exist
    //    // In a real test, you might need to manipulate test data or mock the service

    //    // Arrange
    //    var client = GetAuthenticatedClient("testmanager-id", "Manager");
    //    // For this test, we assume the test database is in a state where no co-producer payments exist

    //    // Act
    //    var response = await client.GetAsync("/api/PaymentsInfo/GetCoProducerPayments");

    //    // Assert
    //    // Since we can't control the test database state in this template, we check for either success or not found
    //    Assert.True(
    //        response.StatusCode == HttpStatusCode.OK ||
    //        response.StatusCode == HttpStatusCode.NotFound,
    //        $"Expected OK or NotFound, got {response.StatusCode}"
    //    );
    //}

    /*[Fact]
    public async Task GetProducerPayments_EmptyResults_ReturnsNotFound()
    {
        // Similar to the previous test, this assumes a way to ensure no producer payments exist

        // Arrange
        var client = GetAuthenticatedClient("testmanager-id", "Manager");

        // Act
        var response = await client.GetAsync("/api/PaymentsInfo/GetProducerPayments");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound, got {response.StatusCode}"
        );
    }*/
}
