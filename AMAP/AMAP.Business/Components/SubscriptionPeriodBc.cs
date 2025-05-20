using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;

namespace AMAP.Business.Components
{
    public class SubscriptionPeriodBc : ISubscriptionPeriodBc
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailSender _emailSender;

        public SubscriptionPeriodBc(IUnitOfWork unitOfWork, EmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }

        public SubscriptionPeriod? GetSubscriptionPeriodById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid subscription period id.", nameof(id));

            return _unitOfWork.SubscriptionPeriod.Get(p => p.Id == id, includeProperties: "DeliveryDates");
        }

        public DeliveryDate? GetDeliveryDateById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid delivery date id.", nameof(id));

            return _unitOfWork.DeliveryDate.Get(p => p.Id == id);
        }

        public async Task<SubscriptionPeriod> CreateSubscriptionPeriodAndDeliveryDateAsync(SubscriptionPeriod subscriptionPeriod)
        {
            if (subscriptionPeriod == null)
                throw new ArgumentException("Invalid subscription period data.");

            if (subscriptionPeriod.StartDate > subscriptionPeriod.EndDate)
                throw new ArgumentException("Invalid subscription period dates.");

            var existingPeriod = _unitOfWork.SubscriptionPeriod
                .GetAll(sp => sp.StartDate == subscriptionPeriod.StartDate && sp.EndDate == subscriptionPeriod.EndDate)
                .FirstOrDefault();

            if (existingPeriod != null)
                throw new InvalidOperationException("A subscription period with the same start and end date already exists.");

            if (subscriptionPeriod.DeliveryDates == null || !subscriptionPeriod.DeliveryDates.Any())
                throw new ArgumentException("Delivery dates are required.");

            var duplicateDates = subscriptionPeriod.DeliveryDates
                .GroupBy(d => d.Date)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateDates.Count != 0)
                throw new ArgumentException($"Duplicate delivery dates found: {string.Join(", ", duplicateDates)}");

            foreach (var deliveryDate in subscriptionPeriod.DeliveryDates)
            {
                if (deliveryDate.Date < subscriptionPeriod.StartDate || deliveryDate.Date > subscriptionPeriod.EndDate)
                    throw new ArgumentException($"Delivery date {deliveryDate.Date} is not within the subscription period.");

                deliveryDate.SubscriptionPeriod = subscriptionPeriod;
            }

            _unitOfWork.SubscriptionPeriod.Add(subscriptionPeriod);
            _unitOfWork.Save();

            // Notificar produtores
            var producers = _unitOfWork.AppUser.GetAll(u => u.UserType == UserType.Producer).ToList();
            if (producers.Count != 0)
            {
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email notification failed: {ex.Message}");
                }
            }

            return subscriptionPeriod;
        }

        public async Task DeleteSubscriptionPeriodAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid subscription period id.", nameof(id));

            var subscriptionPeriod = _unitOfWork.SubscriptionPeriod.Get(p => p.Id == id);
            if (subscriptionPeriod == null)
                throw new KeyNotFoundException("This Subscription Period does not exist");

            _unitOfWork.SubscriptionPeriod.Remove(subscriptionPeriod);
            _unitOfWork.Save();

            // Notificar produtores
            var producers = _unitOfWork.AppUser.GetAll(u => u.UserType == UserType.Producer).ToList();
            if (producers.Count != 0)
            {
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email notification failed: {ex.Message}");
                }
            }
        }
    }
}
