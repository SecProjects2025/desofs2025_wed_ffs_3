namespace AMAP.Business.Interfaces
{
    public interface IDeliveryBc
    {
        void UpdateDeliveredQuantity(int subscriptionId, int deliveredQuantity, string currentProducerId);
    }
}

