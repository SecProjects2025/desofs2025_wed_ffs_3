using AMAP.Entities;

namespace AMAP.DataAccess.Repository.IRepository
{
    public interface IProductOfferRepository : IRepository<ProductOffer>
    {
        public void Update(ProductOffer productOffer);
    }
}