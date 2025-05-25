using AMAP.Entities;

namespace AMAP.DataAccess.Repository.IRepository
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        void Update(Payment payment);
    }
}