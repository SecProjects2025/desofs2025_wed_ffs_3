using AMAP.DataAccess.Repository.IRepository;
using AMAP.DataAccess.Data;
using AMAP.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AMAP.DataAccess.Repository
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        private readonly AppDbContext _db;

        public PaymentRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Payment> GetById(int id)
        {
            return await _db.Payments.FindAsync(id);
        }

        public async Task Update(Payment payment)
        {
            _db.Payments.Update(payment);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(Payment payment)
        {
            _db.Payments.Remove(payment);
            await _db.SaveChangesAsync();
        }
    }
}

