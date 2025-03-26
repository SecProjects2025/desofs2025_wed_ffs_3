using AMAP.DataAccess.Data;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMAP.DataAccess.Repository
{
    public class CompositeRepository : Repository<Composite>, ICompositeRepository
    {
        private readonly AppDbContext _context;

        public CompositeRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
