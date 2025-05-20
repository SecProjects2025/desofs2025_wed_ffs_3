using AMAP.Entities.Utilities;
using AMAP.Entities;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Business.Interfaces;

namespace AMAP.Business.Components
{
    public class SubscriptionBc : ISubscriptionBc
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionBc(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Subscription CreateSubscription(Subscription subscription, string coProducerId)
        {
            if (subscription == null)
                throw new ArgumentException("Invalid data.");

            // Atribuir o CoProducer autenticado
            subscription.CoProducerId = coProducerId;
            if (subscription.CoProducerId == null)
                throw new UnauthorizedAccessException("Invalid CoProducer.");

            // Obter a oferta de produto
            var productOffer = _unitOfWork.ProductOffer.Get(po => po.Id == subscription.ProductOfferId);
            if (productOffer == null)
                throw new KeyNotFoundException("Product offer not found.");

            // Validar quantidade
            if (subscription.ExpectedQuantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            var orderedQuantity = _unitOfWork.Subscription.GetAll(s => s.ProductOfferId == productOffer.Id)
                .Sum(s => s.ExpectedQuantity);

            if (subscription.ExpectedQuantity + orderedQuantity > productOffer.Quantity)
                throw new InvalidOperationException("Requested quantity exceeds available stock.");

            // Validar se o PaymentType contém apenas bits válidos
            if (subscription.PaymentType == 0 || !Enum.IsDefined(typeof(PaymentType), subscription.PaymentType))
                throw new ArgumentException("Invalid payment type.");

            if (!productOffer.PaymentType.HasFlag(subscription.PaymentType))
                throw new InvalidOperationException("Selected payment type is not supported by the product offer.");

            // Criar subscrição
            _unitOfWork.Subscription.Add(subscription);
            _unitOfWork.Save();

            return subscription;
        }

        public Subscription? GetSubscriptionById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid subscription ID", nameof(id));

            return _unitOfWork.Subscription.Get(s => s.Id == id);
        }

        public IEnumerable<Subscription> GetSubscriptionsByCoProducer(string coProducerId)
        {
            if (string.IsNullOrEmpty(coProducerId))
                throw new ArgumentException("CoProducer ID cannot be empty.", nameof(coProducerId));

            return _unitOfWork.Subscription.GetAll(s => s.CoProducerId == coProducerId);
        }
    }
}
