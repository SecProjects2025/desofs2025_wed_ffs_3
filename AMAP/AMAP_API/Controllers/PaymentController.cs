using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMAP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserType.CoProducer))]
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
            payment.Validated = false;

            _unitOfWork.Payment.Add(payment);
            _unitOfWork.Save();

            return CreatedAtAction(nameof(CreatePayment), new { id = payment.Id }, payment);
        }

        [HttpPut("{id}/validate")]
        [Authorize(Roles = nameof(UserType.Producer))]
        public IActionResult ValidatePayment(int id)
        {
            var payment = _unitOfWork.Payment.Get(p => p.Id == id);
            if (payment == null)
            {
                return NotFound($"Payment with ID {id} not found.");
            }

            if (payment.Validated)
            {
                return BadRequest("Payment is already validated.");
            }

            payment.Validated = true;

            _unitOfWork.Payment.Update(payment);
            _unitOfWork.Save();

            return Ok(payment);
        }
    }
}
