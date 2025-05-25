using AMAP.Business.Interfaces;
using AMAP.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMAP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionBc _subscriptionBc;

        public SubscriptionController(ISubscriptionBc subscriptionBc)
        {
            _subscriptionBc = subscriptionBc;
        }

        [HttpPost]
        // [Authorize(Roles = "CoProducer")] // Temporariamente removido para testes
        public async Task<ActionResult<Subscription>> CreateSubscription(Subscription subscription)
        {
            try
            {
                // Temporariamente usando um ID fixo para testes
                var coProducerId = "1cc4c021-19e7-49dd-aeb0-e31a8f382f66"; // Seu ID de co-produtor

                var result = _subscriptionBc.CreateSubscription(subscription, coProducerId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while creating the subscription.");
            }
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmSubscription([FromBody] string token)
        {
            try
            {
                var result = await _subscriptionBc.ConfirmSubscriptionEmail(token);
                if (!result)
                    return NotFound("Invalid confirmation token.");

                return Ok("Subscription confirmed successfully.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while confirming the subscription.");
            }
        }

        [HttpPost("{id}/accept-terms")]
        [Authorize(Roles = "CoProducer")]
        public async Task<IActionResult> AcceptTerms(int id)
        {
            try
            {
                var result = await _subscriptionBc.AcceptTerms(id);
                if (!result)
                    return NotFound("Subscription not found.");

                return Ok("Terms accepted successfully.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while accepting the terms.");
            }
        }
    }
}
