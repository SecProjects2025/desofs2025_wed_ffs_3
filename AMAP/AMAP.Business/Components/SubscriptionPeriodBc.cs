using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.Extensions.Logging;

namespace AMAP.Business.Components
{
    public class SubscriptionPeriodBc : ISubscriptionPeriodBc
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailSender _emailSender;
        private readonly ILogger<SubscriptionPeriodBc> _logger;

        public SubscriptionPeriodBc(IUnitOfWork unitOfWork, EmailSender emailSender, ILogger<SubscriptionPeriodBc> logger)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _logger = logger;
        }

        public SubscriptionPeriod? GetSubscriptionPeriodById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid subscription period id.", nameof(id));

            var subscriptionPeriod = _unitOfWork.SubscriptionPeriod.Get(p => p.Id == id, includeProperties: "DeliveryDates");

            // Log de acesso aos dados do período de subscrição
            if (subscriptionPeriod != null)
            {
                _logger.LogInformation("Subscription period data accessed - ID: {PeriodId}, Duration: {Duration}, StartDate: {StartDate}, EndDate: {EndDate} at {Time}",
                    subscriptionPeriod.Id, subscriptionPeriod.Duration, subscriptionPeriod.StartDate, subscriptionPeriod.EndDate, DateTime.UtcNow);
            }

            return subscriptionPeriod;
        }

        public DeliveryDate? GetDeliveryDateById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid delivery date id.", nameof(id));

            var deliveryDate = _unitOfWork.DeliveryDate.Get(p => p.Id == id);

            // Log de acesso aos dados da data de entrega
            if (deliveryDate != null)
            {
                _logger.LogInformation("Delivery date data accessed - ID: {DeliveryDateId}, Date: {Date}, SubscriptionPeriodId: {PeriodId} at {Time}",
                    deliveryDate.Id, deliveryDate.Date, deliveryDate.SubscriptionPeriodId, DateTime.UtcNow);
            }

            return deliveryDate;
        }

        public async Task<SubscriptionPeriod> CreateSubscriptionPeriodAndDeliveryDateAsync(SubscriptionPeriod subscriptionPeriod)
        {
            if (subscriptionPeriod == null)
                throw new ArgumentException("Invalid subscription period data.");

            if (subscriptionPeriod.StartDate > subscriptionPeriod.EndDate)
                throw new ArgumentException("Invalid subscription period dates.");

            // Log de início da operação de criação
            _logger.LogInformation("Subscription period creation initiated - Duration: {Duration}, StartDate: {StartDate}, EndDate: {EndDate} at {Time}",
                subscriptionPeriod.Duration, subscriptionPeriod.StartDate, subscriptionPeriod.EndDate, DateTime.UtcNow);

            var existingPeriod = _unitOfWork.SubscriptionPeriod
                .GetAll(sp => sp.StartDate == subscriptionPeriod.StartDate && sp.EndDate == subscriptionPeriod.EndDate)
                .FirstOrDefault();

            if (existingPeriod != null)
            {
                _logger.LogWarning("Subscription period creation failed - A period with the same dates already exists - StartDate: {StartDate}, EndDate: {EndDate} at {Time}",
                    subscriptionPeriod.StartDate, subscriptionPeriod.EndDate, DateTime.UtcNow);
                throw new InvalidOperationException("A subscription period with the same start and end date already exists.");
            }

            if (subscriptionPeriod.DeliveryDates == null || !subscriptionPeriod.DeliveryDates.Any())
            {
                _logger.LogWarning("Subscription period creation failed - No delivery dates provided at {Time}", DateTime.UtcNow);
                throw new ArgumentException("Delivery dates are required.");
            }

            var duplicateDates = subscriptionPeriod.DeliveryDates
                .GroupBy(d => d.Date)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateDates.Count != 0)
            {
                _logger.LogWarning("Subscription period creation failed - Duplicate delivery dates found: {DuplicateDates} at {Time}",
                    string.Join(", ", duplicateDates), DateTime.UtcNow);
                throw new ArgumentException($"Duplicate delivery dates found: {string.Join(", ", duplicateDates)}");
            }

            foreach (var deliveryDate in subscriptionPeriod.DeliveryDates)
            {
                if (deliveryDate.Date < subscriptionPeriod.StartDate || deliveryDate.Date > subscriptionPeriod.EndDate)
                {
                    _logger.LogWarning("Subscription period creation failed - Delivery date {DeliveryDate} is not within the subscription period at {Time}",
                        deliveryDate.Date, DateTime.UtcNow);
                    throw new ArgumentException($"Delivery date {deliveryDate.Date} is not within the subscription period.");
                }

                deliveryDate.SubscriptionPeriod = subscriptionPeriod;
            }

            _unitOfWork.SubscriptionPeriod.Add(subscriptionPeriod);
            _unitOfWork.Save();

            // Log de sucesso na criação do período de subscrição
            _logger.LogInformation("Subscription period created successfully - ID: {PeriodId}, Duration: {Duration}, DeliveryDatesCount: {DeliveryDatesCount} at {Time}",
                subscriptionPeriod.Id, subscriptionPeriod.Duration, subscriptionPeriod.DeliveryDates.Count(), DateTime.UtcNow);

            // Notificar produtores
            var producers = _unitOfWork.AppUser.GetAll(u => u.UserType == UserType.Producer).ToList();
            if (producers.Count != 0)
            {
                _logger.LogInformation("Sending subscription period creation notifications to {ProducerCount} producers at {Time}",
                    producers.Count, DateTime.UtcNow);

                var tasks = producers.Select(producer =>
                {
                    if (string.IsNullOrEmpty(producer.Email))
                        return Task.CompletedTask;

                    var subject = "New Subscription Period Created";
                    var body = $@"
                            <p>Dear {producer.Name},</p>
                            <p>A new subscription period has been created with the following details</p>
                            <ul>
                                <li>Duration: {subscriptionPeriod.Duration} </li>
                                <li>Start Date: {subscriptionPeriod.StartDate:yyyy-MM-dd HH:mm:ss}</li>
                                <li>End Date: {subscriptionPeriod.EndDate:yyyy-MM-dd HH:mm:ss}</li>
                            </ul>
                            <p>Here are the new delivery dates:</p>
                            <ul>
                        ";
                    foreach (var deliveryDate in subscriptionPeriod.DeliveryDates)
                    {
                        body += $@"<li>{deliveryDate.Date:yyyy-MM-dd HH:mm:ss}</li>";
                    }

                    body += @"
                        </ul>
                        <p>Please review the updated delivery dates in your dashboard.</p>
                        ";

                    return _emailSender.SendEmailAsync(producer.Email, subject, body);
                });

                try
                {
                    await Task.WhenAll(tasks);
                    _logger.LogInformation("Email notifications sent successfully for subscription period ID: {PeriodId} at {Time}",
                        subscriptionPeriod.Id, DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email notification failed for subscription period ID: {PeriodId} at {Time}",
                        subscriptionPeriod.Id, DateTime.UtcNow);
                    Console.WriteLine($"Email notification failed: {ex.Message}");
                }
            }

            return subscriptionPeriod;
        }

        public async Task DeleteSubscriptionPeriodAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid subscription period id.", nameof(id));

            // Log de início da operação de exclusão
            _logger.LogInformation("Subscription period deletion initiated - ID: {PeriodId} at {Time}", id, DateTime.UtcNow);

            var subscriptionPeriod = _unitOfWork.SubscriptionPeriod.Get(p => p.Id == id);
            if (subscriptionPeriod == null)
            {
                _logger.LogWarning("Subscription period deletion failed - Period not found - ID: {PeriodId} at {Time}", id, DateTime.UtcNow);
                throw new KeyNotFoundException("This Subscription Period does not exist");
            }

            // Log de exclusão dos dados sensíveis
            _logger.LogInformation("Deleting subscription period - ID: {PeriodId}, Duration: {Duration}, StartDate: {StartDate}, EndDate: {EndDate} at {Time}",
                subscriptionPeriod.Id, subscriptionPeriod.Duration, subscriptionPeriod.StartDate, subscriptionPeriod.EndDate, DateTime.UtcNow);

            _unitOfWork.SubscriptionPeriod.Remove(subscriptionPeriod);
            _unitOfWork.Save();

            // Log de sucesso na exclusão do período de subscrição
            _logger.LogInformation("Subscription period deleted successfully - ID: {PeriodId} at {Time}", id, DateTime.UtcNow);

            // Notificar produtores
            var producers = _unitOfWork.AppUser.GetAll(u => u.UserType == UserType.Producer).ToList();
            if (producers.Count != 0)
            {
                _logger.LogInformation("Sending subscription period deletion notifications to {ProducerCount} producers at {Time}",
                    producers.Count, DateTime.UtcNow);

                var tasks = producers.Select(producer =>
                {
                    if (string.IsNullOrEmpty(producer.Email))
                        return Task.CompletedTask;

                    var subject = "Subscription Period Deleted";
                    var body = $@"
                        <p>Dear {producer.Name},</p>
                        <p>A subscription period has been deleted with the following details:</p>
                        <ul>
                            <li>Duration: {subscriptionPeriod.Duration} </li>
                        </ul>
                        <p>Please review your dashboard for updated information.</p>
                    ";

                    return _emailSender.SendEmailAsync(producer.Email, subject, body);
                });

                try
                {
                    await Task.WhenAll(tasks);
                    _logger.LogInformation("Email notifications sent successfully for deleted subscription period ID: {PeriodId} at {Time}",
                        id, DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email notification failed for deleted subscription period ID: {PeriodId} at {Time}",
                        id, DateTime.UtcNow);
                    Console.WriteLine($"Email notification failed: {ex.Message}");
                }
            }
        }
    }
}