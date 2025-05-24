using AMAP.Entities.Utilities;
using AMAP.Entities;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Business.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMAP.Business.Components
{
    public class SubscriptionBc : ISubscriptionBc
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SubscriptionBc> _logger;

        public SubscriptionBc(IUnitOfWork unitOfWork, ILogger<SubscriptionBc> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public Subscription CreateSubscription(Subscription subscription, string coProducerId)
        {
            if (subscription == null)
                throw new ArgumentException("Invalid data.");

            // Log no início da operação de criação de subscrição
            _logger.LogInformation("Subscription creation initiated - CoProducerId: {CoProducerId}, ProductOfferId: {ProductOfferId} at {Time}",
                coProducerId, subscription.ProductOfferId, DateTime.UtcNow);

            // Atribuir o CoProducer autenticado
            subscription.CoProducerId = coProducerId;
            if (subscription.CoProducerId == null)
            {
                _logger.LogWarning("Subscription creation failed - Invalid CoProducer authentication at {Time}", DateTime.UtcNow);
                throw new UnauthorizedAccessException("Invalid CoProducer.");
            }

            // Obter a oferta de produto
            var productOffer = _unitOfWork.ProductOffer.Get(po => po.Id == subscription.ProductOfferId);
            if (productOffer == null)
            {
                _logger.LogWarning("Subscription creation failed - Product offer not found - ID: {ProductOfferId} at {Time}",
                    subscription.ProductOfferId, DateTime.UtcNow);
                throw new KeyNotFoundException("Product offer not found.");
            }

            // Validar quantidade
            if (subscription.ExpectedQuantity <= 0)
            {
                _logger.LogWarning("Subscription creation failed - Invalid quantity: {Quantity} at {Time}",
                    subscription.ExpectedQuantity, DateTime.UtcNow);
                throw new ArgumentException("Quantity must be greater than zero.");
            }

            var orderedQuantity = _unitOfWork.Subscription.GetAll(s => s.ProductOfferId == productOffer.Id)
                .Sum(s => s.ExpectedQuantity);

            if (subscription.ExpectedQuantity + orderedQuantity > productOffer.Quantity)
            {
                _logger.LogWarning("Subscription creation failed - Requested quantity exceeds stock - Requested: {Requested}, Available: {Available} at {Time}",
                    subscription.ExpectedQuantity, productOffer.Quantity - orderedQuantity, DateTime.UtcNow);
                throw new InvalidOperationException("Requested quantity exceeds available stock.");
            }

            // Validar se o PaymentType contém apenas bits válidos
            if (subscription.PaymentType == 0 || !Enum.IsDefined(typeof(PaymentType), subscription.PaymentType))
            {
                _logger.LogWarning("Subscription creation failed - Invalid payment type: {PaymentType} at {Time}",
                    subscription.PaymentType, DateTime.UtcNow);
                throw new ArgumentException("Invalid payment type.");
            }

            if (!productOffer.PaymentType.HasFlag(subscription.PaymentType))
            {
                _logger.LogWarning("Subscription creation failed - Payment type not supported - Selected: {Selected}, Supported: {Supported} at {Time}",
                    subscription.PaymentType, productOffer.PaymentType, DateTime.UtcNow);
                throw new InvalidOperationException("Selected payment type is not supported by the product offer.");
            }

            // Criar subscrição
            _unitOfWork.Subscription.Add(subscription);
            _unitOfWork.Save();

            // Log de sucesso na criação da subscrição
            _logger.LogInformation("Subscription created successfully - ID: {SubscriptionId}, CoProducerId: {CoProducerId}, ProductOfferId: {ProductOfferId}, Quantity: {Quantity} at {Time}",
                subscription.Id, subscription.CoProducerId, subscription.ProductOfferId, subscription.ExpectedQuantity, DateTime.UtcNow);

            return subscription;
        }

        public Subscription? GetSubscriptionById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid subscription ID", nameof(id));

            var subscription = _unitOfWork.Subscription.Get(s => s.Id == id);

            // Log de acesso aos dados da subscrição
            if (subscription != null)
            {
                _logger.LogInformation("Subscription data accessed - ID: {SubscriptionId}, CoProducerId: {CoProducerId}, ProductOfferId: {ProductOfferId} at {Time}",
                    subscription.Id, subscription.CoProducerId, subscription.ProductOfferId, DateTime.UtcNow);
            }
            else
            {
                _logger.LogInformation("Subscription data access attempted but not found - ID: {SubscriptionId} at {Time}", id, DateTime.UtcNow);
            }

            return subscription;
        }

        public IEnumerable<Subscription> GetSubscriptionsByCoProducer(string coProducerId)
        {
            if (string.IsNullOrEmpty(coProducerId))
            {
                _logger.LogWarning("Subscription data access failed - Empty CoProducer ID provided at {Time}", DateTime.UtcNow);
                throw new ArgumentException("CoProducer ID cannot be empty.", nameof(coProducerId));
            }

            var subscriptions = _unitOfWork.Subscription.GetAll(s => s.CoProducerId == coProducerId).ToList();

            // Log de acesso aos dados das subscrições do co-produtor
            _logger.LogInformation("Co-Producer subscriptions data accessed - CoProducerId: {CoProducerId}, SubscriptionCount: {Count} at {Time}",
                coProducerId, subscriptions.Count, DateTime.UtcNow);

            return subscriptions;
        }
    }
}