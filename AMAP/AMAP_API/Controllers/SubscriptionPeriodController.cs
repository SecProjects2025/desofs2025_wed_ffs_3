using AMAP.Business.Interfaces;
using AMAP.Entities;
using AMAP.Entities.Models.DTO.SubscriptionPeriod;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMAP.Api.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionPeriodController : Controller
    {

        private readonly ISubscriptionPeriodBc _subscriptionPeriodBc;

        public SubscriptionPeriodController(ISubscriptionPeriodBc subscriptionPeriodBc)
        {
            _subscriptionPeriodBc = subscriptionPeriodBc;
        }

        [HttpGet("{id}")]
        public IActionResult GetSubscriptionPeriodById(int id)
        {
            try
            {
                var subscriptionPeriod = _subscriptionPeriodBc.GetSubscriptionPeriodById(id);
                if (subscriptionPeriod == null)
                    return NotFound(new { message = "This Subscription Period does not exist" });

                var dto = new SubscriptionPeriodDto
                {
                    Duration = subscriptionPeriod.Duration,
                    StartDate = subscriptionPeriod.StartDate,
                    EndDate = subscriptionPeriod.EndDate,
                    DeliveryDates = subscriptionPeriod.DeliveryDates?.Select(dd => new DeliveryDateDto
                    {
                        Date = dd.Date
                    })
                };

                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("DeliveryDate/{id}")]
        public IActionResult GetDeliveryDateById(int id)
        {
            try
            {
                var deliveryDate = _subscriptionPeriodBc.GetDeliveryDateById(id);
                if (deliveryDate == null)
                    return NotFound(new { message = "This Delivery Date does not exist" });

                var dto = new DeliveryDateDto
                {
                    Date = deliveryDate.Date
                };

                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [Authorize(Roles = nameof(UserType.Manager))]
        [HttpPost()]
        public async Task<IActionResult> CreateSubscriptionPeriodAndDeliveryDate([FromBody] SubscriptionPeriod subscriptionPeriod)
        {
            try
            {
                var created = await _subscriptionPeriodBc.CreateSubscriptionPeriodAndDeliveryDateAsync(subscriptionPeriod);
                return CreatedAtAction(nameof(GetSubscriptionPeriodById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = nameof(UserType.Manager))]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubscriptionPeriod(int id)
        {
            try
            {
                await _subscriptionPeriodBc.DeleteSubscriptionPeriodAsync(id);
                return Ok(new { message = "Subscription Period successfully deleted" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}

