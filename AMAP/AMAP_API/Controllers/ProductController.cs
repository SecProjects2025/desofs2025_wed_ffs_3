using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AMAP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(UserType.Producer))]
    public class ProductController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            var product = _unitOfWork.Product.Get(p => p.Id == id && !p.IsDeleted, "Composites.SubProduct");

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Photo,
                product.ReferencePrice,
                product.DeliveryUnits,
                Type = product.Composites.Any() ? "Composite" : "Simple",
                Components = product.Composites.Select(c => new
                {
                    SubProductId = c.SubProduct.Id,
                    SubProductName = c.SubProduct.Name,
                    c.Quantity
                })
            });
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest(new { message = "Invalid product data" });
            }

            var producerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(producerId))
            {
                return Unauthorized(new { message = "Producer not authenticated" });
            }

            product.ProducerId = producerId;

            // Verificar duplicidade
            var existingProduct = _unitOfWork.Product.Get(p => p.Name == product.Name && p.ProducerId == producerId && !p.IsDeleted);
            if (existingProduct is not null)
            {
                return Conflict(new { message = "You already have a product with the same name" });
            }

            // Garantir que os compostos não estão nulos para evitar NullReferenceException
            var composites = product.Composites?.ToList() ?? [];
            product.Composites = null; // Evitar ciclo de dependência ao salvar o produto principal

            // Adicionar o produto principal (não atribuir manualmente o Id)
            _unitOfWork.Product.Add(product);
            _unitOfWork.Save(); // O EF Core gera o Id automaticamente

            // Agora que o produto principal foi salvo, processar os compostos
            foreach (var composite in composites)
            {
                var subProduct = _unitOfWork.Product.Get(p => p.Id == composite.SubProductId && !p.IsDeleted);

                if (subProduct == null)
                {
                    return BadRequest(new { message = $"Subproduct with ID {composite.SubProductId} does not exist." });
                }

                if (subProduct.ProducerId != producerId)
                {
                    return BadRequest(new { message = $"Subproduct with ID {composite.SubProductId} does not belong to the producer." });
                }

                // Associar o ID do produto principal
                composite.ProductId = product.Id;

                _unitOfWork.Composite.Add(composite);
            }

            // Salvar os compostos no banco
            if (composites.Count != 0)
            {
                _unitOfWork.Save(); // Salvar os compostos após o produto principal
            }

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product product)
        {
            // Validar se os dados do produto foram enviados
            if (product == null)
            {
                return BadRequest(new { message = "Invalid product data" });
            }

            // Obter o ID do produtor autenticado
            var producerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            if (string.IsNullOrEmpty(producerId))
            {
                return Unauthorized(new { message = "Producer not authenticated" });
            }

            // Obter o produto existente
            var existingProduct = _unitOfWork.Product.Get(p => p.Id == id && p.ProducerId == producerId && !p.IsDeleted, "Composites", tracked: true);

            if (existingProduct == null)
            {
                return NotFound(new { message = "Product not found or you are not authorized to update it" });
            }

            // Verificar se já existe um produto com o mesmo nome para outro produtor (excluindo o atual)
            var duplicateProduct = _unitOfWork.Product.Get(p => p.Name == product.Name && p.Id != id && !p.IsDeleted);

            if (duplicateProduct != null)
            {
                return Conflict(new { message = "A product with the same name already exists for another producer" });
            }

            // Verificar se o produtor já possui um produto com o mesmo nome
            var producerProduct = _unitOfWork.Product.Get(p => p.Name == product.Name && p.ProducerId == producerId && p.Id != id && !p.IsDeleted);

            if (producerProduct != null)
            {
                return Conflict(new { message = "You already have a product with the same name" });
            }

            // Atualizar os campos do produto principal (exceto ProducerId)
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Photo = product.Photo;
            existingProduct.ReferencePrice = product.ReferencePrice;
            existingProduct.DeliveryUnits = product.DeliveryUnits;

            // Atualizar subprodutos se o produto for composto
            if (product.Composites != null && product.Composites.Count() != 0)
            {
                // Verificar se os subprodutos são válidos e pertencem ao mesmo produtor
                foreach (var composite in product.Composites)
                {
                    var subProduct = _unitOfWork.Product.Get(p => p.Id == composite.SubProductId && !p.IsDeleted);

                    if (subProduct == null || subProduct.ProducerId != producerId)
                    {
                        return BadRequest(new { message = $"Invalid subproduct with ID {composite.SubProductId} or it does not belong to the producer." });
                    }
                }

                // Remover associações antigas
                foreach (var oldComposite in existingProduct.Composites.ToList())
                {
                    _unitOfWork.Composite.Remove(oldComposite);
                }

                // Adicionar novas associações
                foreach (var composite in product.Composites)
                {
                    composite.ProductId = existingProduct.Id;
                    _unitOfWork.Composite.Add(composite);
                }
            }
            else
            {
                // Se nenhum subproduto foi enviado, significa que o produto não é mais composto
                foreach (var oldComposite in existingProduct.Composites.ToList())
                {
                    _unitOfWork.Composite.Remove(oldComposite);
                }
            }

            // Atualizar o produto no repositório
            _unitOfWork.Product.Update(existingProduct);
            _unitOfWork.Save();

            return NoContent(); // Retorna 204 No Content
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var producerId = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            if (string.IsNullOrEmpty(producerId))
            {
                return Unauthorized(new { message = "Producer not authenticated" });
            }

            var product = _unitOfWork.Product.Get(p => p.Id == id && p.ProducerId == producerId && !p.IsDeleted);

            if (product == null)
            {
                return NotFound(new { message = "Product not found or you are not authorized to delete it" });
            }

            var compositesToDelete = _unitOfWork.Composite.GetAll(c => c.ProductId == id);
            if (compositesToDelete.Any())
            {
                _unitOfWork.Composite.RemoveRange(compositesToDelete);
                _unitOfWork.Save();
            }

            product.IsDeleted = true;

            _unitOfWork.Product.Update(product);
            _unitOfWork.Save();

            return NoContent();
        }
    }
}
