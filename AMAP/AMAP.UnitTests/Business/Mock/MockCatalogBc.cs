using AMAP.Business.Interfaces;
using AMAP.Entities.Models.DTO.Catalog;
using NSubstitute;

namespace AMAP.UnitTests.Business.Mock
{
    public static class MockCatalogBc
    {
        public static ICatalogBc GetMockCatalogBc()
        {
            var mockCatalogBc = Substitute.For<ICatalogBc>();

            SetupGetAllCatalogs(mockCatalogBc);
            SetupGetCatalogByProducer(mockCatalogBc);

            return mockCatalogBc;
        }

        private static void SetupGetAllCatalogs(ICatalogBc mockCatalogBc)
        {
            // Setup the mock to dynamically check the flag each time the method is called
            mockCatalogBc
                .GetAllCatalogs()
                .Returns(_ =>
                {
                    // Check the flag and return appropriate result
                    if (MockIsEmptyFlag.IsGetAllCatalogsEmpty)
                    {
                        return Task.FromResult(new List<CatalogDto>());
                    }
                    else
                    {
                        return Task.FromResult(new List<CatalogDto>
                        {
                            new CatalogDto
                            {
                                ProducerName = "Test Producer 1",
                                Products = new List<ProductDto>
                                {
                                    new ProductDto
                                    {
                                        Name = "Product 1",
                                        Description = "Description 1",
                                        Photo = "photo1.jpg",
                                        ReferencePrice = 10.99,
                                        DeliveryUnits = "kg",
                                        Type = "Simple"
                                    },
                                    new ProductDto
                                    {
                                        Name = "Product 2",
                                        Description = "Description 2",
                                        Photo = "photo2.jpg",
                                        ReferencePrice = 15.50,
                                        DeliveryUnits = "box",
                                        Type = "Simple"
                                    }
                                }
                            },
                            new CatalogDto
                            {
                                ProducerName = "Test Producer 2",
                                Products = new List<ProductDto>
                                {
                                    new ProductDto
                                    {
                                        Name = "Product 3",
                                        Description = "Description 3",
                                        Photo = "photo3.jpg",
                                        ReferencePrice = 5.99,
                                        DeliveryUnits = "unit",
                                        Type = "Simple"
                                    }
                                }
                            }
                        });
                    }
                });
        }

        private static void SetupGetCatalogByProducer(ICatalogBc mockCatalogBc)
        {
            // Regular producer with products
            mockCatalogBc
                .GetCatalogByProducer("test-producer-id")
                .Returns(Task.FromResult(new List<CatalogDto>
                {
                    new CatalogDto
                    {
                        ProducerName = "Test Producer",
                        Products = new List<ProductDto>
                        {
                            new ProductDto
                            {
                                Name = "Product 1",
                                Description = "Description 1",
                                Photo = "photo1.jpg",
                                ReferencePrice = 10.99,
                                DeliveryUnits = "kg",
                                Type = "Simple"
                            },
                            new ProductDto
                            {
                                Name = "Product 2",
                                Description = "Description 2",
                                Photo = "photo2.jpg",
                                ReferencePrice = 15.50,
                                DeliveryUnits = "box",
                                Type = "Simple"
                            }
                        }
                    }
                }));

            // Producer without products or non-existing producer
            mockCatalogBc
                .GetCatalogByProducer("non-existing-producer-id")
                .Returns(Task.FromResult(new List<CatalogDto>()));

            // Producer with many products
            mockCatalogBc
                .GetCatalogByProducer("producer-with-many-products")
                .Returns(Task.FromResult(new List<CatalogDto>
                {
                    new CatalogDto
                    {
                        ProducerName = "Producer With Many Products",
                        Products = new List<ProductDto>
                        {
                            new ProductDto
                            {
                                Name = "Product 1",
                                Description = "Description 1",
                                Photo = "photo1.jpg",
                                ReferencePrice = 10.99,
                                DeliveryUnits = "kg",
                                Type = "Simple"
                            },
                            new ProductDto
                            {
                                Name = "Product 2",
                                Description = "Description 2",
                                Photo = "photo2.jpg",
                                ReferencePrice = 15.50,
                                DeliveryUnits = "box",
                                Type = "Simple"
                            },
                            new ProductDto
                            {
                                Name = "Product 3",
                                Description = "Description 3",
                                Photo = "photo3.jpg",
                                ReferencePrice = 20.00,
                                DeliveryUnits = "unit",
                                Type = "Composite",
                                Composites = new List<ProductComposeDto>
                                {
                                    new ProductComposeDto { SubProductName = "Sub Product A", Quantity = 2 }
                                }
                            },
                            new ProductDto
                            {
                                Name = "Product 4",
                                Description = "Description 4",
                                Photo = "photo4.jpg",
                                ReferencePrice = 5.99,
                                DeliveryUnits = "piece",
                                Type = "Simple"
                            },
                            new ProductDto
                            {
                                Name = "Product 5",
                                Description = "Description 5",
                                Photo = "photo5.jpg",
                                ReferencePrice = 8.75,
                                DeliveryUnits = "kg",
                                Type = "Simple"
                            }
                        }
                    }
                }));
        }
    }

    // Helper class to control the behavior of mocks
    public static class MockIsEmptyFlag
    {
        public static bool IsGetAllCatalogsEmpty { get; set; } = false;
    }
}
