using System.Net;
using System.Text;
using System.Text.Json;

namespace AMAP.IntegrationTests.Services;

public class DeliveryControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateDeliveredQuantity_NonProducerRole_ReturnsForbidden()
    {
        // Arrange
        var client = GetAuthenticatedClient("testcopr-id", "CoProducer"); // CoProducer role, not Producer
        int deliveredQuantity = 5;

        // Act
        var response = await client.PutAsync(
            "/api/Delivery/UpdateDeliveredQuantity/1", 
            new StringContent(JsonSerializer.Serialize(deliveredQuantity), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDeliveredQuantity_NonExistingSubscription_ReturnsNotFound()
    {
        // Arrange
        var client = GetAuthenticatedClient("testproducer-id", "Producer");
        int deliveredQuantity = 5;
        int nonExistingSubscriptionId = 9999;

        // Act
        var response = await client.PutAsync(
            $"/api/Delivery/UpdateDeliveredQuantity/{nonExistingSubscriptionId}", 
            new StringContent(JsonSerializer.Serialize(deliveredQuantity), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}