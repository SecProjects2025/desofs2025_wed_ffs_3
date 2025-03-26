using AMAP.DataAccess.Repository.IRepository;
using AMAP.DataAccess.Data;
using AMAP.Entities;

namespace AMAP.DataAccess.Repository
{
    public class DeliveryRepository : Repository<Delivery>, IDeliveryRepository
    {
        private readonly AppDbContext _context;

        public DeliveryRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Delivery delivery)
        {
           var deliveryFromDb = _context.Deliveries.FirstOrDefault(p => p.Id == delivery.Id);

            if (deliveryFromDb != null)
            {
                deliveryFromDb.DeliveredQuantity = delivery.DeliveredQuantity;
            }
        }
    }
}

