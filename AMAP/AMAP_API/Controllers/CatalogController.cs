using AMAP.Business.Interfaces;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMAP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : BaseController
    {
        private readonly ICatalogBc _catalogBc;

        public CatalogController(ICatalogBc catalogBc)
        {
            _catalogBc = catalogBc;
        }

        [HttpGet("GetAllCatalogs")]
        public async Task<IActionResult> GetAllCatalogs()
        {
            var catalogsData = await _catalogBc.GetAllCatalogs();

            if (catalogsData.Count == 0)
            {
                return NotFound("No producers found.");
            }

            return Ok(catalogsData);
        }

        [HttpGet("GetCatalog")]
        [Authorize(Roles = nameof(UserType.Producer))]
        public async Task<IActionResult> GetCatalogOfLoggedProducer()
        {
            var producerId = GetLoggedId();
            if (producerId is null)
            {
                return NotFound("Producer not logged in.");
            }

            var catalogData = await _catalogBc.GetCatalogByProducer(producerId);
            if (catalogData.Count == 0)
            {
                return NotFound("Producer not found.");
            }

            return Ok(catalogData);
        }

        [HttpGet("GetCatalog/{producerId}")]
        public async Task<IActionResult> GetCatalogByProducer(string producerId)
        {
            var catalogData = await _catalogBc.GetCatalogByProducer(producerId);
            if (catalogData.Count == 0)
            {
                return NotFound("Producer not found.");
            }

            return Ok(catalogData);
        }
    }
}
