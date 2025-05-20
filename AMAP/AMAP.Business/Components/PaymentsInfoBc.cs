using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;

namespace AMAP.Business.Components
{
    public class PaymentsInfoBc : IPaymentsInfoBc
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentsInfoBc(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>> GetProducerPayments()
        {
            // buscar produtores
            var producers = _unitOfWork.AppUser.GetAll(u => u.UserType.Equals(UserType.Producer));

            var productOffers = new Dictionary<AppUser, List<ProductOffer>>();
            foreach (var producer in producers)
            {
                productOffers[producer] = [];
                var products = _unitOfWork.Product.GetAll(p => p.ProducerId == producer.Id);
                foreach (var product in products)
                {
                    productOffers[producer].AddRange(_unitOfWork.ProductOffer.GetAll(po => po.ProductId == product.Id));
                }
            }

            var result = new Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>>(); // arvore
            foreach (var producerProductOffers in productOffers)
            {
                var subscriptions = new Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>();
                foreach (var productOffer in producerProductOffers.Value)
                {
                    foreach (var subscription in _unitOfWork.Subscription.GetAll(s => s.ProductOfferId == productOffer.Id))
                    {
                        var subscriptionPayments = new Tuple<Subscription, IEnumerable<Payment>>(subscription, _unitOfWork.Payment.GetAll(p => p.SubscriptionId == subscription.Id));
                        subscriptions.Add(subscription.Id, subscriptionPayments);
                    }
                }
                result.Add(producerProductOffers.Key.Id, new Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>(producerProductOffers.Key, subscriptions));
            }

            // limpeza
            var keysToRemove = result.Where(ps => ps.Value.Item2.Count == 0).Select(ps => ps.Key).ToList();
            foreach (var key in keysToRemove)
            {
                result.Remove(key);
            }

            return result;
        }

        public Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>> GetCoProducerPayments()
        {
            // buscar coprodutores
            var coProducers = _unitOfWork.AppUser.GetAll(u => u.UserType.Equals(UserType.CoProducer));

            var result = new Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>>();
            foreach (var coProducer in coProducers)
            {
                var subscriptions = new Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>();
                foreach (var subscription in _unitOfWork.Subscription.GetAll(s => s.CoProducerId == coProducer.Id))
                {
                    var subscriptionPayments = new Tuple<Subscription, IEnumerable<Payment>>(subscription, _unitOfWork.Payment.GetAll(p => p.SubscriptionId == subscription.Id));
                    subscriptions.Add(subscription.Id, subscriptionPayments);
                }
                result.Add(coProducer.Id, new Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>(coProducer, subscriptions));
            }

            // limpeza - fixing the potential issue with modifying a collection during iteration
            var keysToRemove = result.Where(cs => cs.Value.Item2.Count == 0).Select(cs => cs.Key).ToList();
            foreach (var key in keysToRemove)
            {
                result.Remove(key);
            }

            return result;
        }

        public Dictionary<string, double> GetProducerToReceive()
        {
            var totals = GetProducerTotals();
            var result = new Dictionary<string, double>();

            foreach (var user in totals)
            {
                result[user.Key] = user.Value.Item1 - user.Value.Item2;
            }

            return result;
        }

        public Dictionary<string, double> GetCoProducerToPay()
        {
            var totals = GetCoProducerTotals();
            var result = new Dictionary<string, double>();

            foreach (var user in totals)
            {
                result[user.Key] = user.Value.Item1 - user.Value.Item2;
            }

            return result;
        }

        public Dictionary<string, Tuple<double, double>> GetProducerTotals()
        {
            // Buscar produtores
            var producers = _unitOfWork.AppUser.GetAll(u => u.UserType.Equals(UserType.Producer));

            var productOffers = new Dictionary<AppUser, List<ProductOffer>>();
            foreach (var producer in producers)
            {
                productOffers[producer] = new List<ProductOffer>();
                var products = _unitOfWork.Product.GetAll(p => p.ProducerId == producer.Id);
                foreach (var product in products)
                {
                    productOffers[producer].AddRange(_unitOfWork.ProductOffer.GetAll(po => po.ProductId == product.Id));
                }
            }

            var result = new Dictionary<AppUser, Dictionary<Subscription, IEnumerable<Payment>>>();
            foreach (var producerProductOffers in productOffers)
            {
                result[producerProductOffers.Key] = new Dictionary<Subscription, IEnumerable<Payment>>();
                foreach (var productOffer in producerProductOffers.Value)
                {
                    foreach (var subscription in _unitOfWork.Subscription.GetAll(s => s.ProductOfferId == productOffer.Id))
                    {
                        var payments = _unitOfWork.Payment.GetAll(p => p.SubscriptionId == subscription.Id);
                        result[producerProductOffers.Key].Add(subscription, payments);
                    }
                }
            }

            // Limpeza de produtores sem subscrições
            result = result.Where(x => x.Value.Count > 0).ToDictionary(x => x.Key, x => x.Value);

            var sums = new Dictionary<string, Tuple<double, double>>();
            foreach (var resultUser in result)
            {
                double subscriptionTotals = 0;
                double paymentTotals = 0;

                foreach (var subscription in resultUser.Value)
                {
                    // Obter a quantidade entregue a partir da tabela Delivery
                    var deliveredQuantity = _unitOfWork.Delivery
                        .GetAll(d => d.SubscriptionId == subscription.Key.Id)
                        .Sum(d => d.DeliveredQuantity);

                    // Usar a quantidade esperada se não houver entregas
                    var quantity = deliveredQuantity > 0 ? deliveredQuantity : subscription.Key.ExpectedQuantity;

                    var productOffer = _unitOfWork.ProductOffer.Get(po => po.Id == subscription.Key.ProductOfferId);
                    subscriptionTotals += quantity * productOffer.Price;

                    paymentTotals += subscription.Value.Sum(p => p.AmountPaid);
                }

                sums[resultUser.Key.Id] = new Tuple<double, double>(subscriptionTotals, paymentTotals);
            }

            return sums;
        }

        public Dictionary<string, Tuple<double, double>> GetCoProducerTotals()
        {
            // Buscar coprodutores
            var coProducers = _unitOfWork.AppUser.GetAll(u => u.UserType.Equals(UserType.CoProducer));

            var result = new Dictionary<AppUser, Dictionary<Subscription, IEnumerable<Payment>>>();
            foreach (var coProducer in coProducers)
            {
                result[coProducer] = new Dictionary<Subscription, IEnumerable<Payment>>();

                foreach (var subscription in _unitOfWork.Subscription.GetAll(s => s.CoProducerId == coProducer.Id))
                {
                    var payments = _unitOfWork.Payment.GetAll(p => p.SubscriptionId == subscription.Id);
                    result[coProducer].Add(subscription, payments);
                }
            }

            // Limpeza de coProdutores sem subscrições
            result = result.Where(x => x.Value.Count > 0).ToDictionary(x => x.Key, x => x.Value);

            var sums = new Dictionary<string, Tuple<double, double>>();
            foreach (var resultUser in result)
            {
                double subscriptionTotals = 0;
                double paymentTotals = 0;

                foreach (var subscription in resultUser.Value)
                {
                    // Obter a quantidade entregue a partir da tabela Delivery
                    var deliveredQuantity = _unitOfWork.Delivery
                        .GetAll(d => d.SubscriptionId == subscription.Key.Id)
                        .Sum(d => d.DeliveredQuantity);

                    // Usar a quantidade esperada se não houver entregas
                    var quantity = deliveredQuantity > 0 ? deliveredQuantity : subscription.Key.ExpectedQuantity;

                    var productOffer = _unitOfWork.ProductOffer.Get(po => po.Id == subscription.Key.ProductOfferId);
                    subscriptionTotals += quantity * productOffer.Price;

                    paymentTotals += subscription.Value.Sum(p => p.AmountPaid);
                }

                sums[resultUser.Key.Id] = new Tuple<double, double>(subscriptionTotals, paymentTotals);
            }

            return sums;
        }

        public Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>> GetProducerCC()
        {
            // buscar produtores
            var producers = _unitOfWork.AppUser.GetAll(u => u.UserType.Equals(UserType.Producer));

            var productOffers = new Dictionary<AppUser, List<ProductOffer>>();
            foreach (var producer in producers)
            {
                productOffers[producer] = [];
                var products = _unitOfWork.Product.GetAll(p => p.ProducerId == producer.Id);
                foreach (var product in products)
                {
                    productOffers[producer].AddRange(_unitOfWork.ProductOffer.GetAll(po => po.ProductId == product.Id));
                }
            }

            var result = new Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>>(); // arvore
            foreach (var producerProductOffers in productOffers)
            {
                var subscriptions = new List<Subscription>();
                var payments = new List<Payment>();
                foreach (var productOffer in producerProductOffers.Value)
                {
                    foreach (var subscription in _unitOfWork.Subscription.GetAll(s => s.ProductOfferId == productOffer.Id))
                    {
                        subscriptions.Add(subscription);
                        payments.AddRange(_unitOfWork.Payment.GetAll(p => p.SubscriptionId == subscription.Id));
                    }
                }
                result.Add(producerProductOffers.Key.Id, new Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>(producerProductOffers.Key, subscriptions, payments));
            }

            // limpeza - fixing the collection modification during iteration issue
            var keysToRemove = result.Where(ps => ps.Value.Item2.Count() == 0).Select(ps => ps.Key).ToList();
            foreach (var key in keysToRemove)
            {
                result.Remove(key);
            }

            return result;
        }

        public Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>> GetCoProducerCC()
        {
            // buscar coprodutores
            var coProducers = _unitOfWork.AppUser.GetAll(u => u.UserType.Equals(UserType.CoProducer));

            var result = new Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>>();
            foreach (var coProducer in coProducers)
            {
                var subscriptions = new List<Subscription>();
                var payments = new List<Payment>();
                foreach (var subscription in _unitOfWork.Subscription.GetAll(s => s.CoProducerId == coProducer.Id))
                {
                    subscriptions.Add(subscription);
                    payments.AddRange(_unitOfWork.Payment.GetAll(p => p.SubscriptionId == subscription.Id));
                }
                result.Add(coProducer.Id, new Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>(coProducer, subscriptions, payments));
            }

            // limpeza - fixing the collection modification during iteration issue
            var keysToRemove = result.Where(cs => cs.Value.Item2.Count() == 0).Select(cs => cs.Key).ToList();
            foreach (var key in keysToRemove)
            {
                result.Remove(key);
            }

            return result;
        }
    }
}
