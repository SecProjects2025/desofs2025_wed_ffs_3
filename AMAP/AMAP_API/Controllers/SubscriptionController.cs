using AMAP.Business.Interfaces;
using AMAP.Entities;
using System.Security.Claims;
using AMAP.Entities.Models.DTO.Subscription;
using AMAP.Entities.Utilities;
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
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetSubscriptionById(int id)
        {
            try
            {
                var subscription = _subscriptionBc.GetSubscriptionById(id);
                if (subscription == null)
                    return NotFound("Subscription not found.");

                var dto = new SubscriptionDto
                {
                    ProductOfferId = subscription.ProductOfferId,
                    ExpectedQuantity = subscription.ExpectedQuantity,
                    PaymentType = subscription.PaymentType
                };

                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetSubscriptionsByCoProducer()
        {
            try
            {
                var coProducerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
                if (coProducerId == null)
                {
                    return Unauthorized("Invalid CoProducer.");
                }

                var subscriptions = _subscriptionBc.GetSubscriptionsByCoProducer(coProducerId);
                if (subscriptions == null || !subscriptions.Any())
                {
                    return NotFound("No subscriptions found.");
                }

                var dtos = subscriptions.Select(subscription => new SubscriptionDto
                {
                    ProductOfferId = subscription.ProductOfferId,
                    ExpectedQuantity = subscription.ExpectedQuantity,
                    PaymentType = subscription.PaymentType
                }).ToList();

                return Ok(dtos);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
