using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMAP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(UserType.CoProducer))]
    public class PaymentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public IActionResult CreatePayment([FromBody] Payment payment)
        {
            if (payment is null)
            {
                return BadRequest("Invalid payment data.");
            }

            // Verificar se a subscrição associada existe
            var subscription = _unitOfWork.Subscription.Get(s => s.Id == payment.SubscriptionId);
            if (subscription is null)
            {
                return NotFound($"Subscription with ID {payment.SubscriptionId} not found.");
            }

            payment.PaymentDate = DateTime.UtcNow;

            _unitOfWork.Payment.Add(payment);
            _unitOfWork.Save();

            return CreatedAtAction(nameof(CreatePayment), new { id = payment.Id }, payment);
        }
    }
}
