using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Models.DTO.Catalog;
using AMAP.Entities.Models.DTO.Product;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AMAP.Business.Components
{
    public class CatalogBc : ICatalogBc
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<CatalogBc> _logger;

        public CatalogBc(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, ILogger<CatalogBc> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<List<CatalogDto>> GetAllCatalogs()
        {
            _logger.LogInformation("Accessing all catalogs data at {Time}", DateTime.UtcNow);

            // Obter todos os produtores
            var producers = _userManager.Users.Where(u => u.UserType == UserType.Producer).ToList();

            if (producers.Count == 0)
            {
                _logger.LogInformation("No producers found in the system at {Time}", DateTime.UtcNow);
                return new List<CatalogDto>();
            }

            _logger.LogInformation("Found {ProducerCount} producers in the system at {Time}", producers.Count, DateTime.UtcNow);

            var catalogs = new List<CatalogDto>();

            foreach (var producer in producers)
            {
                _logger.LogInformation("Accessing products data for producer - ProducerId: {ProducerId}, Name: {ProducerName} at {Time}",
                    producer.Id, producer.Name, DateTime.UtcNow);

                // Obter todos os produtos não deletados do produtor
                var products = _unitOfWork.Product.GetAll(
                    p => p.ProducerId == producer.Id && !p.IsDeleted,
                    includeProperties: "Composites.SubProduct"
                ).ToList();

                if (products.Any())
                {
                    _logger.LogInformation("Found {ProductCount} products for producer - ProducerId: {ProducerId} at {Time}",
                        products.Count, producer.Id, DateTime.UtcNow);

                    // Criar o catálogo para cada produtor
                    var producerCatalog = new CatalogDto
                    {
                        ProducerName = producer.Name
                    };

                    foreach (var product in products)
                    {
                        _logger.LogInformation("Processing product data - ProductId: {ProductId}, Name: {ProductName}, ProducerId: {ProducerId} at {Time}",
                            product.Id, product.Name, producer.Id, DateTime.UtcNow);

                        // Verificar se o produto é composto
                        var composites = product.Composites?.Where(c => c.SubProduct != null && !c.SubProduct.IsDeleted).ToList();

                        var productDto = new ProductDto
                        {
                            Name = product.Name,
                            Description = product.Description,
                            Photo = product.Photo,
                            ReferencePrice = product.ReferencePrice,
                            DeliveryUnits = product.DeliveryUnits,
                            Type = composites != null && composites.Any() ? "Composite" : "Simple",
                            // Initialize with empty list to avoid null
                            Composites = new List<ProductComposeDto>()
                        };

                        if (composites != null && composites.Any())
                        {
                            _logger.LogInformation("Processing composite product data - ProductId: {ProductId}, CompositeCount: {CompositeCount} at {Time}",
                                product.Id, composites.Count, DateTime.UtcNow);

                            productDto.Composites = composites.Select(c => new ProductComposeDto
                            {
                                SubProductName = c.SubProduct.Name,
                                Quantity = c.Quantity
                            }).ToList();
                        }

                        producerCatalog.Products.Add(productDto);
                    }

                    catalogs.Add(producerCatalog);
                }
                else
                {
                    _logger.LogInformation("No products found for producer - ProducerId: {ProducerId} at {Time}",
                        producer.Id, DateTime.UtcNow);
                }
            }

            _logger.LogInformation("Completed retrieving catalog data - TotalCatalogs: {CatalogCount} at {Time}",
                catalogs.Count, DateTime.UtcNow);

            return catalogs;
        }

        public async Task<List<CatalogDto>> GetCatalogByProducer(string producerId)
        {
            _logger.LogInformation("Accessing catalog data for specific producer - ProducerId: {ProducerId} at {Time}",
                producerId, DateTime.UtcNow);

            // Obter o produtor pelo ProducerId utilizando o UserManager
            var producer = await _userManager.FindByIdAsync(producerId);

            if (producer == null)
            {
                _logger.LogWarning("Producer not found - ProducerId: {ProducerId} at {Time}",
                    producerId, DateTime.UtcNow);
                return new List<CatalogDto>();
            }

            _logger.LogInformation("Found producer - ProducerId: {ProducerId}, Name: {ProducerName} at {Time}",
                producer.Id, producer.Name, DateTime.UtcNow);

            // Obter todos os produtos do produtor com o ProducerId, não deletados
            var products = _unitOfWork.Product.GetAll(
                p => p.ProducerId == producerId && !p.IsDeleted,
                includeProperties: "Composites.SubProduct"
            ).ToList();

            _logger.LogInformation("Retrieved {ProductCount} products for producer - ProducerId: {ProducerId} at {Time}",
                products.Count, producerId, DateTime.UtcNow);

            var producerCatalog = new CatalogDto
            {
                ProducerName = producer.Name
            };

            foreach (var product in products)
            {
                _logger.LogInformation("Processing product data - ProductId: {ProductId}, Name: {ProductName}, ProducerId: {ProducerId} at {Time}",
                    product.Id, product.Name, producerId, DateTime.UtcNow);

                // Verificar se o produto é composto
                var composites = product.Composites?.Where(c => c.SubProduct != null && !c.SubProduct.IsDeleted).ToList();

                var productDto = new ProductDto
                {
                    Name = product.Name,
                    Description = product.Description,
                    Photo = product.Photo,
                    ReferencePrice = product.ReferencePrice,
                    DeliveryUnits = product.DeliveryUnits,
                    Type = composites != null && composites.Any() ? "Composite" : "Simple",
                    Composites = new List<ProductComposeDto>()
                };

                if (composites != null && composites.Any())
                {
                    _logger.LogInformation("Processing composite product data - ProductId: {ProductId}, CompositeCount: {CompositeCount} at {Time}",
                        product.Id, composites.Count, DateTime.UtcNow);

                    productDto.Composites = composites.Select(c => new ProductComposeDto
                    {
                        SubProductName = c.SubProduct.Name,
                        Quantity = c.Quantity
                    }).ToList();
                }

                producerCatalog.Products.Add(productDto);
            }

            _logger.LogInformation("Completed retrieving catalog data for producer - ProducerId: {ProducerId}, ProductCount: {ProductCount} at {Time}",
                producerId, producerCatalog.Products.Count, DateTime.UtcNow);

            return new List<CatalogDto> { producerCatalog };
        }
    }
}