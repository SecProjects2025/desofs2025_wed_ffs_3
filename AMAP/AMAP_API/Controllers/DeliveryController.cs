using AMAP.Business.Interfaces;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AMAP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(UserType.Producer))]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryBc _deliveryBc;

        public DeliveryController(IDeliveryBc deliveryBc)
        {
            _deliveryBc = deliveryBc;
        }

        [HttpPut("UpdateDeliveredQuantity/{subscriptionId}")]
        public IActionResult UpdateDeliveredQuantity(int subscriptionId, [FromBody] int deliveredQuantity)
        {
            try
            {
                var producerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                _deliveryBc.UpdateDeliveredQuantity(subscriptionId, deliveredQuantity, producerId);
                return Ok("Delivered quantity updated successfully.");
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
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}

