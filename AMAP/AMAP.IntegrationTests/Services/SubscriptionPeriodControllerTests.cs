using AMAP.Entities;
using AMAP.Entities.Models.DTO.SubscriptionPeriod;
using System.Net;
using System.Net.Http.Json;

namespace AMAP.IntegrationTests.Services;

public class SubscriptionPeriodControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task GetSubscriptionPeriodById_ExistingPeriod_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _client;

        // Act
        var response = await client.GetAsync("/api/SubscriptionPeriod/1");

        // Assert
        response.EnsureSuccessStatusCode();
        var subscriptionPeriodDto = await response.Content.ReadFromJsonAsync<SubscriptionPeriodDto>();
        Assert.NotNull(subscriptionPeriodDto);
        Assert.Equal("Monthly", subscriptionPeriodDto.Duration);
    }

    [Fact]
    public async Task GetSubscriptionPeriodById_NonExistingPeriod_ReturnsNotFound()
    {
        // Arrange
        var client = _client;

        // Act
        var response = await client.GetAsync("/api/SubscriptionPeriod/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDeliveryDateById_ExistingDate_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _client;
        var response = await client.GetAsync("/api/SubscriptionPeriod/DeliveryDate/1");
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        var deliveryDateDto = await response.Content.ReadFromJsonAsync<DeliveryDateDto>();
        Assert.NotNull(deliveryDateDto);
    }

    [Fact]
    public async Task GetDeliveryDateById_NonExistingDate_ReturnsNotFound()
    {
        // Arrange
        var client = _client;

        // Act
        var response = await client.GetAsync("/api/SubscriptionPeriod/DeliveryDate/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    //[Fact]
    //public async Task CreateSubscriptionPeriod_AuthenticatedManager_ReturnsCreated()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testproducer-id", "Manager");
    //    var deliveryDate = DateTime.Now.AddDays(30);
        
    //    var newSubscriptionPeriod = new SubscriptionPeriod
    //    {
    //        Duration = "Fall 2023",
    //        StartDate = DateTime.Now.AddDays(10),
    //        EndDate = DateTime.Now.AddDays(100),
    //        DeliveryDates = new List<DeliveryDate>
    //        {
    //            new DeliveryDate { Date = deliveryDate }
    //        }
    //    };

    //    // Act
    //    var response = await client.PostAsJsonAsync("/api/SubscriptionPeriod", newSubscriptionPeriod);

    //    // Assert
    //    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    //    var createdPeriod = await response.Content.ReadFromJsonAsync<SubscriptionPeriod>();
    //    Assert.NotNull(createdPeriod);
    //    Assert.Equal("Fall 2023", createdPeriod.Duration);
    //    Assert.NotEmpty(createdPeriod.DeliveryDates);
    //}

    [Fact]
    public async Task CreateSubscriptionPeriod_NonManagerRole_ReturnsForbidden()
    {
        // Arrange
        // Use a client with Producer role explicitly
        var client = GetAuthenticatedClient("testproducer-id", "Producer");

        // Verify the client is authenticated with Producer role
        Console.WriteLine("Testing with Producer role user");

        var newSubscriptionPeriod = new SubscriptionPeriod
        {
            Duration = "Spring 2024",
            StartDate = DateTime.Now.AddDays(220),
            EndDate = DateTime.Now.AddDays(310),
            DeliveryDates = new List<DeliveryDate>
        {
            new DeliveryDate { Date = DateTime.Now.AddDays(250) }
        }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/SubscriptionPeriod", newSubscriptionPeriod);

        // Log response for debugging
        Console.WriteLine($"Response status for non-manager role test: {response.StatusCode}");
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response content: {responseContent}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    //[Fact]
    //public async Task CreateSubscriptionPeriod_InvalidData_ReturnsBadRequest()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testmanager-id", "Manager");

    //    // Use more clearly invalid data
    //    var invalidSubscriptionPeriod = new SubscriptionPeriod
    //    {
    //        // Explicitly set Duration to null to ensure it fails validation
    //        Duration = null,
    //        StartDate = DateTime.Now.AddDays(10),
    //        EndDate = DateTime.Now.AddDays(5), // End date before start date
    //        DeliveryDates = new List<DeliveryDate>()
    //    };

    //    // Act
    //    Console.WriteLine("Sending invalid subscription period:");
    //    Console.WriteLine($"  Duration: {(invalidSubscriptionPeriod.Duration == null ? "null" : invalidSubscriptionPeriod.Duration)}");
    //    Console.WriteLine($"  StartDate: {invalidSubscriptionPeriod.StartDate}");
    //    Console.WriteLine($"  EndDate: {invalidSubscriptionPeriod.EndDate}");
    //    Console.WriteLine($"  DeliveryDates count: {invalidSubscriptionPeriod.DeliveryDates?.Count() ?? 0}");

    //    var response = await client.PostAsJsonAsync("/api/SubscriptionPeriod", invalidSubscriptionPeriod);

    //    // Log actual response for debugging
    //    Console.WriteLine($"Response status code: {response.StatusCode} ({(int)response.StatusCode})");
    //    var responseContent = await response.Content.ReadAsStringAsync();
    //    Console.WriteLine($"Response content: {responseContent}");

    //    // Assert - accept either BadRequest (400) or UnprocessableEntity (422) 
    //    // Both are appropriate for validation errors in REST APIs
    //    Assert.True(
    //        response.StatusCode == HttpStatusCode.BadRequest ||
    //        response.StatusCode == HttpStatusCode.UnprocessableEntity,
    //        $"Expected BadRequest (400) or UnprocessableEntity (422), but got {response.StatusCode} ({(int)response.StatusCode})");
    //}

    [Fact]
    public async Task DeleteSubscriptionPeriod_NonExistingPeriod_ReturnsNotFound()
    {
        // Arrange
        var client = GetAuthenticatedClient("testproducer-id", "Manager");

        // Act
        var response = await client.DeleteAsync("/api/SubscriptionPeriod/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    //[Fact]
    //public async Task DeleteSubscriptionPeriod_NonManagerRole_ReturnsForbidden()
    //{
    //    // Arrange
    //    // Use a client with Producer role explicitly
    //    var client = GetAuthenticatedClient("testproducer-id", "Producer");

    //    // Verify the client is authenticated with Producer role
    //    Console.WriteLine("Testing delete with Producer role user");

    //    // Act
    //    var response = await client.DeleteAsync("/api/SubscriptionPeriod/1");

    //    // Log response for debugging
    //    Console.WriteLine($"Response status for non-manager delete: {response.StatusCode}");
    //    var responseContent = await response.Content.ReadAsStringAsync();
    //    Console.WriteLine($"Response content: {responseContent}");

    //    // Assert
    //    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    //}
}