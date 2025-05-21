using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Repository.Mock
{
    public static class MockCatalogRepository
    {
        public static (IUnitOfWork unitOfWork, UserManager<AppUser> userManager) GetMockDependencies()
        {
            var mockUnitOfWork = Substitute.For<IUnitOfWork>();
            var mockProductRepo = Substitute.For<IProductRepository>();
            SetupProductRepository(mockProductRepo);
            mockUnitOfWork.Product.Returns(mockProductRepo);

            var mockUserManager = SetupUserManager();

            return (mockUnitOfWork, mockUserManager);
        }

        private static void SetupProductRepository(IProductRepository mockProductRepo)
        {
            // Test data for products
            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Carrots",
                    Description = "Fresh organic carrots",
                    ProducerId = "producer-1",
                    Photo = "carrots.jpg",
                    ReferencePrice = 2.50,
                    DeliveryUnits = "kg",
                    IsDeleted = false,
                    Composites = new List<Composite>() // Simple product with no composites
                },
                new Product
                {
                    Id = 2,
                    Name = "Fruit Basket",
                    Description = "Assorted fruit basket",
                    ProducerId = "producer-1",
                    Photo = "fruit-basket.jpg",
                    ReferencePrice = 10.00,
                    DeliveryUnits = "basket",
                    IsDeleted = false,
                    Composites = new List<Composite>
                    {
                        new Composite
                        {
                            SubProductId = 3,
                            Quantity = 2,
                            SubProduct = new Product
                            {
                                Id = 3,
                                Name = "Apples",
                                IsDeleted = false
                            }
                        },
                        new Composite
                        {
                            SubProductId = 4,
                            Quantity = 3,
                            SubProduct = new Product
                            {
                                Id = 4,
                                Name = "Oranges",
                                IsDeleted = false
                            }
                        }
                    }
                },
                new Product
                {
                    Id = 5,
                    Name = "Potatoes",
                    Description = "Fresh potatoes",
                    ProducerId = "producer-2",
                    Photo = "potatoes.jpg",
                    ReferencePrice = 1.75,
                    DeliveryUnits = "kg",
                    IsDeleted = false,
                    Composites = new List<Composite>() // Simple product with no composites
                },
                new Product
                {
                    Id = 6,
                    Name = "Deleted Product",
                    Description = "This product is deleted",
                    ProducerId = "producer-1",
                    Photo = "deleted.jpg",
                    ReferencePrice = 5.00,
                    DeliveryUnits = "box",
                    IsDeleted = true, // This product is deleted and should not appear in catalogs
                    Composites = new List<Composite>()
                }
            };

            // Mock GetAll method
            mockProductRepo.GetAll(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Product, bool>>>();
                    var includeProperties = callInfo.Arg<string>();

                    if (predicate != null)
                    {
                        var func = predicate.Compile();
                        return products.Where(func).ToList();
                    }
                    return products;
                });
        }

        private static UserManager<AppUser> SetupUserManager()
        {
            // Test data for users
            var users = new List<AppUser>
            {
                new AppUser
                {
                    Id = "producer-1",
                    UserName = "producer1@example.com",
                    Email = "producer1@example.com",
                    Name = "Producer One",
                    UserType = UserType.Producer
                },
                new AppUser
                {
                    Id = "producer-2",
                    UserName = "producer2@example.com",
                    Email = "producer2@example.com",
                    Name = "Producer Two",
                    UserType = UserType.Producer
                },
                new AppUser
                {
                    Id = "coproducer-1",
                    UserName = "coproducer1@example.com",
                    Email = "coproducer1@example.com",
                    Name = "CoProducer One",
                    UserType = UserType.CoProducer
                }
            };

            // Create a mock user store and user manager
            var mockStore = Substitute.For<IUserStore<AppUser>>();
            var mockUserManager = Substitute.For<UserManager<AppUser>>(
                mockStore, null, null, null, null, null, null, null, null);

            // Mock the relevant methods of UserManager
            mockUserManager.Users.Returns(users.AsQueryable());

            mockUserManager.FindByIdAsync(Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var userId = callInfo.Arg<string>();
                    return Task.FromResult(users.FirstOrDefault(u => u.Id == userId));
                });

            return mockUserManager;
        }
    }
}
