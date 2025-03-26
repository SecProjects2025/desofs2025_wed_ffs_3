using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMAP.Api.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionPeriodController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailSender _emailSender;

        public SubscriptionPeriodController(IUnitOfWork unitOfWork, EmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }

        [HttpGet("SubscriptionPeriod/{id}")]
        public IActionResult GetSubscriptionPeriodById(int id)
        {
            var subscriptionPeriod = _unitOfWork.SubscriptionPeriod.Get(p => p.Id == id, includeProperties: "DeliveryDates");
            if (subscriptionPeriod == null)
            {
                return NotFound(new { message = "This Subscription Period does not exist" });
            }
            return Ok(subscriptionPeriod);
        }

        [HttpGet("DeliveryDate/{id}")]
        public IActionResult GetDeliveryDateById(int id)
        {
            var deliveryDate = _unitOfWork.DeliveryDate.Get(p => p.Id == id);
            if (deliveryDate == null)
            {
                return NotFound(new { message = "This Delivery Date does not exist" });
            }
            return Ok(deliveryDate);
        }

        [Authorize(Roles = nameof(UserType.Manager))]
        [HttpPost("SubscriptionPeriod")]
        public async Task<IActionResult> CreateSubscriptionPeriodAndDeliveryDate([FromBody] SubscriptionPeriod subscriptionPeriod)
        {
            if (subscriptionPeriod == null)
            {
                return BadRequest(new { message = "Invalid subscription period data" });
            }

            if (subscriptionPeriod.StartDate > subscriptionPeriod.EndDate)
            {
                return BadRequest(new { message = "Invalid subscription period dates" });
            }

            var existingPeriod = _unitOfWork.SubscriptionPeriod
                .GetAll(sp => sp.StartDate == subscriptionPeriod.StartDate && sp.EndDate == subscriptionPeriod.EndDate)
                .FirstOrDefault();

            if (existingPeriod != null)
            {
                return BadRequest(new { message = "A subscription period with the same start and end date already exists" });
            }

            if (subscriptionPeriod.DeliveryDates == null || subscriptionPeriod.DeliveryDates.Count() == 0)
            {
                return BadRequest(new { message = "Delivery dates are required" });
            }

            // Verificar duplicados no modelo recebido
            var duplicateDates = subscriptionPeriod.DeliveryDates
                .GroupBy(d => d.Date)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateDates.Count != 0)
            {
                return BadRequest(new { message = $"Duplicate delivery dates found: {string.Join(", ", duplicateDates)}" });
            }

            foreach (var deliveryDate in subscriptionPeriod.DeliveryDates)
            {
                if (deliveryDate.Date < subscriptionPeriod.StartDate || deliveryDate.Date > subscriptionPeriod.EndDate)
                {
                    return BadRequest(new { message = $"Delivery date {deliveryDate.Date} is not within the subscription period" });
                }

                deliveryDate.SubscriptionPeriod = subscriptionPeriod;
            }

            _unitOfWork.SubscriptionPeriod.Add(subscriptionPeriod);
            _unitOfWork.Save();

            // Obter todos os utilizadores com a role Producer
            var producers = _unitOfWork.AppUser.GetAll(u => u.UserType == UserType.Producer).ToList();

            if (producers.Count != 0)
            {
                var tasks = producers.Select(producer =>
                {
                    if (string.IsNullOrEmpty(producer.Email))
                    {
                        return Task.CompletedTask;
                    }

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
                        body += $@"
                            <li>{deliveryDate.Date:yyyy-MM-dd HH:mm:ss}</li>
                        ";
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

            return CreatedAtAction(nameof(GetSubscriptionPeriodById), new { id = subscriptionPeriod.Id }, subscriptionPeriod);
        }

        [Authorize(Roles = nameof(UserType.Manager))]
        [HttpDelete("SubscriptionPeriod/{id}")]
        public async Task<IActionResult> DeleteSubscriptionPeriod(int id)
        {
            var subscriptionPeriod = _unitOfWork.SubscriptionPeriod.Get(p => p.Id == id);
            if (subscriptionPeriod == null)
            {
                return NotFound(new { message = "This Subscription Period does not exist" });
            }

            _unitOfWork.SubscriptionPeriod.Remove(subscriptionPeriod);
            _unitOfWork.Save();

            // Enviar notificação
            var producers = _unitOfWork.AppUser.GetAll(u => u.UserType == UserType.Producer).ToList();
            if (producers.Count != 0)
            {
                var tasks = producers.Select(producer =>
                {
                    if (string.IsNullOrEmpty(producer.Email))
                    {
                        return Task.CompletedTask;
                    }

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

            return Ok(new { message = "Subscription Period successfully deleted" });
        }

    }
}

