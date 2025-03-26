using AMAP.DataAccess.Repository.IRepository;
using AMAP.DataAccess.Data;
using AMAP.Entities;

namespace AMAP.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var productFromDb = _context.Products.FirstOrDefault(p => p.Id == product.Id);

            if (productFromDb != null)
            {
                // Atualiza os campos básicos
                productFromDb.Name = product.Name;
                productFromDb.Description = product.Description;
                productFromDb.ReferencePrice = product.ReferencePrice;
                productFromDb.DeliveryUnits = product.DeliveryUnits;
                productFromDb.IsDeleted = product.IsDeleted;

                // Atualiza a imagem apenas se uma nova foi fornecida
                if (!string.IsNullOrEmpty(product.Photo))
                {
                    productFromDb.Photo = product.Photo;
                }
            }
        }
    }
}
