using AMAP.Business.Interfaces;
using AMAP.Entities;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AMAP.UnitTests.Business.Mock
{
    public static class MockProductBc
    {
        public static IProductBc GetMockProductBc()
        {
            var mockProductBc = Substitute.For<IProductBc>();

            SetupGetProduct(mockProductBc);
            SetupCreateProduct(mockProductBc);
            SetupUpdateProduct(mockProductBc);
            SetupDeleteProduct(mockProductBc);
            SetupImportProductsFromFile(mockProductBc);
            SetupExportProductsToFile(mockProductBc);

            return mockProductBc;
        }

        private static void SetupGetProduct(IProductBc mockProductBc)
        {
            // Regular valid product (ID: 1)
            mockProductBc.GetProduct(1).Returns(new Product
            {
                Id = 1,
                Name = "Test Product",
                Description = "Test Description",
                Photo = "test-photo.jpg",
                ReferencePrice = 10.99,
                DeliveryUnits = "kg",
                Composites = new List<Composite>()
            });

            // Composite product (ID: 2)
            var compositeProduct = new Product
            {
                Id = 2,
                Name = "Composite Product",
                Description = "A product with sub-products",
                Photo = "composite.jpg",
                ReferencePrice = 15.99,
                DeliveryUnits = "box",
                Composites = new List<Composite>()
            };

            var subProduct = new Product
            {
                Id = 10,
                Name = "Sub Product",
                Description = "A component product",
                Photo = "sub.jpg",
                ReferencePrice = 5.99,
                DeliveryUnits = "piece"
            };

            compositeProduct.Composites.Add(new Composite
            {
                ProductId = 2,
                SubProductId = 10,
                SubProduct = subProduct,
                Product = compositeProduct,
                Quantity = 3
            });

            mockProductBc.GetProduct(2).Returns(compositeProduct);

            // Simple product (ID: 3)
            mockProductBc.GetProduct(3).Returns(new Product
            {
                Id = 3,
                Name = "Simple Product",
                Description = "Just a simple product",
                Photo = "simple.jpg",
                ReferencePrice = 7.50,
                DeliveryUnits = "piece",
                Composites = new List<Composite>()
            });

            // Non-existing product
            mockProductBc.GetProduct(999).Returns((Product)null);
        }

        private static void SetupCreateProduct(IProductBc mockProductBc)
        {
            // Valid product creation
            mockProductBc
                .CreateProduct(
                    Arg.Is<Product>(p =>
                        p != null &&
                        p.Name == "New Test Product" &&
                        !string.IsNullOrEmpty(p.Description)),
                    Arg.Is<string>(id => id == "test-producer-id")
                )
                .Returns(callInfo =>
                {
                    var p = callInfo.Arg<Product>();
                    p.Id = 100;  // Assign a new ID
                    p.ProducerId = "test-producer-id";
                    return p;
                });

            // Duplicate product name
            mockProductBc
                .CreateProduct(
                    Arg.Is<Product>(p =>
                        p != null &&
                        p.Name == "Existing Product"),
                    Arg.Any<string>()
                )
                .Throws(new InvalidOperationException("You already have a product with the same name."));

            // Null product
            mockProductBc
                .CreateProduct(
                    Arg.Is<Product>(p => p == null),
                    Arg.Any<string>()
                )
                .Throws(new ArgumentException("Invalid product data.", "product"));

            // Missing producer ID
            mockProductBc
                .CreateProduct(
                    Arg.Any<Product>(),
                    Arg.Is<string>(id => string.IsNullOrEmpty(id))
                )
                .Throws(new ArgumentException("Producer not authenticated.", "producerId"));

            // Invalid subproduct
            mockProductBc
                .CreateProduct(
                    Arg.Is<Product>(p =>
                        p != null &&
                        p.Composites != null &&
                        p.Composites.Any(c => c.SubProductId == 9999)),
                    Arg.Any<string>()
                )
                .Throws(new InvalidOperationException("Subproduct with ID 9999 does not exist."));
        }

        private static void SetupUpdateProduct(IProductBc mockProductBc)
        {
            // Successful update
            mockProductBc
                .When(x => x.UpdateProduct(
                    Arg.Is<int>(id => id == 1),
                    Arg.Is<Product>(p => p != null),
                    Arg.Is<string>(pid => pid == "test-producer-id")
                ))
                .Do(x => { /* Do nothing, successful update */ });

            // Product not found or unauthorized
            mockProductBc
                .When(x => x.UpdateProduct(
                    Arg.Is<int>(id => id == 999),
                    Arg.Any<Product>(),
                    Arg.Any<string>()
                ))
                .Throw(new KeyNotFoundException("Product not found or you are not authorized to update it."));

            // Null product
            mockProductBc
                .When(x => x.UpdateProduct(
                    Arg.Any<int>(),
                    Arg.Is<Product>(p => p == null),
                    Arg.Any<string>()
                ))
                .Throw(new ArgumentException("Invalid product data.", "product"));

            // Missing producer ID
            mockProductBc
                .When(x => x.UpdateProduct(
                    Arg.Any<int>(),
                    Arg.Any<Product>(),
                    Arg.Is<string>(id => string.IsNullOrEmpty(id))
                ))
                .Throw(new ArgumentException("Producer not authenticated.", "producerId"));

            // Duplicate product name
            mockProductBc
                .When(x => x.UpdateProduct(
                    Arg.Is<int>(id => id == 2),
                    Arg.Is<Product>(p => p != null && p.Name == "Duplicate Name"),
                    Arg.Any<string>()
                ))
                .Throw(new InvalidOperationException("You already have a product with the same name."));

            // Invalid subproduct
            mockProductBc
                .When(x => x.UpdateProduct(
                    Arg.Is<int>(id => id == 3),
                    Arg.Is<Product>(p => p != null && p.Composites != null && p.Composites.Any(c => c.SubProductId == 9999)),
                    Arg.Any<string>()
                ))
                .Throw(new InvalidOperationException("Invalid subproduct with ID 9999 or it does not belong to the producer."));
        }

        private static void SetupDeleteProduct(IProductBc mockProductBc)
        {
            // Successful delete
            mockProductBc
                .When(x => x.DeleteProduct(
                    Arg.Is<int>(id => id == 1),
                    Arg.Is<string>(pid => pid == "test-producer-id")
                ))
                .Do(x => { /* Do nothing, successful deletion */ });

            // Product not found or unauthorized
            mockProductBc
                .When(x => x.DeleteProduct(
                    Arg.Is<int>(id => id == 999),
                    Arg.Any<string>()
                ))
                .Throw(new KeyNotFoundException("Product not found or you are not authorized to delete it."));

            // Invalid product ID
            mockProductBc
                .When(x => x.DeleteProduct(
                    Arg.Is<int>(id => id <= 0),
                    Arg.Any<string>()
                ))
                .Throw(new ArgumentException("Invalid product id.", "id"));

            // Missing producer ID
            mockProductBc
                .When(x => x.DeleteProduct(
                    Arg.Any<int>(),
                    Arg.Is<string>(id => string.IsNullOrEmpty(id))
                ))
                .Throw(new ArgumentException("Producer not authenticated.", "producerId"));
        }

        private static void SetupImportProductsFromFile(IProductBc mockProductBc)
        {
            // Successful import
            mockProductBc
                .ImportProductsFromFile(
                    Arg.Any<Stream>(),
                    Arg.Is<string>(fn => fn.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)),
                    Arg.Is<string>(pid => pid == "test-producer-id")
                )
                .Returns(callInfo =>
                {
                    var products = new List<Product>
                    {
                        new Product
                        {
                            Id = 101,
                            Name = "Imported Product 1",
                            Description = "Imported Description 1",
                            Photo = "imported1.jpg",
                            ReferencePrice = 9.99,
                            DeliveryUnits = "box"
                        },
                        new Product
                        {
                            Id = 102,
                            Name = "Imported Product 2",
                            Description = "Imported Description 2",
                            Photo = "imported2.jpg",
                            ReferencePrice = 12.99,
                            DeliveryUnits = "kg"
                        }
                    };

                    var errors = new List<string>();
                    return (products, errors); // Ensure this matches the expected return type
                });

            // Import with warnings
            mockProductBc
                .ImportProductsFromFile(
                    Arg.Any<Stream>(),
                    Arg.Is<string>(fn => fn.Contains("warnings")),
                    Arg.Is<string>(pid => pid == "test-producer-id")
                )
                .Returns(callInfo =>
                {
                    var products = new List<Product>
                    {
                        new Product
                        {
                            Id = 101,
                            Name = "Imported Product 1",
                            Description = "Imported Description 1",
                            Photo = "imported1.jpg",
                            ReferencePrice = 9.99,
                            DeliveryUnits = "box"
                        }
                    };

                    var errors = new List<string>
                    {
                        "Line 2: Invalid price value '10,xx'. Use format 0.00",
                        "Line 3: Missing required field 'Description'"
                    };
                    return (products, errors); // Ensure this matches the expected return type
                });

            // Empty file
            mockProductBc
                .ImportProductsFromFile(
                    Arg.Is<Stream>(s => s.Length == 0),
                    Arg.Any<string>(),
                    Arg.Any<string>()
                )
                .Throws(new ArgumentException("No file was uploaded.", "fileStream"));

            // Invalid file format
            mockProductBc
                .ImportProductsFromFile(
                    Arg.Any<Stream>(),
                    Arg.Is<string>(fn => !fn.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)),
                    Arg.Any<string>()
                )
                .Throws(new ArgumentException("File must be text format.", "fileName"));

            // Missing producer ID
            mockProductBc
                .ImportProductsFromFile(
                    Arg.Any<Stream>(),
                    Arg.Any<string>(),
                    Arg.Is<string>(id => string.IsNullOrEmpty(id))
                )
                .Throws(new ArgumentException("Producer not authenticated.", "producerId"));

            // Duplicate product names
            mockProductBc
                .ImportProductsFromFile(
                    Arg.Any<Stream>(),
                    Arg.Is<string>(fn => fn.Contains("duplicate")),
                    Arg.Any<string>()
                )
                .Throws(x =>
                {
                    var ex = new InvalidOperationException("Import failed. Found duplicate product names.");
                    ex.Data["Errors"] = new List<string>
                    {
                        "Line 2: Product with name 'Duplicate Product' already exists",
                        "Line 5: Product with name 'Another Duplicate' already exists"
                    };
                    return ex;
                });

            // Processing error
            mockProductBc
                .ImportProductsFromFile(
                    Arg.Any<Stream>(),
                    Arg.Is<string>(fn => fn.Contains("error")),
                    Arg.Any<string>()
                )
                .Throws(new Exception("Unexpected error during file processing"));
        }

        private static void SetupExportProductsToFile(IProductBc mockProductBc)
        {
            // Successful export
            mockProductBc
                .ExportProductsToFile(
                    Arg.Is<string>(pid => pid == "test-producer-id"),
                    Arg.Any<string>()
                )
                .Returns(callInfo =>
                {
                    var fileContent = System.Text.Encoding.UTF8.GetBytes(
                        "Name,Description,Photo,Price,Units\r\n" +
                        "Test Product,Test Description,test-photo.jpg,10.99,kg\r\n" +
                        "Simple Product,Just a simple product,simple.jpg,7.50,piece"
                    );
                    var fileName = "products_export.csv";
                    var contentType = "text/csv";

                    return (fileContent, fileName, contentType);
                });

            // Producer not found
            mockProductBc
                .ExportProductsToFile(
                    Arg.Is<string>(pid => pid == "non-existing-producer"),
                    Arg.Any<string>()
                )
                .Throws(new KeyNotFoundException("Producer not found."));

            // No products
            mockProductBc
                .ExportProductsToFile(
                    Arg.Is<string>(pid => pid == "empty-producer"),
                    Arg.Any<string>()
                )
                .Throws(new InvalidOperationException("No products found for export."));

            // Missing producer ID
            mockProductBc
                .ExportProductsToFile(
                    Arg.Is<string>(pid => string.IsNullOrEmpty(pid)),
                    Arg.Any<string>()
                )
                .Throws(new ArgumentException("Invalid producer ID."));
        }

        public static IWebHostEnvironment GetMockWebHostEnvironment()
        {
            var mockEnv = Substitute.For<IWebHostEnvironment>();
            mockEnv.ContentRootPath.Returns("/app/root");
            return mockEnv;
        }
    }
}
