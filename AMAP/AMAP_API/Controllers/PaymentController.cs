using AMAP.Business.Interfaces;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AMAP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(UserType.CoProducer))]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentBc _paymentBc;

        public PaymentController(IPaymentBc paymentBc)
        {
            _paymentBc = paymentBc;
        }

        [HttpPost]
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
        }
    }
}
