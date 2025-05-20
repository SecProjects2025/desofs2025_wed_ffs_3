// AMAP.Business/Components/CatalogBc.cs
using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Models.DTO.Catalog;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AMAP.Business.Components
{
    public class CatalogBc : ICatalogBc
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public CatalogBc(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<List<CatalogDto>> GetAllCatalogs()
        {
            // Obter todos os produtores
            var producers = _userManager.Users.Where(u => u.UserType == UserType.Producer).ToList();

            if (producers.Count == 0)
            {
                return new List<CatalogDto>();
            }

            var catalogs = new List<CatalogDto>();

            foreach (var producer in producers)
            {
                // Obter todos os produtos não deletados do produtor
                var products = _unitOfWork.Product.GetAll(
                    p => p.ProducerId == producer.Id && !p.IsDeleted,
                    includeProperties: "Composites.SubProduct"
                ).ToList();

                if (products.Any())
                {
                    // Criar o catálogo para cada produtor
                    var producerCatalog = new CatalogDto
                    {
                        ProducerName = producer.Name
                    };

                    foreach (var product in products)
                    {
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
            }

            return catalogs;
        }

        public async Task<List<CatalogDto>> GetCatalogByProducer(string producerId)
        {
            // Obter o produtor pelo ProducerId utilizando o UserManager
            var producer = await _userManager.FindByIdAsync(producerId);

            if (producer == null)
            {
                return new List<CatalogDto>();
            }

            // Obter todos os produtos do produtor com o ProducerId, não deletados
            var products = _unitOfWork.Product.GetAll(
                p => p.ProducerId == producerId && !p.IsDeleted,
                includeProperties: "Composites.SubProduct"
            ).ToList();

            var producerCatalog = new CatalogDto
            {
                ProducerName = producer.Name
            };

            foreach (var product in products)
            {
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
                    productDto.Composites = composites.Select(c => new ProductComposeDto
                    {
                        SubProductName = c.SubProduct.Name,
                        Quantity = c.Quantity
                    }).ToList();
                }

                producerCatalog.Products.Add(productDto);
            }

            return new List<CatalogDto> { producerCatalog };
        }
    }
}
