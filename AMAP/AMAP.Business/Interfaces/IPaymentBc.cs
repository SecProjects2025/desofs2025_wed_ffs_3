using AMAP.Entities;

namespace AMAP.Business.Interfaces
{
    public interface IPaymentBc
    {
        Payment CreatePayment(Payment payment, string currentCoProducerId);
    }
}
