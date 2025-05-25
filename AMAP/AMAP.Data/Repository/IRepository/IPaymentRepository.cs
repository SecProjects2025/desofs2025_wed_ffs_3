using AMAP.Entities;
using System.Threading.Tasks;

namespace AMAP.DataAccess.Repository.IRepository
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment> GetById(int id);
        Task Update(Payment payment);
        Task Delete(Payment payment);
    }
}