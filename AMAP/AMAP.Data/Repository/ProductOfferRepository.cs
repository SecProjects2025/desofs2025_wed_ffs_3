using AMAP.DataAccess.Data;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;

namespace AMAP.DataAccess.Repository;

public class ProductOfferRepository : Repository<ProductOffer>, IProductOfferRepository
{
    private readonly AppDbContext _context;

    public ProductOfferRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public void Update(ProductOffer productOffer)
    {
        var productOfferFromDb = _context.ProductOffers.FirstOrDefault(po => po.Id == productOffer.Id);

        if (productOfferFromDb != null)
        {
            productOfferFromDb.Quantity = productOfferFromDb.Quantity;
        }
    }
}