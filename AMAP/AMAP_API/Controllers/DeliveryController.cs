using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
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
        private readonly IUnitOfWork _unitOfWork;

        public DeliveryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPut("UpdateDeliveredQuantity/{subscriptionId}")]
        public IActionResult UpdateDeliveredQuantity(int subscriptionId, [FromBody] int deliveredQuantity)
        {
            if (deliveredQuantity < 0)
            {
                return BadRequest("Delivered quantity cannot be negative.");
            }

            var subscription = _unitOfWork.Subscription.Get(s => s.Id == subscriptionId, includeProperties: "ProductOffer.Product");
            if (subscription == null)
            {
                return NotFound("Subscription not found.");
            }

            var producerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (subscription.ProductOffer == null || subscription.ProductOffer.Product.ProducerId != producerId)
            {
                return Forbid("You are not allowed to update this subscription.");
            }

            var existingDelivery = _unitOfWork.Delivery.Get(d => d.SubscriptionId == subscriptionId);
            if (existingDelivery == null)
            {
                var newDelivery = new Delivery
                {
                    SubscriptionId = subscriptionId,
                    DeliveredQuantity = deliveredQuantity
                };
                _unitOfWork.Delivery.Add(newDelivery);
            }
            else
            {
                existingDelivery.DeliveredQuantity = deliveredQuantity;
                _unitOfWork.Delivery.Update(existingDelivery);
            }

            _unitOfWork.Save();

            return Ok("Delivered quantity updated successfully.");
        }
    }
}