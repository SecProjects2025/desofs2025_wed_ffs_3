using AMAP.DataAccess.Repository.IRepository;
using AMAP.DataAccess.Data;
using AMAP.Entities;
using Microsoft.EntityFrameworkCore;

namespace AMAP.DataAccess.Repository
{
    public class SubscriptionPeriodRepository : Repository<SubscriptionPeriod>, ISubscriptionPeriodRepository
    {
        private readonly AppDbContext _context;

        public SubscriptionPeriodRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(SubscriptionPeriod subscriptionPeriod)
        {
            var subscriptionPeriodFromDb = _context.SubscriptionPeriods.FirstOrDefault(sp => sp.Id == subscriptionPeriod.Id);

            if (subscriptionPeriodFromDb != null)
            {
                // Atualizar o SubscriptionPeriod
                subscriptionPeriodFromDb.Duration = subscriptionPeriod.Duration;
                subscriptionPeriodFromDb.StartDate = subscriptionPeriod.StartDate;
                subscriptionPeriodFromDb.EndDate = subscriptionPeriod.EndDate;
            }
        }
    }
}

