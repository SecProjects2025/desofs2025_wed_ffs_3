using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMAP.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IProductRepository Product { get; }

        ICompositeRepository Composite { get; }

        ISubscriptionPeriodRepository SubscriptionPeriod { get; }

        IDeliveryDateRepository DeliveryDate { get; }

        IProductOfferRepository ProductOffer { get; }

        IAppUserRepository AppUser { get; }

        ISubscriptionRepository Subscription { get; }

        IDeliveryRepository Delivery { get; }

        IPaymentRepository Payment { get; }

        void Save();
    }
}
