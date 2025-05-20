using AMAP.Entities;

namespace AMAP.Business.Interfaces
{
    public interface ISubscriptionPeriodBc
    {
        SubscriptionPeriod? GetSubscriptionPeriodById(int id);

        DeliveryDate? GetDeliveryDateById(int id);

        Task<SubscriptionPeriod> CreateSubscriptionPeriodAndDeliveryDateAsync(SubscriptionPeriod subscriptionPeriod);

        Task DeleteSubscriptionPeriodAsync(int id);
    }
}
