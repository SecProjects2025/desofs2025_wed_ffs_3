using AMAP.Entities;

namespace AMAP.DataAccess.Repository.IRepository
{
    public interface ISubscriptionPeriodRepository : IRepository<SubscriptionPeriod>
    {
        void Update(SubscriptionPeriod subscriptionPeriod);
    }
}