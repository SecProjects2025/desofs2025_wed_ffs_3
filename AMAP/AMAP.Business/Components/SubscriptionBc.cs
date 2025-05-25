using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AMAP.Business.Components
{
    public class SubscriptionBc : ISubscriptionBc
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailSender _emailSender;
        private readonly ILogger<SubscriptionBc> _logger;

        public SubscriptionBc(IUnitOfWork unitOfWork, EmailSender emailSender, ILogger<SubscriptionBc> logger)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _logger = logger;
        }

        public Subscription CreateSubscription(Subscription subscription, string coProducerId)
        {
            if (subscription == null)
            {
                throw new ArgumentException("Invalid data.");
            }

            if (string.IsNullOrEmpty(coProducerId))
            {
                throw new UnauthorizedAccessException("Invalid CoProducer.");
            }

            if (subscription.ExpectedQuantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero.");
            }

            if (!Enum.IsDefined(typeof(PaymentType), subscription.PaymentType) || subscription.PaymentType == 0)
            {
                throw new ArgumentException("Invalid payment type.");
            }

            var productOffer = _unitOfWork.ProductOffer.Get(po => po.Id == subscription.ProductOfferId);
            if (productOffer == null)
            {
                throw new KeyNotFoundException("Product offer not found.");
            }

            if (productOffer.SubscriptionPeriod?.EndDate < DateTime.Now)
            {
                throw new InvalidOperationException("Subscription period has ended.");
            }

            var existingSubscriptions = _unitOfWork.Subscription.GetAll(s => s.ProductOfferId == subscription.ProductOfferId && s.CoProducerId == coProducerId);
            var totalSubscribedQuantity = existingSubscriptions?.Sum(s => s.ExpectedQuantity) ?? 0;

            if (totalSubscribedQuantity + subscription.ExpectedQuantity > productOffer.Quantity)
            {
                throw new InvalidOperationException("Requested quantity exceeds available stock.");
            }

            subscription.CoProducerId = coProducerId;
            subscription.EmailConfirmed = false;
            subscription.TermsAccepted = true;
            subscription.ConfirmationToken = GenerateConfirmationToken();

            _unitOfWork.Subscription.Add(subscription);

            try
            {
                SendConfirmationEmail(subscription, coProducerId).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending confirmation email: {ex.Message}");
                // Não lançamos a exceção aqui para permitir que a subscrição seja criada mesmo se o email falhar
            }

            _unitOfWork.Save();
            return subscription;
        }

        public async Task<bool> ConfirmSubscriptionEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var subscription = _unitOfWork.Subscription.Get(s => s.ConfirmationToken == token);
            if (subscription == null)
                return false;

            subscription.EmailConfirmed = true;
            subscription.EmailConfirmedAt = DateTime.UtcNow;
            _unitOfWork.Save();

            return true;
        }

        public async Task<bool> AcceptTerms(int subscriptionId)
        {
            if (subscriptionId <= 0)
            {
                throw new ArgumentException("Invalid subscription ID.");
            }

            var subscription = _unitOfWork.Subscription.Get(s => s.Id == subscriptionId);
            if (subscription == null)
                return false;

            if (!subscription.EmailConfirmed)
            {
                throw new InvalidOperationException("Email must be confirmed before accepting terms.");
            }

            subscription.TermsAccepted = true;
            subscription.TermsAcceptedAt = DateTime.UtcNow;
            _unitOfWork.Save();

            return true;
        }

        private string GenerateConfirmationToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }

        private string GenerateDigitalSignature(Subscription subscription)
        {
            var data = $"{subscription.Id}{subscription.CoProducerId}{subscription.ProductOfferId}{subscription.ExpectedQuantity}{subscription.PaymentType}{DateTime.UtcNow}";
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }

        private async Task SendConfirmationEmail(Subscription subscription, string coProducerId)
        {
            try
            {
                var subject = "Confirm Your Subscription";
                var body = $"Please confirm your subscription by clicking the following link: {subscription.ConfirmationToken}";
                await _emailSender.SendEmailAsync(coProducerId, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending confirmation email: {ex.Message}");
                throw;
            }
        }

        public Subscription GetSubscriptionById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid subscription ID (Parameter 'id')");
            return _unitOfWork.Subscription.Get(s => s.Id == id);
        }

        public IEnumerable<Subscription> GetSubscriptionsByCoProducer(string coProducerId)
        {
            if (string.IsNullOrEmpty(coProducerId))
                throw new ArgumentException("CoProducer ID cannot be empty. (Parameter 'coProducerId')");
            return _unitOfWork.Subscription.GetAll(s => s.CoProducerId == coProducerId);
        }
    }
}