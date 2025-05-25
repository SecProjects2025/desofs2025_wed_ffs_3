using AMAP.Entities;

namespace AMAP.Business.Interfaces
{
    public interface ISubscriptionBc
    {
        Subscription CreateSubscription(Subscription subscription, string coProducerId);
        Task<bool> ConfirmSubscriptionEmail(string token);
        Task<bool> AcceptTerms(int subscriptionId);
    }
}