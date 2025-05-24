using AMAP.Business.Components;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.UnitTests.Repository.Mock;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace AMAP.UnitTests.Business
{
    public class CatalogBcTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly CatalogBc _catalogBc;

        public CatalogBcTests()
        {
            // Arrange - Setup mock dependencies
            var (unitOfWork, userManager) = MockCatalogRepository.GetMockDependencies();
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _catalogBc = new CatalogBc(_unitOfWork, _userManager);
        }

        [Fact]
        public async Task GetAllCatalogs_WithProducersAndProducts_ReturnsValidCatalogs()
        {
            // Arrange
            // Setup is done in the constructor

            // Act
            var result = await _catalogBc.GetAllCatalogs();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Two producers with products

            // Verify the first catalog (Producer One)
            var producer1Catalog = result.FirstOrDefault(c => c.ProducerName == "Producer One");
            Assert.NotNull(producer1Catalog);
            Assert.Equal(2, producer1Catalog.Products.Count); // Two non-deleted products

            // Verify the simple product
            var simpleProduct = producer1Catalog.Products.FirstOrDefault(p => p.Name == "Carrots");
            Assert.NotNull(simpleProduct);
            Assert.Equal("Simple", simpleProduct.Type);
            Assert.Empty(simpleProduct.Composites);
            Assert.Equal(2.50, simpleProduct.ReferencePrice);
            Assert.Equal("kg", simpleProduct.DeliveryUnits);

            // Verify the composite product
            var compositeProduct = producer1Catalog.Products.FirstOrDefault(p => p.Name == "Fruit Basket");
            Assert.NotNull(compositeProduct);
            Assert.Equal("Composite", compositeProduct.Type);
            Assert.Equal(2, compositeProduct.Composites.Count());
            Assert.Contains(compositeProduct.Composites, c => c.SubProductName == "Apples" && c.Quantity == 2);
            Assert.Contains(compositeProduct.Composites, c => c.SubProductName == "Oranges" && c.Quantity == 3);

            // Verify the second catalog (Producer Two)
            var producer2Catalog = result.FirstOrDefault(c => c.ProducerName == "Producer Two");
            Assert.NotNull(producer2Catalog);
            Assert.Equal(1, producer2Catalog.Products.Count); // One non-deleted product
            Assert.Equal("Potatoes", producer2Catalog.Products[0].Name);
        }

        [Fact]
        public async Task GetAllCatalogs_WithNoProducers_ReturnsEmptyList()
        {
            // Arrange
            var emptyUserManager = Substitute.For<UserManager<AppUser>>(
                Substitute.For<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);
            emptyUserManager.Users.Returns(new List<AppUser>().AsQueryable());

            var catalogBcWithNoProducers = new CatalogBc(_unitOfWork, emptyUserManager);

            // Act
            var result = await catalogBcWithNoProducers.GetAllCatalogs();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCatalogs_ProducerWithNoProducts_ExcludesFromResult()
        {
            // Arrange
            // Mock a producer with no products
            var mockUserManager = Substitute.For<UserManager<AppUser>>(
                Substitute.For<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

            var users = new List<AppUser>
            {
                new AppUser
                {
                    Id = "producer-3",
                    UserName = "producer3@example.com",
                    Name = "Producer Three",
                    UserType = Entities.Utilities.UserType.Producer
                }
            };

            mockUserManager.Users.Returns(users.AsQueryable());

            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            var mockProductRepo = Substitute.For<IProductRepository>();
            mockProductRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(), Arg.Any<string>())
                .Returns(new List<Product>());
            mockUnitOfWork.Product.Returns(mockProductRepo);

            var catalogBcWithEmptyProducts = new CatalogBc(mockUnitOfWork, mockUserManager);

            // Act
            var result = await catalogBcWithEmptyProducts.GetAllCatalogs();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // No catalogs returned because producer has no products
        }

        [Fact]
        public async Task GetCatalogByProducer_ExistingProducer_ReturnsCatalog()
        {
            // Arrange
            var producerId = "producer-1";

            // Act
            var result = await _catalogBc.GetCatalogByProducer(producerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // One catalog in the list

            var catalog = result[0];
            Assert.Equal("Producer One", catalog.ProducerName);
            Assert.Equal(2, catalog.Products.Count); // Two non-deleted products

            // Verify the simple product
            var simpleProduct = catalog.Products.FirstOrDefault(p => p.Name == "Carrots");
            Assert.NotNull(simpleProduct);
            Assert.Equal("Simple", simpleProduct.Type);

            // Verify the composite product
            var compositeProduct = catalog.Products.FirstOrDefault(p => p.Name == "Fruit Basket");
            Assert.NotNull(compositeProduct);
            Assert.Equal("Composite", compositeProduct.Type);
            Assert.Equal(2, compositeProduct.Composites.Count());
        }

        [Fact]
        public async Task GetCatalogByProducer_NonExistingProducer_ReturnsEmptyList()
        {
            // Arrange
            var nonExistingProducerId = "non-existing-producer";

            // Act
            var result = await _catalogBc.GetCatalogByProducer(nonExistingProducerId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCatalogByProducer_ProducerWithNoProducts_ReturnsEmptyCatalog()
        {
            // Arrange
            var producerId = "producer-2";

            // Mock a producer with no products
            var mockUserManager = _userManager;

            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            var mockProductRepo = Substitute.For<IProductRepository>();
            mockProductRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(), Arg.Any<string>())
                .Returns(new List<Product>());
            mockUnitOfWork.Product.Returns(mockProductRepo);

            var catalogBcWithEmptyProducts = new CatalogBc(mockUnitOfWork, mockUserManager);

            // Act
            var result = await catalogBcWithEmptyProducts.GetCatalogByProducer(producerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // One catalog with no products
            Assert.Equal("Producer Two", result[0].ProducerName);
            Assert.Empty(result[0].Products);
        }

        [Fact]
        public async Task GetCatalogByProducer_FilteredDeletedProducts_ExcludesDeletedProducts()
        {
            // Arrange
            var producerId = "producer-1"; // This producer has a deleted product

            // Act
            var result = await _catalogBc.GetCatalogByProducer(producerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var catalog = result[0];

            // Verify no deleted products are included
            Assert.Equal(2, catalog.Products.Count); // Only 2 non-deleted products
            Assert.DoesNotContain(catalog.Products, p => p.Name == "Deleted Product");
        }

        [Fact]
        public async Task GetCatalogByProducer_CompositeProducts_IncludesSubProductDetails()
        {
            // Arrange
            var producerId = "producer-1"; // Has composite product "Fruit Basket"

            // Act
            var result = await _catalogBc.GetCatalogByProducer(producerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);

            var compositeProduct = result[0].Products.FirstOrDefault(p => p.Name == "Fruit Basket");
            Assert.NotNull(compositeProduct);
            Assert.Equal("Composite", compositeProduct.Type);

            // Verify composite details
            Assert.Equal(2, compositeProduct.Composites.Count());
            var applePart = compositeProduct.Composites.FirstOrDefault(c => c.SubProductName == "Apples");
            Assert.NotNull(applePart);
            Assert.Equal(2, applePart.Quantity);

            var orangePart = compositeProduct.Composites.FirstOrDefault(c => c.SubProductName == "Oranges");
            Assert.NotNull(orangePart);
            Assert.Equal(3, orangePart.Quantity);
        }
    }
}
