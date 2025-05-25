using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using Microsoft.Extensions.Logging;

namespace AMAP.Business.Components
{
    public class DeliveryBc : IDeliveryBc
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeliveryBc> _logger;

        public DeliveryBc(IUnitOfWork unitOfWork, ILogger<DeliveryBc> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public void UpdateDeliveredQuantity(int subscriptionId, int deliveredQuantity, string currentProducerId)
        {
            if (deliveredQuantity < 0)
            {
                _logger.LogWarning("Delivery update failed - Negative quantity - SubscriptionId: {SubscriptionId}, Quantity: {Quantity}, ProducerId: {ProducerId} at {Time}",
                    subscriptionId, deliveredQuantity, currentProducerId, DateTime.UtcNow);
                throw new ArgumentException("Delivered quantity cannot be negative.");
            }

            // Log no início da operação de atualização
            _logger.LogInformation("Delivery quantity update initiated - SubscriptionId: {SubscriptionId}, Quantity: {Quantity}, ProducerId: {ProducerId} at {Time}",
                subscriptionId, deliveredQuantity, currentProducerId, DateTime.UtcNow);

            var subscription = _unitOfWork.Subscription.Get(
                s => s.Id == subscriptionId,
                includeProperties: "ProductOffer.Product"
            );

            if (subscription == null)
            {
                _logger.LogWarning("Delivery update failed - Subscription not found - SubscriptionId: {SubscriptionId}, ProducerId: {ProducerId} at {Time}",
                    subscriptionId, currentProducerId, DateTime.UtcNow);
                throw new KeyNotFoundException("Subscription not found.");
            }

            // Log do acesso aos dados da subscrição
            _logger.LogInformation("Subscription data accessed - ID: {SubscriptionId}, CoProducerId: {CoProducerId}, ProductOfferId: {ProductOfferId} at {Time}",
                subscription.Id, subscription.CoProducerId, subscription.ProductOfferId, DateTime.UtcNow);

            // Verificar se o produtor atual tem permissão
            if (subscription.ProductOffer == null ||
                subscription.ProductOffer.Product.ProducerId != currentProducerId)
            {
                _logger.LogWarning("Delivery update failed - Unauthorized access - RequestingProducerId: {RequestingProducerId}, OwnerProducerId: {OwnerProducerId}, SubscriptionId: {SubscriptionId} at {Time}",
                    currentProducerId, subscription.ProductOffer?.Product.ProducerId, subscriptionId, DateTime.UtcNow);
                throw new UnauthorizedAccessException("You are not allowed to update this subscription.");
            }

            // Buscar ou criar entrega
            var existingDelivery = _unitOfWork.Delivery.Get(d => d.SubscriptionId == subscriptionId);

            // Log dos dados de entrega existentes (se houver)
            if (existingDelivery != null)
            {
                _logger.LogInformation("Existing delivery data accessed - ID: {DeliveryId}, SubscriptionId: {SubscriptionId}, OldQuantity: {OldQuantity}, NewQuantity: {NewQuantity}, ProducerId: {ProducerId} at {Time}",
                    existingDelivery.Id, subscriptionId, existingDelivery.DeliveredQuantity, deliveredQuantity, currentProducerId, DateTime.UtcNow);
            }

            if (existingDelivery == null)
            {
                var newDelivery = new Delivery
                {
                    SubscriptionId = subscriptionId,
                    DeliveredQuantity = deliveredQuantity
                };
                _unitOfWork.Delivery.Add(newDelivery);

                // Log de criação de nova entrega
                _logger.LogInformation("New delivery created - SubscriptionId: {SubscriptionId}, Quantity: {Quantity}, ProducerId: {ProducerId} at {Time}",
                    subscriptionId, deliveredQuantity, currentProducerId, DateTime.UtcNow);
            }
            else
            {
                existingDelivery.DeliveredQuantity = deliveredQuantity;
                _unitOfWork.Delivery.Update(existingDelivery);

                // Log de atualização de entrega existente
                _logger.LogInformation("Delivery updated - ID: {DeliveryId}, SubscriptionId: {SubscriptionId}, NewQuantity: {Quantity}, ProducerId: {ProducerId} at {Time}",
                    existingDelivery.Id, subscriptionId, deliveredQuantity, currentProducerId, DateTime.UtcNow);
            }

            _unitOfWork.Save();

            // Log de sucesso na operação de atualização
            _logger.LogInformation("Delivery quantity updated successfully - SubscriptionId: {SubscriptionId}, Quantity: {Quantity}, ProducerId: {ProducerId} at {Time}",
                subscriptionId, deliveredQuantity, currentProducerId, DateTime.UtcNow);
        }
    }
}