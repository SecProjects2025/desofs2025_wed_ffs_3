using AMAP.Business.Interfaces;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Claims;

namespace AMAP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentBc _paymentBc;

        public PaymentController(IPaymentBc paymentBc)
        {
            _paymentBc = paymentBc;
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserType.CoProducer))]
        public IActionResult CreatePayment([FromBody] Payment payment)
        {
            try
            {
                var coProducerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
                var createdPayment = _paymentBc.CreatePayment(payment, coProducerId);
                return CreatedAtAction(nameof(CreatePayment), new { id = createdPayment.Id }, createdPayment);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
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
