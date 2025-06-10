using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AMAP.Business.Interfaces;
using AMAP.DataAccess.UnitOfWork;
using AMAP.Entities.Models.DTO.Payment;

namespace AMAP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None, Duration = 0)]
    public class PaymentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentController> _logger;
        private readonly IPaymentBc _paymentBc;

        public PaymentController(IUnitOfWork unitOfWork, ILogger<PaymentController> logger, IPaymentBc paymentBc)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _paymentBc = paymentBc;
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserType.CoProducer))]
        public IActionResult CreatePayment([FromBody] Payment payment)
        {
            if (payment == null)
            {
                return BadRequest("Invalid payment data.");
            }

            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                var result = _paymentBc.CreatePayment(payment, currentUserId);
                return CreatedAtAction(nameof(GetPaymentById), new { id = result.Id }, result);
            }
            catch (ArgumentException)
            {
                return BadRequest("Invalid payment data.");
            }
            catch (UnauthorizedAccessException)
            {
                return new ObjectResult("Forbidden") { StatusCode = 403 };
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Subscription not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return StatusCode(500, "An error occurred while creating the payment.");
            }
        }

        [HttpPut("{id}/validate")]
        [Authorize(Roles = nameof(UserType.Producer))]
        public IActionResult ValidatePayment(int id)
        {
            try
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

                var producerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(producerId))
                {
                    return Unauthorized("User identity not found or invalid.");
                }

                var historyEntry = new PaymentValidationHistory
                {
                    PaymentId = payment.Id,
                    ProducerId = producerId,
                    ValidationTimestamp = DateTime.UtcNow
                };

                _unitOfWork.PaymentValidationHistory.Add(historyEntry);

                payment.Validated = true;
                payment.ValidationDate = DateTime.UtcNow;
                payment.ValidatedByProducerId = producerId;

                _unitOfWork.Payment.Update(payment);
                _unitOfWork.Save();

                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment");
                return new ObjectResult("An error occurred while validating the payment.")
                {
                    StatusCode = 500
                };
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = nameof(UserType.Manager))]
        public IActionResult UpdatePayment(int id, [FromBody] Payment updatedPayment)
        {
            try
            {
                var existingPayment = _unitOfWork.Payment.Get(p => p.Id == id);
                if (existingPayment == null)
                    return NotFound("Payment not found");

                existingPayment.PaymentMethod = updatedPayment.PaymentMethod;
                existingPayment.AmountPaid = updatedPayment.AmountPaid;
                existingPayment.Validated = updatedPayment.Validated;
                existingPayment.ValidationDate = updatedPayment.ValidationDate;
                existingPayment.ValidatedByProducerId = updatedPayment.ValidatedByProducerId;

                _unitOfWork.Payment.Update(existingPayment);
                _unitOfWork.Save();
                return Ok("Payment updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment");
                return new ObjectResult("An error occurred while updating the payment.")
                {
                    StatusCode = 500
                };
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserType.Manager))]
        public IActionResult DeletePayment(int id)
        {
            try
            {
                var payment = _unitOfWork.Payment.Get(p => p.Id == id);
                if (payment == null)
                    return NotFound("Payment not found");

                _unitOfWork.Payment.Delete(payment);
                _unitOfWork.Save();
                return Ok("Payment deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment");
                return new ObjectResult("An error occurred while deleting the payment.")
                {
                    StatusCode = 500
                };
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetPaymentById(int id)
        {
            try
            {
                var payment = _paymentBc.GetPaymentById(id);
                if (payment == null)
                {
                    return NotFound();
                }
                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment by id");
                return StatusCode(500, "An error occurred while retrieving the payment.");
            }
        }
    }
}
