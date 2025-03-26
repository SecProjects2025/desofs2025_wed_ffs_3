using AMAP.Entities;

namespace AMAP.DataAccess.Repository.IRepository
{
    public interface IDeliveryDateRepository : IRepository<DeliveryDate>
    {
        void Update(DeliveryDate deliveryDate);
    }

}