using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using NSubstitute;
using System.Linq.Expressions;

namespace AMAP.UnitTests.Business.Mock
{
    public static class MockProductRepository
    {
        public static IUnitOfWork GetMockUnitOfWork()
        {
            var mockUnitOfWork = Substitute.For<IUnitOfWork>();

            // Set up Product repository
            var mockProductRepo = Substitute.For<IProductRepository>();
            SetupProductRepository(mockProductRepo);
            mockUnitOfWork.Product.Returns(mockProductRepo);

            // Set up Composite repository
            var mockCompositeRepo = Substitute.For<ICompositeRepository>();
            SetupCompositeRepository(mockCompositeRepo);
            mockUnitOfWork.Composite.Returns(mockCompositeRepo);

            // Set up AppUser repository
            var mockAppUserRepo = Substitute.For<IAppUserRepository>();
            SetupAppUserRepository(mockAppUserRepo);
            mockUnitOfWork.AppUser.Returns(mockAppUserRepo);

            return mockUnitOfWork;
        }

        private static void SetupProductRepository(IProductRepository mockProductRepo)
        {
            // Setup test products
            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    ProducerId = "producer-1",
                    Name = "Carrots",
                    Description = "Fresh organic carrots",
                    Photo = "carrots.jpg",
                    ReferencePrice = 2.50,
                    DeliveryUnits = "kg",
                    IsDeleted = false,
                    Composites = new List<Composite>()
                },
                new Product
                {
                    Id = 2,
                    ProducerId = "producer-1",
                    Name = "Vegetable Box",
                    Description = "Mixed vegetables box",
                    Photo = "vegbox.jpg",
                    ReferencePrice = 15.99,
                    DeliveryUnits = "box",
                    IsDeleted = false,
                    Composites = new List<Composite>
                    {
                        new Composite
                        {
                            ProductId = 2,
                            SubProductId = 1,
                            Quantity = 2,
                            SubProduct = new Product
                            {
                                Id = 1,
                                Name = "Carrots",
                                ProducerId = "producer-1"
                            }
                        }
                    }
                },
                new Product
                {
                    Id = 3,
                    ProducerId = "producer-2",
                    Name = "Apples",
                    Description = "Fresh apples",
                    Photo = "apples.jpg",
                    ReferencePrice = 3.99,
                    DeliveryUnits = "kg",
                    IsDeleted = false,
                    Composites = new List<Composite>()
                },
                new Product
                {
                    Id = 4,
                    ProducerId = "producer-1",
                    Name = "Deleted Product",
                    Description = "This product is deleted",
                    Photo = "deleted.jpg",
                    ReferencePrice = 1.99,
                    DeliveryUnits = "unit",
                    IsDeleted = true,
                    Composites = new List<Composite>()
                }
            };

            // Setup Get method
            mockProductRepo.Get(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Product, bool>>>();
                    var includes = callInfo.Arg<string>();

                    // Apply the predicate
                    var func = predicate.Compile();
                    var result = products.FirstOrDefault(func);

                    return result;
                });

            // Setup GetAll method
            mockProductRepo.GetAll(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Product, bool>>>();

                    // Apply the predicate
                    var func = predicate.Compile();
                    var result = products.Where(func).ToList();

                    return result;
                });


            // Setup Add method
            mockProductRepo.When(x => x.Add(Arg.Any<Product>()))
                .Do(callInfo =>
                {
                    var product = callInfo.Arg<Product>();
                    product.Id = products.Count + 1;
                    products.Add(product);
                });
            ;

            // Setup Update method
            mockProductRepo.When(x => x.Update(Arg.Any<Product>()))
                .Do(callInfo =>
                {
                    var product = callInfo.Arg<Product>();
                    var existingProduct = products.FirstOrDefault(p => p.Id == product.Id);
                    if (existingProduct != null)
                    {
                        // Update properties
                        products.Remove(existingProduct);
                        products.Add(product);
                    }
                });
        }

        private static void SetupCompositeRepository(ICompositeRepository mockCompositeRepo)
        {
            // Create a list of composites that includes ones for our test product
            var composites = new List<Composite>
    {
        new Composite
        {
            ProductId = 2, // This matches our test product ID
            SubProductId = 1,
            Quantity = 2
        },
        new Composite
        {
            ProductId = 3,
            SubProductId = 1,
            Quantity = 3
        }
    };

            // Setup Add method
            mockCompositeRepo.When(x => x.Add(Arg.Any<Composite>()))
                .Do(callInfo =>
                {
                    var composite = callInfo.Arg<Composite>();
                    composites.Add(composite);
                });

            // Setup GetAll method to return proper composites for our test
            mockCompositeRepo.GetAll(Arg.Any<Expression<Func<Composite, bool>>>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<Composite, bool>>>();

                    // Apply the predicate to our list of composites
                    if (predicate != null)
                    {
                        var func = predicate.Compile();
                        return composites.Where(func).ToList();
                    }
                    return composites;
                });

            // Setup RemoveRange method
            mockCompositeRepo.When(x => x.RemoveRange(Arg.Any<IEnumerable<Composite>>()))
                .Do(callInfo =>
                {
                    var compositesToRemove = callInfo.Arg<IEnumerable<Composite>>();
                    foreach (var composite in compositesToRemove)
                    {
                        var existingComposite = composites.FirstOrDefault(c =>
                            c.ProductId == composite.ProductId && c.SubProductId == composite.SubProductId);
                        if (existingComposite != null)
                        {
                            composites.Remove(existingComposite);
                        }
                    }
                });
        }


        private static void SetupAppUserRepository(IAppUserRepository mockAppUserRepo)
        {
            var users = new List<AppUser>
            {
                new AppUser
                {
                    Id = "producer-1",
                    Name = "Test Producer 1",
                    UserType = UserType.Producer,
                    Email = "producer1@test.com"
                },
                new AppUser
                {
                    Id = "producer-2",
                    Name = "Test Producer 2",
                    UserType = UserType.Producer,
                    Email = "producer2@test.com"
                }
            };

            // Setup Get method
            mockAppUserRepo.Get(Arg.Any<Expression<Func<AppUser, bool>>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var predicate = callInfo.Arg<Expression<Func<AppUser, bool>>>();

                    // Apply the predicate
                    var func = predicate.Compile();
                    var result = users.FirstOrDefault(func);

                    return result;
                });
        }
    }
}
