using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using System.Security.Cryptography;
using System.Text;
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
        private readonly EmailSender _emailSender;
        private readonly ILogger<SubscriptionBc> _logger;

        public SubscriptionBc(IUnitOfWork unitOfWork, EmailSender emailSender, ILogger<SubscriptionBc> logger)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        private readonly ILogger<SubscriptionBc> _logger;

        public SubscriptionBc(IUnitOfWork unitOfWork, ILogger<SubscriptionBc> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public Subscription CreateSubscription(Subscription subscription, string coProducerId)
        {
            if (subscription == null)
                throw new ArgumentException("Invalid subscription data.");

            // Validate ProductOffer
            var productOffer = _unitOfWork.ProductOffer.Get(po => po.Id == subscription.ProductOfferId);
            if (productOffer == null)
                throw new KeyNotFoundException("Product offer not found.");

            // Validate CoProducer
            var coProducer = _unitOfWork.AppUser.Get(u => u.Id == coProducerId);
            if (coProducer == null)
                throw new KeyNotFoundException("Co-producer not found.");

            // Generate confirmation token
            subscription.ConfirmationToken = GenerateConfirmationToken();
            subscription.EmailConfirmed = false;
            subscription.TermsAccepted = false;
            subscription.TermsAcceptedAt = DateTime.UtcNow;

            // Generate digital signature
            subscription.DigitalSignature = GenerateDigitalSignature(subscription);

            _unitOfWork.Subscription.Add(subscription);
            _unitOfWork.Save();

            // Send confirmation email
            SendConfirmationEmail(subscription, coProducer.Email);
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

        public async Task<bool> ConfirmSubscriptionEmail(string token)
        {
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
            var subscription = _unitOfWork.Subscription.Get(s => s.Id == subscriptionId);
            if (subscription == null)
                return false;

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

        private async Task SendConfirmationEmail(Subscription subscription, string email)
        {
            try
            {
                var subject = "Confirm Your AMAP Subscription";
                var confirmationUrl = $"http://localhost:3000/confirm-subscription?token={subscription.ConfirmationToken}";

                var body = $@"
                    <h2>Welcome to AMAP!</h2>
                    <p>Dear Co-producer,</p>
                    <p>Please confirm your subscription by clicking the link below:</p>
                    <p><a href='{confirmationUrl}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Confirm Subscription</a></p>
                    
                    <h3>Subscription Details:</h3>
                    <ul>
                        <li>Product Offer ID: {subscription.ProductOfferId}</li>
                        <li>Expected Quantity: {subscription.ExpectedQuantity}</li>
                        <li>Payment Type: {subscription.PaymentType}</li>
                    </ul>
                    
                    <p><strong>Confirmation Token:</strong> {subscription.ConfirmationToken}</p>
                    
                    <p>By confirming this subscription, you agree to our terms and conditions.</p>
                    <p>If you did not create this subscription, please ignore this email.</p>
                    
                    <hr>
                    <p style='color: #000000; font-size: 12px;'>This is an automated message, please do not reply.</p>
                ";

                _logger.LogInformation($"Sending confirmation email to {email} with token: {subscription.ConfirmationToken}");
                await _emailSender.SendEmailAsync(email, subject, body);
                _logger.LogInformation($"Confirmation email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending confirmation email: {ex.Message}");
                throw;
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