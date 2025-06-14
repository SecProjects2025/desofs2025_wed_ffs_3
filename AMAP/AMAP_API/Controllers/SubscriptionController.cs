using AMAP.Business.Interfaces;
using AMAP.Entities;
using System.Security.Claims;
using AMAP.Entities.Models.DTO.Subscription;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;

namespace AMAP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionBc _subscriptionBc;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(ISubscriptionBc subscriptionBc, ILogger<SubscriptionController> logger)
        {
            _subscriptionBc = subscriptionBc;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserType.CoProducer))]
        public IActionResult CreateSubscription([FromBody] Subscription subscription)
        {
            if (subscription == null)
            {
                return BadRequest("Invalid subscription data.");
            }

            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                var result = _subscriptionBc.CreateSubscription(subscription, currentUserId);
                return CreatedAtAction(nameof(GetSubscriptionById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription");
                return StatusCode(500, "An error occurred while creating the subscription.");
            }
        }

        [HttpPost("confirm")]
        public IActionResult ConfirmSubscription([FromBody] string token)
        {
            try
            {
                var result = _subscriptionBc.ConfirmSubscriptionEmail(token).Result;
                if (!result)
                    return NotFound("Invalid confirmation token.");

                return Ok("Subscription confirmed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming subscription");
                return StatusCode(500, "An error occurred while confirming the subscription.");
            }
        }

        [HttpPost("{id}/accept-terms")]
        [Authorize(Roles = nameof(UserType.CoProducer))]
        public IActionResult AcceptTerms(int id)
        {
            try
            {
                var result = _subscriptionBc.AcceptTerms(id).Result;
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
                _logger.LogError(ex, "Error accepting terms");
                return StatusCode(500, "An error occurred while accepting the terms.");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetSubscriptionById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid subscription ID.");
                }
                var subscription = _subscriptionBc.GetSubscriptionById(id);
                if (subscription == null)
                {
                    return NotFound("Subscription not found.");
                }
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscription");
                return StatusCode(500, "An error occurred while retrieving the subscription.");
            }
        }

        [HttpGet("co-producer/{coProducerId}")]
        [Authorize(Roles = nameof(UserType.CoProducer))]
        public IActionResult GetSubscriptionsByCoProducer(string coProducerId)
        {
            try
            {
                var subscriptions = _subscriptionBc.GetSubscriptionsByCoProducer(coProducerId);
                if (!subscriptions.Any())
                {
                    return NotFound("No subscriptions found for this co-producer.");
                }
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions by co-producer");
                return StatusCode(500, "An error occurred while retrieving subscriptions.");
            }
        }
    }
}
