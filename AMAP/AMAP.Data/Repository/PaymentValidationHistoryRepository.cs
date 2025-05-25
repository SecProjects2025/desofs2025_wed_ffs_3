using AMAP.DataAccess.Data;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;

namespace AMAP.DataAccess.Repository
{
    public class PaymentValidationHistoryRepository : Repository<PaymentValidationHistory>, IPaymentValidationHistoryRepository
    {
        private readonly AppDbContext _context;

        public PaymentValidationHistoryRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // Specific methods for PaymentValidationHistory can be implemented here if needed
    }
}