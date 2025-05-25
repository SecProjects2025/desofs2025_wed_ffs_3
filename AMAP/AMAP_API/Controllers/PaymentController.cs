using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AMAP_API.Controllers
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
            payment.ValidationDate = null;
            payment.ValidatedByProducerId = null;

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

            // Get the Producer's user ID from the claims
            var producerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            string? producerId = producerIdClaim?.Value;

            if (string.IsNullOrEmpty(producerId))
            {
                return Unauthorized("User identity not found or invalid.");
            }

            // Create a history entry before updating the payment status
            var historyEntry = new PaymentValidationHistory
            {
                PaymentId = payment.Id,
                // Assign the string producerId directly as the type in PaymentValidationHistory is now string?
                ProducerId = producerId,
                ValidationTimestamp = DateTime.UtcNow
            };

            _unitOfWork.PaymentValidationHistory.Add(historyEntry);

            // Update the payment status
            payment.Validated = true;
            payment.ValidationDate = DateTime.UtcNow;
            payment.ValidatedByProducerId = producerId;

            _unitOfWork.Payment.Update(payment);
            _unitOfWork.Save();

            return Ok(payment);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = nameof(UserType.Manager))]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] Payment updatedPayment)
        {
            var existingPayment = await _unitOfWork.Payment.GetById(id);
            if (existingPayment == null)
                return NotFound("Payment not found");

            // Atualiza apenas os campos necessários, mantendo o ID original
            existingPayment.PaymentMethod = updatedPayment.PaymentMethod;
            existingPayment.AmountPaid = updatedPayment.AmountPaid;
            existingPayment.Validated = updatedPayment.Validated;
            existingPayment.ValidationDate = updatedPayment.ValidationDate;
            existingPayment.ValidatedByProducerId = updatedPayment.ValidatedByProducerId;

            await _unitOfWork.Payment.Update(existingPayment);
            _unitOfWork.Save();
            return Ok("Payment updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserType.Manager))]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _unitOfWork.Payment.GetById(id);
            if (payment == null)
                return NotFound("Payment not found");

            await _unitOfWork.Payment.Delete(payment);
            _unitOfWork.Save();
            return Ok("Payment deleted successfully");
        }
    }
}
