using AMAP.Api.Controllers;
using AMAP.Business.Interfaces;
using AMAP.Entities.Models.DTO.Catalog;
using AMAP.UnitTests.Business.Mock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AMAP.UnitTests.Services
{
    public class CatalogControllerTests
    {
        private readonly ICatalogBc _catalogBc;
        private readonly CatalogController _catalogController;

        public CatalogControllerTests()
        {
            _catalogBc = MockCatalogBc.GetMockCatalogBc();
            _catalogController = new CatalogController(_catalogBc);

            // Reset the mock flag before each test
            MockIsEmptyFlag.IsGetAllCatalogsEmpty = false;
        }

        private void SetupControllerWithUserClaims(string userId = "test-producer-id", string role = "Producer")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _catalogController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetAllCatalogs_WithData_ReturnsOk()
        {
            // Arrange

            // Act
            var result = await _catalogController.GetAllCatalogs();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var catalogs = Assert.IsAssignableFrom<List<CatalogDto>>(okResult.Value);
            Assert.Equal(2, catalogs.Count);
        }

        [Fact]
        public async Task GetAllCatalogs_NoData_ReturnsNotFound()
        {
            // Arrange
            MockIsEmptyFlag.IsGetAllCatalogsEmpty = true;

            // Act
            var result = await _catalogController.GetAllCatalogs();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("No producers found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetCatalogOfLoggedProducer_LoggedIn_ReturnsOk()
        {
            // Arrange
            SetupControllerWithUserClaims();

            // Act
            var result = await _catalogController.GetCatalogOfLoggedProducer();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var catalogs = Assert.IsAssignableFrom<List<CatalogDto>>(okResult.Value);
            Assert.Single(catalogs);
            Assert.Equal("Test Producer", catalogs[0].ProducerName);
        }

        [Fact]
        public async Task GetCatalogOfLoggedProducer_NotLoggedIn_ReturnsNotFound()
        {
            // Arrange
            // Set up empty claims principal (no claims) to simulate not logged in
            _catalogController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = await _catalogController.GetCatalogOfLoggedProducer();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Producer not logged in.", notFoundResult.Value);
        }


        [Fact]
        public async Task GetCatalogOfLoggedProducer_NoProducts_ReturnsNotFound()
        {
            // Arrange
            SetupControllerWithUserClaims("non-existing-producer-id");

            // Act
            var result = await _catalogController.GetCatalogOfLoggedProducer();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Producer not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetCatalogByProducer_ExistingProducer_ReturnsOk()
        {
            // Arrange
            var producerId = "test-producer-id";

            // Act
            var result = await _catalogController.GetCatalogByProducer(producerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var catalogs = Assert.IsAssignableFrom<List<CatalogDto>>(okResult.Value);
            Assert.Single(catalogs);
            Assert.Equal("Test Producer", catalogs[0].ProducerName);
        }

        [Fact]
        public async Task GetCatalogByProducer_NonExistingProducer_ReturnsNotFound()
        {
            // Arrange
            var producerId = "non-existing-producer-id";

            // Act
            var result = await _catalogController.GetCatalogByProducer(producerId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Producer not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetCatalogByProducer_ProducerWithManyProducts_ReturnsOk()
        {
            // Arrange
            var producerId = "producer-with-many-products";

            // Act
            var result = await _catalogController.GetCatalogByProducer(producerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var catalogs = Assert.IsAssignableFrom<List<CatalogDto>>(okResult.Value);
            Assert.Single(catalogs);
            Assert.Equal("Producer With Many Products", catalogs[0].ProducerName);
            Assert.Equal(5, catalogs[0].Products.Count);
        }
    }
}
