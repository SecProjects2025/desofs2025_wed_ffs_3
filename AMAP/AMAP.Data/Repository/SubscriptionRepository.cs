using AMAP.DataAccess.Repository.IRepository;
using AMAP.DataAccess.Data;
using AMAP.Entities;

namespace AMAP.DataAccess.Repository
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        private readonly AppDbContext _context;

        public SubscriptionRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}

