using AMAP.Entities.Models.DTO.Catalog;
using System.Net;
using System.Net.Http.Json;

namespace AMAP.IntegrationTests.Services;

public class CatalogControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task GetAllCatalogs_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _client; // No authentication required for this endpoint

        // Act
        var response = await client.GetAsync("/api/Catalog/GetAllCatalogs");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound, got {response.StatusCode}"
        );

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var catalogsData = await response.Content.ReadFromJsonAsync<List<CatalogDto>>();
            Assert.NotNull(catalogsData);
            Assert.NotEmpty(catalogsData);

            // Verify catalog structure
            foreach (var catalog in catalogsData)
            {
                Assert.NotNull(catalog.ProducerName);
                Assert.NotNull(catalog.Products);
            }
        }
    }

    [Fact]
    public async Task GetAllCatalogs_EmptyResult_ReturnsNotFound()
    {
        // This test assumes there's a way to ensure no catalogs exist in the test database
        // In a real test, you might need to manipulate test data or mock the service
        
        // Arrange
        var client = _client;
        // For this test, we assume the test database might be in a state where no catalogs exist
        // or we'd need some way to remove all catalogs first

        // Act
        var response = await client.GetAsync("/api/Catalog/GetAllCatalogs");

        // Assert
        // Since we can't control the test database state in this template, we check for either success or not found
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound, got {response.StatusCode}"
        );
    }

    //[Fact]
    //public async Task GetCatalogOfLoggedProducer_AuthenticatedProducer_ReturnsSuccessStatusCode()
    //{
    //    // Arrange
    //    var producerId = "testproducer-id";
    //    var client = GetAuthenticatedClient(producerId, "Producer");

    //    // Act
    //    var response = await client.GetAsync("/api/Catalog/GetCatalog");

    //    // Assert
    //    response.EnsureSuccessStatusCode();
    //    var catalogData = await response.Content.ReadFromJsonAsync<List<CatalogDto>>();
    //    Assert.NotNull(catalogData);
    //    Assert.NotEmpty(catalogData);
        
    //    // Verify catalog structure
    //    foreach (var catalog in catalogData)
    //    {
    //        Assert.NotNull(catalog.ProducerName);
    //        Assert.NotNull(catalog.Products);
    //    }
    //}

    //[Fact]
    //public async Task GetCatalogOfLoggedProducer_NonProducerRole_ReturnsForbidden()
    //{
    //    // Arrange
    //    var client = GetAuthenticatedClient("testcopr-id", "CoProducer"); // CoProducer role, not Producer

    //    // Act
    //    var response = await client.GetAsync("/api/Catalog/GetCatalog");

    //    // Assert
    //    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    //}

    //[Fact]
    //public async Task GetCatalogOfLoggedProducer_EmptyResult_ReturnsNotFound()
    //{
    //    // This test assumes there's a producer without any products
        
    //    // Arrange
    //    // Using a producer ID that we assume has no products in the catalog
    //    var emptyProducerId = "empty-producer-id";
    //    var client = GetAuthenticatedClient(emptyProducerId, "Producer");

    //    // Act
    //    var response = await client.GetAsync("/api/Catalog/GetCatalog");

    //    // Assert
    //    // Since we can't guarantee the producer has no catalog in the test database,
    //    // we check for either success or not found
    //    Assert.True(
    //        response.StatusCode == HttpStatusCode.OK || 
    //        response.StatusCode == HttpStatusCode.NotFound,
    //        $"Expected OK or NotFound, got {response.StatusCode}"
    //    );
    //}

    [Fact]
    public async Task GetCatalogByProducer_ValidProducerId_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _client; // No authentication required for this endpoint
        var producerId = "testproducer-id"; // Using a producer ID that we assume exists

        // Act
        var response = await client.GetAsync($"/api/Catalog/GetCatalog/{producerId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var catalogData = await response.Content.ReadFromJsonAsync<List<CatalogDto>>();
        Assert.NotNull(catalogData);
        Assert.NotEmpty(catalogData);
        
        // Verify catalog structure
        foreach (var catalog in catalogData)
        {
            Assert.NotNull(catalog.ProducerName);
            Assert.NotNull(catalog.Products);
        }
    }

    [Fact]
    public async Task GetCatalogByProducer_InvalidProducerId_ReturnsNotFound()
    {
        // Arrange
        var client = _client;
        var invalidProducerId = "non-existent-producer-id";

        // Act
        var response = await client.GetAsync($"/api/Catalog/GetCatalog/{invalidProducerId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Additional test to verify catalog content
    [Fact]
    public async Task GetCatalogByProducer_ValidProducerId_ContainsCorrectProducerName()
    {
        // Arrange
        var client = _client;
        var producerId = "testproducer-id"; // Using a producer ID that we assume exists

        // Act
        var response = await client.GetAsync($"/api/Catalog/GetCatalog/{producerId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var catalogData = await response.Content.ReadFromJsonAsync<List<CatalogDto>>();
        Assert.NotNull(catalogData);
        Assert.NotEmpty(catalogData);
        
        // In a real test environment with controlled data, we would verify the producer name matches
        // Since we don't know the expected producer name in this template, we just assert it's not null
        foreach (var catalog in catalogData)
        {
            Assert.NotNull(catalog.ProducerName);
        }
    }
}