using AMAP.Entities;
using AMAP.Entities.Models.DTO.ProductOffer;
using AMAP.Entities.Utilities;
using System.Net;
using System.Net.Http.Json;

namespace AMAP.IntegrationTests.Services;

public class ProductOfferControllerTests : IntegrationTestBase
{
    //[Fact]
    //public async Task CreateProductOffer_NonProducerRole_ReturnsForbidden()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testcoproducer-id", "CoProducer"); // CoProducer role, not Producer
    //    var newProductOffer = new ProductOffer
    //    {
    //        ProductId = 1,
    //        SubscriptionPeriodId = 1,
    //        Price = 15.99,
    //        Quantity = 5,
    //        DeliveryDate = DateTime.Now.AddDays(30),
    //        PaymentType = PaymentType.Money
    //    };

    //    // Act
    //    var response = await client.PostAsJsonAsync("/api/ProductOffer", newProductOffer);

    //    // Assert
    //    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    //}

    [Fact]
    public async Task CreateProductOffer_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var client = GetAuthenticatedClient("testproducer-id", "Producer");
        var invalidProductOffer = new ProductOffer
        {
            // Missing ProductId
            SubscriptionPeriodId = 1,
            Price = -10.99, // Negative price should be invalid
            Quantity = 5,
            DeliveryDate = DateTime.Now.AddDays(30),
            PaymentType = PaymentType.Money
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/ProductOffer", invalidProductOffer);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    //[Fact]
    //public async Task GetProductOfferById_ExistingOffer_ReturnsSuccessStatusCode()
    //{
    //    // Arrange
    //    var producerId = "testproducer-id";
    //    var createClient = GetAuthenticatedClient(producerId, "Producer");

    //    // Use a unique delivery date to avoid conflicts
    //    var deliveryDate = DateTime.Now.Date.AddDays(21); // Pick a date not used in seeding

    //    var newProductOffer = new ProductOffer
    //    {
    //        ProductId = 1,
    //        SubscriptionPeriodId = 1,
    //        Price = 19.99,
    //        Quantity = 15,
    //        DeliveryDate = deliveryDate,
    //        PaymentType = PaymentType.Money
    //    };

    //    // Post the product offer and validate the response
    //    var createResponse = await createClient.PostAsJsonAsync("/api/ProductOffer", newProductOffer);

    //    if (!createResponse.IsSuccessStatusCode)
    //    {
    //        var errorContent = await createResponse.Content.ReadAsStringAsync();
    //        Console.WriteLine($"Failed to create product offer. Status: {createResponse.StatusCode}, Content: {errorContent}");
    //        Assert.True(createResponse.IsSuccessStatusCode,
    //            $"Failed to create product offer. Status: {createResponse.StatusCode}, Content: {errorContent}");
    //    }

    //    var createdOffer = await createResponse.Content.ReadFromJsonAsync<ProductOffer>();

    //    // Act
    //    var response = await createClient.GetAsync($"/api/ProductOffer/{createdOffer.Id}");

    //    // Assert
    //    response.EnsureSuccessStatusCode();
    //    var productOfferDto = await response.Content.ReadFromJsonAsync<ProductOfferDto>();
    //    Assert.NotNull(productOfferDto);
    //    Assert.Equal(newProductOffer.ProductId, productOfferDto.ProductId);
    //    Assert.Equal(newProductOffer.SubscriptionPeriodId, productOfferDto.SubscriptionPeriodId);
    //    Assert.Equal(newProductOffer.Price, productOfferDto.Price);
    //    Assert.Equal(newProductOffer.Quantity, productOfferDto.Quantity);
    //    Assert.Equal(newProductOffer.PaymentType, productOfferDto.PaymentType);
    //}

    //[Fact]
    //public async Task GetProductOfferById_NonExistingOffer_ReturnsNotFound()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testproducer-id", "Producer");

    //    // Act
    //    var response = await client.GetAsync("/api/ProductOffer/999");

    //    // Assert
    //    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    //}

    [Fact]
    public async Task GetProductOfferByProducer_ProducerWithNoOffers_ReturnsNotFound()
    {
        // Arrange
        var emptyProducerId = "empty-producer-id";
        var client = GetAuthenticatedClient(emptyProducerId, "Producer");
        
        // Act
        var response = await client.GetAsync($"/api/ProductOffer/producer/{emptyProducerId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    //[Fact]
    //public async Task DeleteProductOffer_NonExistingOffer_ReturnsNotFound()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testproducer-id", "Producer");

    //    // Use a non-existent ID
    //    int nonExistentId = 999;

    //    // Act
    //    var response = await client.DeleteAsync($"/api/ProductOffer/{nonExistentId}");

    //    // Log for debugging
    //    Console.WriteLine($"Delete non-existing offer status: {response.StatusCode}");
    //    string content = await response.Content.ReadAsStringAsync();
    //    Console.WriteLine($"Response content: {content}");

    //    // Assert
    //    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    //}

    //[Fact]
    //public async Task DeleteProductOffer_NonProducerRole_ReturnsForbidden()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testcoproducer-id", "CoProducer"); // CoProducer role, not Producer
        
    //    // Act
    //    var response = await client.DeleteAsync("/api/ProductOffer/1");
        
    //    // Assert
    //    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    //}
}