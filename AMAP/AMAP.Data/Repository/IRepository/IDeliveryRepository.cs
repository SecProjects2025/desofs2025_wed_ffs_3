using AMAP.Entities;

namespace AMAP.DataAccess.Repository.IRepository
{
    public interface IDeliveryRepository : IRepository<Delivery>
    {
        public void Update(Delivery delivery);
    }
}