using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using Microsoft.Extensions.Logging;

namespace AMAP.Business.Components
{
    public class PaymentBc : IPaymentBc
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentBc> _logger;

        public PaymentBc(IUnitOfWork unitOfWork, ILogger<PaymentBc> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public Payment CreatePayment(Payment payment, string currentCoProducerId)
        {
            if (payment is null)
                throw new ArgumentNullException(nameof(payment), "Invalid payment data.");

            if (string.IsNullOrEmpty(currentCoProducerId))
                throw new ArgumentException("CoProducer ID is required.", nameof(currentCoProducerId));

            _logger.LogInformation("Payment creation initiated - SubscriptionId: {SubscriptionId}, CoProducerId: {CoProducerId}, Amount: {Amount} at {Time}",
                payment.SubscriptionId, currentCoProducerId, payment.AmountPaid, DateTime.UtcNow);

            // Verificar se a subscrição associada existe
            var subscription = _unitOfWork.Subscription.Get(s => s.Id == payment.SubscriptionId);
            if (subscription is null)
            {
                _logger.LogWarning("Payment creation failed - Subscription not found - SubscriptionId: {SubscriptionId}, CoProducerId: {CoProducerId} at {Time}",
                    payment.SubscriptionId, currentCoProducerId, DateTime.UtcNow);
                throw new KeyNotFoundException($"Subscription with ID {payment.SubscriptionId} not found.");
            }

            // Verificar se o pagamento é feito pelo CoProducer associado à subscrição
            if (subscription.CoProducerId != currentCoProducerId)
            {
                _logger.LogWarning("Payment creation failed - Unauthorized access - RequestingCoProducerId: {RequestingCoProducerId}, SubscriptionOwnerId: {SubscriptionOwnerId}, SubscriptionId: {SubscriptionId} at {Time}",
                    currentCoProducerId, subscription.CoProducerId, payment.SubscriptionId, DateTime.UtcNow);
                throw new UnauthorizedAccessException("You can only make payments for your own subscriptions.");
            }

            // Definir a data de pagamento para agora
            payment.PaymentDate = DateTime.UtcNow;

            // Log detalhado antes de salvar os dados financeiros sensíveis
            _logger.LogInformation("Processing payment - SubscriptionId: {SubscriptionId}, CoProducerId: {CoProducerId}, Amount: {Amount}, Date: {Date} at {Time}",
                payment.SubscriptionId, currentCoProducerId, payment.AmountPaid, payment.PaymentDate, DateTime.UtcNow);

            // Adicionar e salvar
            _unitOfWork.Payment.Add(payment);
            _unitOfWork.Save();

            _logger.LogInformation("Payment created successfully - ID: {PaymentId}, SubscriptionId: {SubscriptionId}, CoProducerId: {CoProducerId}, Amount: {Amount} at {Time}",
                payment.Id, payment.SubscriptionId, currentCoProducerId, payment.AmountPaid, DateTime.UtcNow);

            return payment;
        }

        public Payment GetPaymentById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid payment ID.", nameof(id));
            return _unitOfWork.Payment.Get(p => p.Id == id);
        }
    }
}
