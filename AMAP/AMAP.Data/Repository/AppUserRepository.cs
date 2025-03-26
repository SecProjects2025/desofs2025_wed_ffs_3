using AMAP.DataAccess.Data;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;

namespace AMAP.DataAccess.Repository
{
    public class AppUserRepository : Repository<AppUser>, IAppUserRepository
    {
        private readonly AppDbContext _context;

        public AppUserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
