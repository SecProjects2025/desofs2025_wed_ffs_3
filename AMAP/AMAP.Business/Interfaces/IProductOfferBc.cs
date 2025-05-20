using AMAP.Entities;

namespace AMAP.Business.Interfaces
{
    public interface IProductOfferBc
    {
        ProductOffer CreateProductOffer(ProductOffer productOffer, string producerId);

        ProductOffer? GetProductOfferById(int id);

        ProductOffer? GetProductOfferByProducer(string producerId);

        void DeleteProductOffer(int id);
    }
}
