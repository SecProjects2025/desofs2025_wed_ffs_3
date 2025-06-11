using AMAP.Entities;
using System.Net;
using System.Net.Http.Json;

namespace AMAP.IntegrationTests.Services;

public class ProductControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task GetProduct_ExistingProduct_ReturnsSuccessStatusCode()
    {
        // Arrange
        // Make sure to use the producer ID, not email
        var client = GetAuthenticatedClient("testproducer-id", "Producer");

        // Act
        var response = await client.GetAsync("/api/Product/1");

        // Assert
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);
        Assert.Equal("Test Apples", product.Name);
    }

    [Fact]
    public async Task GetProduct_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        // Use authenticated client here too since the controller requires authentication
        var client = GetAuthenticatedClient("testproducer-id", "Producer");

        // Act
        var response = await client.GetAsync("/api/Product/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_AuthenticatedProducer_ReturnsCreated()
    {
        // Arrange
        // Use ID consistently, not email
        var client = GetAuthenticatedClient("testproducer-id", "Producer");
        var newProduct = new Product
        {
            Name = "Test Bananas",
            Description = "Fresh test bananas",
            ReferencePrice = 1.50,
            DeliveryUnits = "kg",
            Photo = "bananas.jpg"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Product", newProduct);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdProduct = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(createdProduct);
        Assert.Equal("Test Bananas", createdProduct.Name);
    }

    [Fact]
    public async Task UpdateProduct_AuthenticatedProducer_ReturnsNoContent()
    {
        // Arrange
        // Use ID consistently, not email
        var client = GetAuthenticatedClient("testproducer-id", "Producer");
        var updatedProduct = new Product
        {
            Name = "Updated Test Apples",
            Description = "Updated description",
            ReferencePrice = 2.75,
            DeliveryUnits = "kg",
            Photo = "apples.jpg"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/Product/1", updatedProduct);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the update
        var getResponse = await client.GetAsync("/api/Product/1");
        getResponse.EnsureSuccessStatusCode();
        var product = await getResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);
        Assert.Equal("Updated Test Apples", product.Name);
        Assert.Equal(2.75, product.ReferencePrice);
    }

    [Fact]
    public async Task DeleteProduct_AuthenticatedProducer_ReturnsNoContent()
    {
        // Arrange
        // Use ID consistently, not email
        var client = GetAuthenticatedClient("testproducer-id", "Producer");

        // Act
        var response = await client.DeleteAsync("/api/Product/2");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deletion
        var getResponse = await client.GetAsync("/api/Product/2");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}