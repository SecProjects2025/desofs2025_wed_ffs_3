using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;

namespace AMAP.Business.Components
{
    public class PaymentBc : IPaymentBc
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentBc(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Payment CreatePayment(Payment payment, string currentCoProducerId)
        {
            if (payment is null)
                throw new ArgumentNullException(nameof(payment), "Invalid payment data.");

            if (string.IsNullOrEmpty(currentCoProducerId))
                throw new ArgumentException("CoProducer ID is required.", nameof(currentCoProducerId));

            // Verificar se a subscrição associada existe
            var subscription = _unitOfWork.Subscription.Get(s => s.Id == payment.SubscriptionId);
            if (subscription is null)
                throw new KeyNotFoundException($"Subscription with ID {payment.SubscriptionId} not found.");

            // Verificar se o pagamento é feito pelo CoProducer associado à subscrição
            if (subscription.CoProducerId != currentCoProducerId)
                throw new UnauthorizedAccessException("You can only make payments for your own subscriptions.");

            // Definir a data de pagamento para agora
            payment.PaymentDate = DateTime.UtcNow;

            // Adicionar e salvar
            _unitOfWork.Payment.Add(payment);
            _unitOfWork.Save();

            return payment;
        }
    }
}
