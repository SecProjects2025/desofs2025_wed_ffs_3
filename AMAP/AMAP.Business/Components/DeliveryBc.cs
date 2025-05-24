using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;

namespace AMAP.Business.Components
{
    public class DeliveryBc : IDeliveryBc
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeliveryBc(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void UpdateDeliveredQuantity(int subscriptionId, int deliveredQuantity, string currentProducerId)
        {
            if (deliveredQuantity < 0)
                throw new ArgumentException("Delivered quantity cannot be negative.");

            var subscription = _unitOfWork.Subscription.Get(
                s => s.Id == subscriptionId,
                includeProperties: "ProductOffer.Product"
            );

            if (subscription == null)
                throw new KeyNotFoundException("Subscription not found.");

            // Verificar se o produtor atual tem permissão
            if (subscription.ProductOffer == null ||
                subscription.ProductOffer.Product.ProducerId != currentProducerId)
                throw new UnauthorizedAccessException("You are not allowed to update this subscription.");

            // Buscar ou criar entrega
            var existingDelivery = _unitOfWork.Delivery.Get(d => d.SubscriptionId == subscriptionId);
            if (existingDelivery == null)
            {
                var newDelivery = new Delivery
                {
                    SubscriptionId = subscriptionId,
                    DeliveredQuantity = deliveredQuantity
                };
                _unitOfWork.Delivery.Add(newDelivery);
            }
            else
            {
                existingDelivery.DeliveredQuantity = deliveredQuantity;
                _unitOfWork.Delivery.Update(existingDelivery);
            }

            _unitOfWork.Save();
        }
    }
}

