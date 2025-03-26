using System.Security.Claims;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
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
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // POST: api/Subscription
        [HttpPost]
        public IActionResult CreateSubscription([FromBody] Subscription subscription)
        {
            if (subscription == null)
            {
                return BadRequest("Invalid data.");
            }

            // Obter o CoProducer autenticado
            subscription.CoProducerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
            if (subscription.CoProducerId == null)
            {
                return Unauthorized("Invalid CoProducer.");
            }

            // Obter a oferta de produto
            var productOffer = _unitOfWork.ProductOffer.Get(po => po.Id == subscription.ProductOfferId);
            if (productOffer == null)
            {
                return NotFound("Product offer not found.");
            }

            // Validar quantidade
            if (subscription.ExpectedQuantity <= 0)
            {
                return BadRequest("Quantity must be greater than zero.");
            }

            var orderedQuantity = _unitOfWork.Subscription.GetAll(s => s.ProductOfferId == productOffer.Id).Sum(s => s.ExpectedQuantity);

            if (subscription.ExpectedQuantity + orderedQuantity > productOffer.Quantity)
            {
                return BadRequest("Requested quantity exceeds available stock.");
            }

            // Validar se o PaymentType contém apenas bits válidos
            if (subscription.PaymentType == 0 || !Enum.IsDefined(typeof(PaymentType), subscription.PaymentType))
            {
                return BadRequest("Invalid payment type.");
            }

            if (!productOffer.PaymentType.HasFlag(subscription.PaymentType))
            {
                return BadRequest("Selected payment type is not supported by the product offer.");
            }

            // Criar subscrição
            _unitOfWork.Subscription.Add(subscription);
            _unitOfWork.Save();

            return CreatedAtAction(nameof(GetSubscriptionById), new { id = subscription.Id }, subscription);
        }

        // GET: api/Subscription/{id}
        [HttpGet("{id}")]
        public IActionResult GetSubscriptionById(int id)
        {
            var subscription = _unitOfWork.Subscription.Get(s => s.Id == id);
            if (subscription == null)
            {
                return NotFound("Subscription not found.");
            }
            return Ok(subscription);
        }

        // GET: api/Subscription
        [HttpGet]
        public IActionResult GetSubscriptionsByCoProducer()
        {
            // Obter o CoProducer autenticado
            var coProducerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
            if (coProducerId == null)
            {
                return Unauthorized("Invalid CoProducer.");
            }

            var subscriptions = _unitOfWork.Subscription.GetAll(s => s.CoProducerId == coProducerId);
            if (subscriptions == null || !subscriptions.Any())
            {
                return NotFound("No subscriptions found.");
            }

            return Ok(subscriptions);
        }
    }
}
