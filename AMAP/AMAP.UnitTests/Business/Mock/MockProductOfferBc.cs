using AMAP.Business.Interfaces;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AMAP.UnitTests.Business.Mock
{
    public static class MockProductOfferBc
    {
        public static IProductOfferBc GetMockProductOfferBc()
        {
            var mockProductOfferBc = Substitute.For<IProductOfferBc>();

            SetupCreateProductOffer(mockProductOfferBc);
            SetupGetProductOfferById(mockProductOfferBc);
            SetupGetProductOfferByProducer(mockProductOfferBc);
            SetupDeleteProductOffer(mockProductOfferBc);

            return mockProductOfferBc;
        }

        private static void SetupCreateProductOffer(IProductOfferBc mockProductOfferBc)
        {
            // Valid product offer creation
            mockProductOfferBc
                .CreateProductOffer(
                    Arg.Is<ProductOffer>(po =>
                        po != null &&
                        po.ProductId > 0 &&
                        po.SubscriptionPeriodId > 0 &&
                        po.Quantity > 0 &&
                        po.Price > 0 &&
                        po.DeliveryDate > DateTime.Now),
                    Arg.Is<string>(id => id == "test-producer-id")
                )
                .Returns(callInfo =>
                {
                    var po = callInfo.Arg<ProductOffer>();
                    po.Id = 100;  // Assign a new ID
                    return po;
                });

            // Null product offer
            mockProductOfferBc
                .CreateProductOffer(
                    Arg.Is<ProductOffer>(po => po == null),
                    Arg.Any<string>()
                )
                .Throws(new ArgumentException("Invalid data."));

            // Missing producer ID
            mockProductOfferBc
                .CreateProductOffer(
                    Arg.Any<ProductOffer>(),
                    Arg.Is<string>(id => string.IsNullOrEmpty(id))
                )
                .Throws(new ArgumentException("Producer not authenticated."));

            // Invalid product ID
            mockProductOfferBc
                .CreateProductOffer(
                    Arg.Is<ProductOffer>(po => po != null && po.ProductId <= 0),
                    Arg.Any<string>()
                )
                .Throws(new KeyNotFoundException("Product not found."));

            // Product not owned by producer
            mockProductOfferBc
                .CreateProductOffer(
                    Arg.Is<ProductOffer>(po => po != null && po.ProductId == 999),
                    Arg.Any<string>()
                )
                .Throws(new InvalidOperationException("You cannot create an offer for a product from another producer."));

            // Invalid subscription period
            mockProductOfferBc
                .CreateProductOffer(
                    Arg.Is<ProductOffer>(po => po != null && po.SubscriptionPeriodId == 999),
                    Arg.Any<string>()
                )
                .Throws(new KeyNotFoundException("Subscription period not found."));

            // Delivery date in the past
            mockProductOfferBc
                .CreateProductOffer(
                    Arg.Is<ProductOffer>(po => po != null && po.DeliveryDate < DateTime.Now),
                    Arg.Any<string>()
                )
                .Throws(new InvalidOperationException("Delivery date cannot be in the past."));

            // Duplicate offer for same product and date
            mockProductOfferBc
                .CreateProductOffer(
                    Arg.Is<ProductOffer>(po =>
                        po != null &&
                        po.ProductId == 5 &&
                        po.DeliveryDate.Date == DateTime.Now.AddDays(7).Date),
                    Arg.Any<string>()
                )
                .Throws(new InvalidOperationException("A product offer already exists for this product on the selected delivery date."));

            // Invalid quantity
            mockProductOfferBc
                .CreateProductOffer(
                    Arg.Is<ProductOffer>(po => po != null && po.Quantity <= 0),
                    Arg.Any<string>()
                )
                .Throws(new ArgumentException("Product offer quantity must be greater than zero."));

            // Invalid payment type
            mockProductOfferBc
                .CreateProductOffer(
                    Arg.Is<ProductOffer>(po => po != null && po.PaymentType == 0),
                    Arg.Any<string>()
                )
                .Throws(new ArgumentException("Invalid payment type."));

            // In your MockProductOfferBc.cs
            mockProductOfferBc
                .CreateProductOffer(
                    Arg.Is<ProductOffer>(p => p.ProductId == 777),
                    Arg.Any<string>())
                .Throws(new Exception("Simulated server error"));
        }

        private static void SetupGetProductOfferById(IProductOfferBc mockProductOfferBc)
        {
            // Valid product offer (ID: 1)
            mockProductOfferBc.GetProductOfferById(1).Returns(new ProductOffer
            {
                Id = 1,
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 12.99,
                Quantity = 10,
                DeliveryDate = DateTime.Now.AddDays(7),
                PaymentType = PaymentType.Money | PaymentType.BankTransfer
            });

            // Non-existing product offer (ID: 999)
            mockProductOfferBc.GetProductOfferById(999).Returns((ProductOffer)null);

            // Invalid ID argument
            mockProductOfferBc
                .GetProductOfferById(Arg.Is<int>(id => id <= 0))
                .Throws(new ArgumentException("Product offer ID must be greater than zero."));
        }

        private static void SetupGetProductOfferByProducer(IProductOfferBc mockProductOfferBc)
        {
            // Valid producer ID
            mockProductOfferBc.GetProductOfferByProducer("test-producer-id").Returns(new ProductOffer
            {
                Id = 1,
                ProductId = 1,
                SubscriptionPeriodId = 1,
                Price = 12.99,
                Quantity = 10,
                DeliveryDate = DateTime.Now.AddDays(7),
                PaymentType = PaymentType.Money | PaymentType.BankTransfer
            });

            // Non-existing producer offers
            mockProductOfferBc.GetProductOfferByProducer("non-existing-producer").Returns((ProductOffer)null);

            // Invalid producer ID
            mockProductOfferBc
                .GetProductOfferByProducer(Arg.Is<string>(id => string.IsNullOrEmpty(id)))
                .Throws(new ArgumentException("Producer ID cannot be empty."));
        }

        private static void SetupDeleteProductOffer(IProductOfferBc mockProductOfferBc)
        {
            // Successful delete
            mockProductOfferBc
                .When(x => x.DeleteProductOffer(
                    Arg.Is<int>(id => id > 0 && id != 999)
                ))
                .Do(x => { /* Do nothing, successful deletion */ });

            // Non-existing product offer
            mockProductOfferBc
                .When(x => x.DeleteProductOffer(999))
                .Throw(new KeyNotFoundException("Product offer not found."));

            // Invalid ID argument
            mockProductOfferBc
                .When(x => x.DeleteProductOffer(Arg.Is<int>(id => id <= 0)))
                .Throw(new ArgumentException("Product offer ID must be greater than zero."));

            // Product offer with subscriptions
            mockProductOfferBc
                .When(x => x.DeleteProductOffer(50))
                .Throw(new InvalidOperationException("Cannot delete product offer with active subscriptions."));
        }
    }
}
