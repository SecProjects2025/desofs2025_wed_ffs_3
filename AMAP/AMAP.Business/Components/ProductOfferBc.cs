using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;

namespace AMAP.Business.Components
{
    public class ProductOfferBc : IProductOfferBc
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductOfferBc(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ProductOffer CreateProductOffer(ProductOffer productOffer, string producerId)
        {
            if (productOffer == null)
                throw new ArgumentException("Invalid data.");

            // Obter o produto
            var product = _unitOfWork.Product.Get(p => p.Id == productOffer.ProductId);
            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            // Verificar se o produto pertence ao produtor autenticado
            if (string.IsNullOrEmpty(producerId) || product.ProducerId != producerId)
                throw new InvalidOperationException("You cannot create an offer for a product from another producer.");

            // Validate SubscriptionPeriodId
            var subscriptionPeriod = _unitOfWork.SubscriptionPeriod.Get(sp => sp.Id == productOffer.SubscriptionPeriodId);
            if (subscriptionPeriod == null)
                throw new KeyNotFoundException("Subscription period not found.");

            // Validate DeliveryDate
            if (productOffer.DeliveryDate < DateTime.Now)
                throw new InvalidOperationException("Delivery date cannot be in the past.");

            // Verificar se já existe uma oferta para o mesmo produto e mesma data de entrega
            var existingOffer = _unitOfWork.ProductOffer.GetAll(po =>
                po.ProductId == productOffer.ProductId &&
                po.DeliveryDate == productOffer.DeliveryDate).FirstOrDefault();

            if (existingOffer != null)
                throw new InvalidOperationException("A product offer already exists for this product on the selected delivery date.");

            // Obter datas de entrega associadas ao período de subscrição
            subscriptionPeriod.DeliveryDates = _unitOfWork.DeliveryDate.GetAll(d =>
                d.SubscriptionPeriodId == productOffer.SubscriptionPeriodId);

            // Verificar se a data de entrega está dentro do período de subscrição
            if (!subscriptionPeriod.DeliveryDates.Select(dd => dd.Date).Contains(productOffer.DeliveryDate))
                throw new InvalidOperationException("Delivery date outside subscription period.");

            // Validar quantidade
            if (productOffer.Quantity <= 0)
                throw new ArgumentException("Product offer quantity must be greater than zero.");

            // Validar tipo de pagamento
            if (productOffer.PaymentType <= 0 ||
                (int)productOffer.PaymentType > Enum.GetValues<PaymentType>().Cast<int>().Sum())
                throw new ArgumentException("Invalid payment type.");

            _unitOfWork.ProductOffer.Add(productOffer);
            _unitOfWork.Save();

            return productOffer;
        }

        public ProductOffer? GetProductOfferById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid product offer ID.", nameof(id));

            return _unitOfWork.ProductOffer.Get(po => po.Id == id);
        }

        public ProductOffer? GetProductOfferByProducer(string producerId)
        {
            if (string.IsNullOrEmpty(producerId))
                throw new ArgumentException("Invalid producer ID.", nameof(producerId));

            return _unitOfWork.ProductOffer.Get(po => po.Product.ProducerId == producerId);
        }

        public void DeleteProductOffer(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid product offer ID.", nameof(id));

            var productOffer = _unitOfWork.ProductOffer.Get(po => po.Id == id);
            if (productOffer == null)
                throw new KeyNotFoundException("Product offer not found.");

            // Verificar se existe uma Subscription associada
            var hasSubscriptions = _unitOfWork.Subscription.GetAll(s => s.ProductOfferId == id).Any();
            if (hasSubscriptions)
                throw new InvalidOperationException("Cannot delete product offer because it has associated subscriptions.");

            _unitOfWork.ProductOffer.Remove(productOffer);
            _unitOfWork.Save();
        }
    }
}
