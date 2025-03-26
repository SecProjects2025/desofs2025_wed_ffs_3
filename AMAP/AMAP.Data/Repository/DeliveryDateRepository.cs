using AMAP.DataAccess.Repository.IRepository;
using AMAP.DataAccess.Data;
using AMAP.Entities;

namespace AMAP.DataAccess.Repository
{
    public class DeliveryDateRepository : Repository<DeliveryDate>, IDeliveryDateRepository
    {
        private readonly AppDbContext _context;

        public DeliveryDateRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(DeliveryDate deliveryDate)
        {
            var deliveryDateFromDb = _context.DeliveryDates.FirstOrDefault(p => p.Id == deliveryDate.Id);

            if (deliveryDateFromDb != null)
            {
                deliveryDateFromDb.Date = deliveryDate.Date;
            }
        }
    }
}