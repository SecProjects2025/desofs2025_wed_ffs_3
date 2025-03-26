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
    [Authorize(Roles = nameof(UserType.Producer))]
    public class ProductOfferController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductOfferController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // POST: api/ProductOffer
        [HttpPost]
        public IActionResult CreateProductOffer([FromBody] ProductOffer productOffer)
        {
            if (productOffer == null)
            {
                return BadRequest("Invalid data.");
            }

            // Obter o produto
            var product = _unitOfWork.Product.Get(p => p.Id == productOffer.ProductId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // Verificar se o produto pertence ao produtor autenticado 
            var producerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
            if (producerId is null || product.ProducerId != producerId)
            {
                return BadRequest("You cannot create an offer for a product from another producer.");
            }

            // Validate SubscriptionPeriodId
            var subscriptionPeriod = _unitOfWork.SubscriptionPeriod.Get(sp => sp.Id == productOffer.SubscriptionPeriodId);
            if (subscriptionPeriod == null)
            {
                return NotFound("Subscription period not found.");
            }

            // Validate DeliveryDate
            if (productOffer.DeliveryDate < DateTime.Now)
            {
                return BadRequest("Delivery date cannot be in the past.");
            }

            // Verificar se já existe uma oferta para o mesmo produto e mesma data de entrega
            var existingOffer = _unitOfWork.ProductOffer.GetAll(po => po.ProductId == productOffer.ProductId && po.DeliveryDate == productOffer.DeliveryDate).FirstOrDefault();
            if (existingOffer != null)
            {
                return BadRequest("A product offer already exists for this product on the selected delivery date.");
            }

            subscriptionPeriod.DeliveryDates = _unitOfWork.DeliveryDate.GetAll(d => d.SubscriptionPeriodId == productOffer.SubscriptionPeriodId);

            if (!subscriptionPeriod.DeliveryDates.Select(dd => dd.Date).Contains(productOffer.DeliveryDate))
            {
                return BadRequest("Delivery date outside subscription period.");
            }

            if (productOffer.Quantity <= 0)
            {
                return BadRequest("Product offer quantity must be greater than zero.");
            }

            if (productOffer.PaymentType <= 0 || (int)productOffer.PaymentType > Enum.GetValues<PaymentType>().Cast<int>().Sum())
            {
                return BadRequest("Invalid payment type.");
            }

            _unitOfWork.ProductOffer.Add(productOffer);
            _unitOfWork.Save();

            return CreatedAtAction(nameof(GetProductOfferById), new { id = productOffer.Id }, productOffer);
        }


        // GET: api/ProductOffer/{id}
        [HttpGet("{id}")]
        public IActionResult GetProductOfferById(int id)
        {
            var productOffer = _unitOfWork.ProductOffer.Get(po => po.Id == id);
            if (productOffer == null)
            {
                return NotFound("Product offer not found.");
            }
            return Ok(productOffer);
        }

        [HttpGet("{producerId}")]
        public IActionResult GetProductOfferByProducer(int producerId)
        {
            var productOffer = _unitOfWork.ProductOffer.Get(po => po.Product.Id == producerId);
            if (productOffer == null)
            {
                return NotFound("Product offer not found.");
            }
            return Ok(productOffer);
        }

        // DELETE: api/ProductOffer/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteProductOffer(int id)
        {
            var productOffer = _unitOfWork.ProductOffer.Get(po => po.Id == id);
            if (productOffer == null)
            {
                return NotFound("Product offer not found.");
            }

            // Verificar se existe uma Subscription associada
            var hasSubscriptions = _unitOfWork.Subscription.GetAll(s => s.ProductOfferId == id).Any();
            if (hasSubscriptions)
            {
                return BadRequest("Cannot delete product offer because it has associated subscriptions.");
            }

            _unitOfWork.ProductOffer.Remove(productOffer);
            _unitOfWork.Save();

            return Ok("Product offer deleted successfully.");
        }

    }
}
