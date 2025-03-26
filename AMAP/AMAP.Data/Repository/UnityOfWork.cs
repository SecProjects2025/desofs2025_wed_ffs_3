using AMAP.DataAccess.Data;
using AMAP.DataAccess.Repository.IRepository;


namespace AMAP.DataAccess.Repository
{
    public class UnityOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IProductRepository Product { get; private set; }

        public ISubscriptionPeriodRepository SubscriptionPeriod { get; private set; }

        public IDeliveryDateRepository DeliveryDate { get; private set; }

        public ICompositeRepository Composite { get; private set; }

        public IProductOfferRepository ProductOffer { get; private set; }

        public IAppUserRepository AppUser { get; private set; }

        public ISubscriptionRepository Subscription { get; private set; }

        public IDeliveryRepository Delivery { get; private set; }

        public IPaymentRepository Payment { get; private set; }

        public UnityOfWork(AppDbContext context)
        {
            _context = context;
            Product = new ProductRepository(_context);
            SubscriptionPeriod = new SubscriptionPeriodRepository(_context);
            DeliveryDate = new DeliveryDateRepository(_context);
            Composite = new CompositeRepository(_context);
            ProductOffer = new ProductOfferRepository(_context);
            AppUser = new AppUserRepository(_context);
            Subscription = new SubscriptionRepository(_context);
            Delivery = new DeliveryRepository(_context);
            Payment = new PaymentRepository(_context);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
