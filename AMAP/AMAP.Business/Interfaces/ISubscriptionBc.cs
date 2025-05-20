using AMAP.Entities;

namespace AMAP.Business.Interfaces
{
    public interface ISubscriptionBc
    {
        Subscription CreateSubscription(Subscription subscription, string coProducerId);

        Subscription? GetSubscriptionById(int id);

        IEnumerable<Subscription> GetSubscriptionsByCoProducer(string coProducerId);
    }
}
