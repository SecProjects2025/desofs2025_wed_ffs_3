using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.Extensions.Logging;

namespace AMAP.Business.Components
{
    public class ProductOfferBc : IProductOfferBc
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductOfferBc> _logger;

        public ProductOfferBc(IUnitOfWork unitOfWork, ILogger<ProductOfferBc> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public ProductOffer CreateProductOffer(ProductOffer productOffer, string producerId)
        {
            if (productOffer == null)
                throw new ArgumentException("Invalid data.");

            // Log no início da criação da oferta
            _logger.LogInformation("Product offer creation initiated - ProductId: {ProductId}, ProducerId: {ProducerId}, SubscriptionPeriodId: {SubscriptionPeriodId} at {Time}",
                productOffer.ProductId, producerId, productOffer.SubscriptionPeriodId, DateTime.UtcNow);

            // Obter o produto
            var product = _unitOfWork.Product.Get(p => p.Id == productOffer.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product offer creation failed - Product not found - ProductId: {ProductId}, ProducerId: {ProducerId} at {Time}",
                    productOffer.ProductId, producerId, DateTime.UtcNow);
                throw new KeyNotFoundException("Product not found.");
            }

            // Verificar se o produto pertence ao produtor autenticado
            if (string.IsNullOrEmpty(producerId) || product.ProducerId != producerId)
            {
                _logger.LogWarning("Product offer creation failed - Unauthorized producer - RequestedProductId: {ProductId}, ProductOwnerId: {ProductOwnerId}, RequestingProducerId: {RequestingProducerId} at {Time}",
                    productOffer.ProductId, product.ProducerId, producerId, DateTime.UtcNow);
                throw new InvalidOperationException("You cannot create an offer for a product from another producer.");
            }

            // Validate SubscriptionPeriodId
            var subscriptionPeriod = _unitOfWork.SubscriptionPeriod.Get(sp => sp.Id == productOffer.SubscriptionPeriodId);
            if (subscriptionPeriod == null)
            {
                _logger.LogWarning("Product offer creation failed - Subscription period not found - SubscriptionPeriodId: {SubscriptionPeriodId}, ProducerId: {ProducerId} at {Time}",
                    productOffer.SubscriptionPeriodId, producerId, DateTime.UtcNow);
                throw new KeyNotFoundException("Subscription period not found.");
            }

            // Validate DeliveryDate
            if (productOffer.DeliveryDate < DateTime.Now)
            {
                _logger.LogWarning("Product offer creation failed - Past delivery date - DeliveryDate: {DeliveryDate}, ProducerId: {ProducerId} at {Time}",
                    productOffer.DeliveryDate, producerId, DateTime.UtcNow);
                throw new InvalidOperationException("Delivery date cannot be in the past.");
            }

            // Verificar se já existe uma oferta para o mesmo produto e mesma data de entrega
            var existingOffer = _unitOfWork.ProductOffer.GetAll(po =>
                po.ProductId == productOffer.ProductId &&
                po.DeliveryDate == productOffer.DeliveryDate).FirstOrDefault();

            if (existingOffer != null)
            {
                _logger.LogWarning("Product offer creation failed - Duplicate offer - ProductId: {ProductId}, DeliveryDate: {DeliveryDate}, ProducerId: {ProducerId} at {Time}",
                    productOffer.ProductId, productOffer.DeliveryDate, producerId, DateTime.UtcNow);
                throw new InvalidOperationException("A product offer already exists for this product on the selected delivery date.");
            }

            // Obter datas de entrega associadas ao período de subscrição
            subscriptionPeriod.DeliveryDates = _unitOfWork.DeliveryDate.GetAll(d =>
                d.SubscriptionPeriodId == productOffer.SubscriptionPeriodId);

            // Verificar se a data de entrega está dentro do período de subscrição
            if (!subscriptionPeriod.DeliveryDates.Select(dd => dd.Date).Contains(productOffer.DeliveryDate))
            {
                _logger.LogWarning("Product offer creation failed - Delivery date outside subscription period - DeliveryDate: {DeliveryDate}, SubscriptionPeriodId: {SubscriptionPeriodId}, ProducerId: {ProducerId} at {Time}",
                    productOffer.DeliveryDate, productOffer.SubscriptionPeriodId, producerId, DateTime.UtcNow);
                throw new InvalidOperationException("Delivery date outside subscription period.");
            }

            // Validar quantidade
            if (productOffer.Quantity <= 0)
            {
                _logger.LogWarning("Product offer creation failed - Invalid quantity - Quantity: {Quantity}, ProducerId: {ProducerId} at {Time}",
                    productOffer.Quantity, producerId, DateTime.UtcNow);
                throw new ArgumentException("Product offer quantity must be greater than zero.");
            }

            // Validar tipo de pagamento
            if (productOffer.PaymentType <= 0 ||
                (int)productOffer.PaymentType > Enum.GetValues<PaymentType>().Cast<int>().Sum())
            {
                _logger.LogWarning("Product offer creation failed - Invalid payment type - PaymentType: {PaymentType}, ProducerId: {ProducerId} at {Time}",
                    productOffer.PaymentType, producerId, DateTime.UtcNow);
                throw new ArgumentException("Invalid payment type.");
            }

            _unitOfWork.ProductOffer.Add(productOffer);
            _unitOfWork.Save();

            // Log de sucesso na criação da oferta
            _logger.LogInformation("Product offer created successfully - ID: {OfferId}, ProductId: {ProductId}, ProducerId: {ProducerId}, Price: {Price}, Quantity: {Quantity}, DeliveryDate: {DeliveryDate} at {Time}",
                productOffer.Id, productOffer.ProductId, producerId, productOffer.Price, productOffer.Quantity, productOffer.DeliveryDate, DateTime.UtcNow);

            return productOffer;
        }

        public ProductOffer? GetProductOfferById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid product offer ID.", nameof(id));

            var productOffer = _unitOfWork.ProductOffer.Get(po => po.Id == id);

            // Log de acesso aos dados da oferta
            if (productOffer != null)
            {
                _logger.LogInformation("Product offer data accessed - ID: {OfferId}, ProductId: {ProductId}, SubscriptionPeriodId: {SubscriptionPeriodId}, Price: {Price}, Quantity: {Quantity} at {Time}",
                    productOffer.Id, productOffer.ProductId, productOffer.SubscriptionPeriodId, productOffer.Price, productOffer.Quantity, DateTime.UtcNow);
            }
            else
            {
                _logger.LogInformation("Product offer access attempted but not found - ID: {OfferId} at {Time}", id, DateTime.UtcNow);
            }

            return productOffer;
        }

        public ProductOffer? GetProductOfferByProducer(string producerId)
        {
            if (string.IsNullOrEmpty(producerId))
            {
                _logger.LogWarning("Product offer access failed - Empty producer ID provided at {Time}", DateTime.UtcNow);
                throw new ArgumentException("Invalid producer ID.", nameof(producerId));
            }

            var productOffer = _unitOfWork.ProductOffer.Get(po => po.Product.ProducerId == producerId);

            // Log de acesso às ofertas do produtor
            if (productOffer != null)
            {
                _logger.LogInformation("Producer's product offer data accessed - ProducerId: {ProducerId}, OfferId: {OfferId}, ProductId: {ProductId} at {Time}",
                    producerId, productOffer.Id, productOffer.ProductId, DateTime.UtcNow);
            }
            else
            {
                _logger.LogInformation("Producer's product offer access attempted but not found - ProducerId: {ProducerId} at {Time}", producerId, DateTime.UtcNow);
            }

            return productOffer;
        }

        public void DeleteProductOffer(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid product offer ID.", nameof(id));

            // Log início da tentativa de exclusão
            _logger.LogInformation("Product offer deletion initiated - ID: {OfferId} at {Time}", id, DateTime.UtcNow);

            var productOffer = _unitOfWork.ProductOffer.Get(po => po.Id == id);
            if (productOffer == null)
            {
                _logger.LogWarning("Product offer deletion failed - Offer not found - ID: {OfferId} at {Time}", id, DateTime.UtcNow);
                throw new KeyNotFoundException("Product offer not found.");
            }

            // Obter detalhes do produto para o log
            var product = _unitOfWork.Product.Get(p => p.Id == productOffer.ProductId);
            string producerId = product?.ProducerId ?? "unknown";

            // Verificar se existe uma Subscription associada
            var hasSubscriptions = _unitOfWork.Subscription.GetAll(s => s.ProductOfferId == id).Any();
            if (hasSubscriptions)
            {
                _logger.LogWarning("Product offer deletion failed - Has associated subscriptions - OfferId: {OfferId}, ProductId: {ProductId}, ProducerId: {ProducerId} at {Time}",
                    id, productOffer.ProductId, producerId, DateTime.UtcNow);
                throw new InvalidOperationException("Cannot delete product offer because it has associated subscriptions.");
            }

            // Log dados sensíveis antes da exclusão
            _logger.LogInformation("Deleting product offer - ID: {OfferId}, ProductId: {ProductId}, ProducerId: {ProducerId}, Price: {Price}, Quantity: {Quantity}, DeliveryDate: {DeliveryDate} at {Time}",
                productOffer.Id, productOffer.ProductId, producerId, productOffer.Price, productOffer.Quantity, productOffer.DeliveryDate, DateTime.UtcNow);

            _unitOfWork.ProductOffer.Remove(productOffer);
            _unitOfWork.Save();

            // Log de exclusão bem-sucedida
            _logger.LogInformation("Product offer deleted successfully - ID: {OfferId}, ProducerId: {ProducerId} at {Time}", id, producerId, DateTime.UtcNow);
        }
    }
}