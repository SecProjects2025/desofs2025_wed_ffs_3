using AMAP.Business;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.UnitTests.Business.Mock;
using NSubstitute;

namespace AMAP.UnitTests.Business
{
    public class ProductBcTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ProductBc _productBc;

        public ProductBcTests()
        {
            // Setup mock repository
            _unitOfWork = MockProductRepository.GetMockUnitOfWork();
            _productBc = new ProductBc(_unitOfWork);
        }

        [Fact]
        public void GetProduct_ValidId_ReturnsProduct()
        {
            // Arrange
            var productId = 1;

            // Act
            var result = _productBc.GetProduct(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Carrots", result.Name);
            Assert.Equal("producer-1", result.ProducerId);
            Assert.Equal(2.50, result.ReferencePrice);
            Assert.False(result.IsDeleted);
        }

        [Fact]
        public void GetProduct_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var invalidId = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _productBc.GetProduct(invalidId));
            Assert.Equal("Product ID must be greater than zero. (Parameter 'id')", exception.Message);
        }

        [Fact]
        public void GetProduct_NonExistingId_ReturnsNull()
        {
            // Arrange
            var nonExistingId = 100;

            // Act
            var result = _productBc.GetProduct(nonExistingId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetProduct_DeletedProduct_ReturnsNull()
        {
            // Arrange
            var deletedProductId = 4;

            // Act
            var result = _productBc.GetProduct(deletedProductId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetProduct_CompositeProduct_ReturnsProductWithComposites()
        {
            // Arrange
            var compositeProductId = 2;

            // Act
            var result = _productBc.GetProduct(compositeProductId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(compositeProductId, result.Id);
            Assert.Equal("Vegetable Box", result.Name);

            // Check composites
            Assert.NotNull(result.Composites);
            Assert.Single(result.Composites);

            var composite = result.Composites.First();
            Assert.Equal(1, composite.SubProductId);
            Assert.Equal(2, composite.Quantity);

            // Check that SubProduct is included
            Assert.NotNull(composite.SubProduct);
            Assert.Equal("Carrots", composite.SubProduct.Name);
        }

        [Fact]
        public void CreateProduct_ValidProduct_ReturnsCreatedProduct()
        {
            // Arrange
            var newProduct = new Product
            {
                Name = "New Test Product",
                Description = "A new test product",
                Photo = "newproduct.jpg",
                ReferencePrice = 9.99,
                DeliveryUnits = "box"
            };
            var producerId = "producer-1";

            // Act
            var result = _productBc.CreateProduct(newProduct, producerId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(0, result.Id);
            Assert.Equal("New Test Product", result.Name);
            Assert.Equal("producer-1", result.ProducerId);
            Assert.Equal(9.99, result.ReferencePrice);
            Assert.False(result.IsDeleted);

            // Verify the save was called
            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public void CreateProduct_NullProduct_ThrowsArgumentException()
        {
            // Arrange
            Product nullProduct = null;
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _productBc.CreateProduct(nullProduct, producerId));
            Assert.Equal("Invalid product data. (Parameter 'product')", exception.Message);
        }

        [Fact]
        public void CreateProduct_EmptyProducerId_ThrowsArgumentException()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "A test product",
                Photo = "test.jpg",
                ReferencePrice = 5.99,
                DeliveryUnits = "kg"
            };
            string producerId = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _productBc.CreateProduct(product, producerId));
            Assert.Equal("Producer not authenticated. (Parameter 'producerId')", exception.Message);
        }

        [Fact]
        public void CreateProduct_DuplicateName_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingProductName = "Carrots"; // Already exists for producer-1
            var product = new Product
            {
                Name = existingProductName,
                Description = "Duplicate product",
                Photo = "duplicate.jpg",
                ReferencePrice = 3.99,
                DeliveryUnits = "kg"
            };
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _productBc.CreateProduct(product, producerId));
            Assert.Equal("You already have a product with the same name.", exception.Message);
        }

        [Fact]
        public void CreateProduct_WithComposites_AddsProductAndComposites()
        {
            // Arrange
            var product = new Product
            {
                Name = "Fruit Box",
                Description = "Mixed fruit box",
                Photo = "fruitbox.jpg",
                ReferencePrice = 12.99,
                DeliveryUnits = "box",
                Composites = new List<Composite>
                {
                    new Composite
                    {
                        SubProductId = 1, // Refers to existing product
                        Quantity = 3
                    }
                }
            };
            var producerId = "producer-1";

            // Act
            var result = _productBc.CreateProduct(product, producerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Fruit Box", result.Name);

            // Verify the save was called twice (once for product, once for composites)
            _unitOfWork.Received(2).Save();

            // Verify composite was added
            _unitOfWork.Composite.Received(1).Add(Arg.Is<Composite>(c =>
                c.ProductId == result.Id && c.SubProductId == 1 && c.Quantity == 3));
        }

        [Fact]
        public void DeleteProduct_ValidId_SetsIsDeletedTrue()
        {
            // Arrange
            var productId = 1;
            var producerId = "producer-1";

            // Act
            _productBc.DeleteProduct(productId, producerId);

            // Assert
            _unitOfWork.Received(1).Save();
            _unitOfWork.Product.Received(1).Update(Arg.Is<Product>(p =>
                p.Id == productId && p.IsDeleted == true));
        }

        [Fact]
        public void DeleteProduct_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var invalidId = 0;
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _productBc.DeleteProduct(invalidId, producerId));
            Assert.Equal("Invalid product id. (Parameter 'id')", exception.Message);
        }

        [Fact]
        public void DeleteProduct_EmptyProducerId_ThrowsArgumentException()
        {
            // Arrange
            var productId = 1;
            string producerId = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _productBc.DeleteProduct(productId, producerId));
            Assert.Equal("Producer not authenticated. (Parameter 'producerId')", exception.Message);
        }

        [Fact]
        public void DeleteProduct_NonExistingProduct_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistingId = 100;
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _productBc.DeleteProduct(nonExistingId, producerId));
            Assert.Equal("Product not found or you are not authorized to delete it.", exception.Message);
        }

        [Fact]
        public void DeleteProduct_ProductFromAnotherProducer_ThrowsKeyNotFoundException()
        {
            // Arrange
            var productId = 3; // Belongs to producer-2
            var producerId = "producer-1";

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _productBc.DeleteProduct(productId, producerId));
            Assert.Equal("Product not found or you are not authorized to delete it.", exception.Message);
        }

        [Fact]
        public void DeleteProduct_WithComposites_RemovesCompositesFirst()
        {
            // Arrange
            var productId = 2; // Product with composites
            var producerId = "producer-1";

            // Act
            _productBc.DeleteProduct(productId, producerId);

            // Assert
            _unitOfWork.Composite.Received(1).GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Composite, bool>>>());

            // Verify that RemoveRange was called with composites
            _unitOfWork.Composite.Received(1).RemoveRange(Arg.Any<IEnumerable<Composite>>());

            // Verify that Product.Update was called to set IsDeleted
            _unitOfWork.Product.Received(1).Update(Arg.Is<Product>(p =>
                p.Id == productId && p.IsDeleted == true));

            // Verify Save was called twice (once after removing composites, once after updating product)
            _unitOfWork.Received(2).Save();
        }

        [Fact]
        public void UpdateProduct_ValidProduct_UpdatesProduct()
        {
            // Arrange
            var productId = 1;
            var producerId = "producer-1";
            var updatedProduct = new Product
            {
                Name = "Updated Carrots",
                Description = "Updated description",
                Photo = "updated.jpg",
                ReferencePrice = 3.99,
                DeliveryUnits = "pack",
                Composites = new List<Composite>()
            };

            // Act
            _productBc.UpdateProduct(productId, updatedProduct, producerId);

            // Assert
            _unitOfWork.Product.Received(1).Update(Arg.Is<Product>(p =>
                p.Id == productId &&
                p.Name == "Updated Carrots" &&
                p.Description == "Updated description" &&
                p.Photo == "updated.jpg" &&
                p.ReferencePrice == 3.99 &&
                p.DeliveryUnits == "pack"));

            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public void UpdateProduct_NullProduct_ThrowsArgumentException()
        {
            // Arrange
            var productId = 1;
            var producerId = "producer-1";
            Product updatedProduct = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _productBc.UpdateProduct(productId, updatedProduct, producerId));
            Assert.Equal("Invalid product data. (Parameter 'product')", exception.Message);
        }

        [Fact]
        public void UpdateProduct_EmptyProducerId_ThrowsArgumentException()
        {
            // Arrange
            var productId = 1;
            var updatedProduct = new Product
            {
                Name = "Updated Product",
                Description = "Updated description",
                Photo = "updated.jpg",
                ReferencePrice = 3.99,
                DeliveryUnits = "pack"
            };
            string producerId = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _productBc.UpdateProduct(productId, updatedProduct, producerId));
            Assert.Equal("Producer not authenticated. (Parameter 'producerId')", exception.Message);
        }

        [Fact]
        public void UpdateProduct_NonExistingProduct_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistingId = 100;
            var producerId = "producer-1";
            var updatedProduct = new Product
            {
                Name = "Updated Product",
                Description = "Updated description",
                Photo = "updated.jpg",
                ReferencePrice = 3.99,
                DeliveryUnits = "pack"
            };

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() =>
                _productBc.UpdateProduct(nonExistingId, updatedProduct, producerId));
            Assert.Equal("Product not found or you are not authorized to update it.", exception.Message);
        }

        [Fact]
        public void UpdateProduct_ProductFromAnotherProducer_ThrowsKeyNotFoundException()
        {
            // Arrange
            var productId = 3; // Belongs to producer-2
            var producerId = "producer-1";
            var updatedProduct = new Product
            {
                Name = "Updated Product",
                Description = "Updated description",
                Photo = "updated.jpg",
                ReferencePrice = 3.99,
                DeliveryUnits = "pack"
            };

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() =>
                _productBc.UpdateProduct(productId, updatedProduct, producerId));
            Assert.Equal("Product not found or you are not authorized to update it.", exception.Message);
        }
    }
}
