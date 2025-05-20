using AMAP.Entities;

namespace AMAP.Business.Interfaces
{
    public interface IPaymentsInfoBc
    {
        Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>> GetProducerPayments();

        Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>> GetCoProducerPayments();

        Dictionary<string, Tuple<double, double>> GetProducerTotals();

        Dictionary<string, Tuple<double, double>> GetCoProducerTotals();

        Dictionary<string, double> GetProducerToReceive();

        Dictionary<string, double> GetCoProducerToPay();

        Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>> GetProducerCC();

        Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>> GetCoProducerCC();
    }
}
