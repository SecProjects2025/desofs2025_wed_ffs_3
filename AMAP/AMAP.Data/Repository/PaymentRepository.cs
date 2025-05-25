using AMAP.DataAccess.Repository.IRepository;
using AMAP.DataAccess.Data;
using AMAP.Entities;
using Microsoft.EntityFrameworkCore;

namespace AMAP.DataAccess.Repository
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Payment payment)
        {
            _context.Payments.Update(payment);
        }
    }
}

