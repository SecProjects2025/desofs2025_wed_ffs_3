using System.Security.Claims;
using AMAP.Business.Interfaces;
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
        private readonly IProductOfferBc _productOfferBc;

        public ProductOfferController(IProductOfferBc productOfferBc)
        {
            _productOfferBc = productOfferBc;
        }

        [HttpPost]
        public IActionResult CreateProductOffer([FromBody] ProductOffer productOffer)
        {
            try
            {
                var producerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
                var createdOffer = _productOfferBc.CreateProductOffer(productOffer, producerId);
                return CreatedAtAction(nameof(GetProductOfferById), new { id = createdOffer.Id }, createdOffer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetProductOfferById(int id)
        {
            try
            {
                var productOffer = _productOfferBc.GetProductOfferById(id);
                if (productOffer == null)
                {
                    return NotFound("Product offer not found.");
                }

                var dto = new ProductOfferDto
                {
                    ProductId = productOffer.ProductId,
                    SubscriptionPeriodId = productOffer.SubscriptionPeriodId,
                    Price = productOffer.Price,
                    Quantity = productOffer.Quantity,
                    DeliveryDate = productOffer.DeliveryDate,
                    PaymentType = productOffer.PaymentType
                };

                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("producer/{producerId}")]
        public IActionResult GetProductOfferByProducer(string producerId)
        {
            try
            {
                var productOffer = _productOfferBc.GetProductOfferByProducer(producerId);
                if (productOffer == null)
                {
                    return NotFound("Product offer not found.");
                }

                var dto = new ProductOfferDto
                {
                    ProductId = productOffer.ProductId,
                    SubscriptionPeriodId = productOffer.SubscriptionPeriodId,
                    Price = productOffer.Price,
                    Quantity = productOffer.Quantity,
                    DeliveryDate = productOffer.DeliveryDate,
                    PaymentType = productOffer.PaymentType
                };

                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProductOffer(int id)
        {
            try
            {
                _productOfferBc.DeleteProductOffer(id);
                return Ok("Product offer deleted successfully.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
                return StatusCode(500, ex.Message);
            }
        }
    }
}
