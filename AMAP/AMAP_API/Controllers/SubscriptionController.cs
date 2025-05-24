using System.Security.Claims;
using AMAP.Business.Interfaces;
using AMAP.Entities;
using AMAP.Entities.Models.DTO.Subscription;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMAP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(UserType.CoProducer))]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionBc _subscriptionBc;

        public SubscriptionController(ISubscriptionBc subscriptionBc)
        {
            _subscriptionBc = subscriptionBc;
        }

        [HttpPost]
        public IActionResult CreateSubscription([FromBody] Subscription subscription)
        {
            try
            {
                var coProducerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
                var createdSubscription = _subscriptionBc.CreateSubscription(subscription, coProducerId);
                return CreatedAtAction(nameof(GetSubscriptionById), new { id = createdSubscription.Id }, createdSubscription);
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
