using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AMAP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public CatalogController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet("GetAllCatalogs")]
        public async Task<IActionResult> GetAllCatalogs()
        {
            // Obter todos os produtores
            var producers = _userManager.Users.ToList();

            if (producers.Count == 0)
            {
                return NotFound("No producers found.");
            }

            var catalogs = new List<object>();

            foreach (var producer in producers)
            {
                // Obter todos os produtos não deletados do produtor
                var products = _unitOfWork.Product.GetAll(
                    p => p.ProducerId == producer.Id && !p.IsDeleted,
                    includeProperties: "Composites.SubProduct" // Incluir Composites e SubProduct dentro de Composites
                ).ToList(); // Materializa a consulta

                if (products.Any())
                {
                    // Criar o catálogo para cada produtor
                    var producerCatalog = new
                    {
                        ProducerName = producer.Name,
                        Products = new List<object>(), // Inicializar como uma lista vazia
                    };

                    foreach (var product in products)
                    {
                        // Verificar se o produto é composto
                        var components = product.Composites?.Where(c => !c.SubProduct.IsDeleted).ToList();

                        if (components != null && components.Any())
                        {
                            // Produto composto
                            producerCatalog.Products.Add(new
                            {
                                product.Name,
                                product.Description,
                                product.Photo,
                                product.ReferencePrice,
                                product.DeliveryUnits,
                                Type = "Composite",
                                Components = components.Select(c => new
                                {
                                    SubProductName = c.SubProduct.Name,
                                    c.Quantity
                                }).ToList(),
                            });
                        }
                        else
                        {
                            // Produto simples
                            producerCatalog.Products.Add(new
                            {
                                product.Name,
                                product.Description,
                                product.Photo,
                                product.ReferencePrice,
                                product.DeliveryUnits,
                                Type = "Simple",
                            });
                        }
                    }

                    catalogs.Add(producerCatalog); // Adiciona o catálogo ao final
                }
            }

            return Ok(catalogs);
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

            return await GetCatalogByProducer(producerId);
        }

        [HttpGet("GetCatalog/{producerId}")]
        public async Task<IActionResult> GetCatalogByProducer(string producerId)
        {
            // Obter o produtor pelo ProducerId utilizando o UserManager
            var producer = await _userManager.FindByIdAsync(producerId);

            if (producer == null)
            {
                return NotFound("Producer not found.");
            }

            // Obter todos os produtos do produtor com o ProducerId, não deletados
            var products = _unitOfWork.Product.GetAll(p => p.ProducerId == producerId && !p.IsDeleted,
                includeProperties: "Composites.SubProduct").ToList();

            var catalog = new List<object>();

            var producerCatalog = new
            {
                ProducerName = producer.Name,
                Products = new List<object>(), // Inicializar como uma lista vazia
            };

            foreach (var product in products)
            {
                // Verificar se o produto é composto
                var components = product.Composites?.Where(c => !c.SubProduct.IsDeleted).ToList();

                if (components != null && components.Any())
                {
                    // Produto composto
                    producerCatalog.Products.Add(new
                    {
                        product.Id,
                        product.Name,
                        product.Description,
                        product.Photo,
                        ReferencePrice = $"{product.ReferencePrice} €",
                        product.DeliveryUnits,
                        Type = "Composite",
                        Components = components.Select(c => new
                        {
                            SubProductName = c.SubProduct.Name,
                            c.Quantity
                        }).ToList(),
                    });
                }
                else
                {
                    // Produto simples
                    producerCatalog.Products.Add(new
                    {
                        product.Id,
                        product.Name,
                        product.Description,
                        product.Photo,
                        ReferencePrice = $"{product.ReferencePrice} €",
                        product.DeliveryUnits,
                        Type = "Simple",
                    });
                }
            }

            catalog.Add(producerCatalog);

            return Ok(catalog);
        }
    }
}
