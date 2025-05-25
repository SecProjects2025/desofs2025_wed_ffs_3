using AMAP.Business.Interfaces;
using AMAP.DataAccess.Repository.IRepository;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.Extensions.Logging;

namespace AMAP.Business.Components
{
    public class PaymentsInfoBc : IPaymentsInfoBc
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentsInfoBc> _logger;

        public PaymentsInfoBc(IUnitOfWork unitOfWork, ILogger<PaymentsInfoBc> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>> GetProducerPayments()
        {
            _logger.LogInformation("Accessing producer payments data at {Time}", DateTime.UtcNow);

            // buscar produtores
            var producers = _unitOfWork.AppUser.GetAll(u => u.UserType.Equals(UserType.Producer));

            _logger.LogInformation("Retrieved {ProducerCount} producers for payment information at {Time}",
                producers.Count(), DateTime.UtcNow);

            var productOffers = new Dictionary<AppUser, List<ProductOffer>>();
            foreach (var producer in producers)
            {
                _logger.LogInformation("Processing payment data for producer - Id: {ProducerId}, Name: {ProducerName} at {Time}",
                    producer.Id, producer.Name, DateTime.UtcNow);

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
                        _logger.LogInformation("Accessing payment details for subscription - SubscriptionId: {SubscriptionId}, CoProducerId: {CoProducerId}, ProductOfferId: {ProductOfferId} at {Time}",
                            subscription.Id, subscription.CoProducerId, subscription.ProductOfferId, DateTime.UtcNow);

                        var payments = _unitOfWork.Payment.GetAll(p => p.SubscriptionId == subscription.Id);

                        _logger.LogInformation("Retrieved {PaymentCount} payments for subscription {SubscriptionId} at {Time}",
                            payments.Count(), subscription.Id, DateTime.UtcNow);

                        var subscriptionPayments = new Tuple<Subscription, IEnumerable<Payment>>(subscription, payments);
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

            _logger.LogInformation("Completed retrieving producer payments data - ProducerCount: {ProducerCount} at {Time}",
                result.Count, DateTime.UtcNow);

            return result;
        }

        public Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>> GetCoProducerPayments()
        {
            _logger.LogInformation("Accessing co-producer payments data at {Time}", DateTime.UtcNow);

            // buscar coprodutores
            var coProducers = _unitOfWork.AppUser.GetAll(u => u.UserType.Equals(UserType.CoProducer));

            _logger.LogInformation("Retrieved {CoProducerCount} co-producers for payment information at {Time}",
                coProducers.Count(), DateTime.UtcNow);

            var result = new Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>>();
            foreach (var coProducer in coProducers)
            {
                _logger.LogInformation("Processing payment data for co-producer - Id: {CoProducerId}, Name: {CoProducerName} at {Time}",
                    coProducer.Id, coProducer.Name, DateTime.UtcNow);

                var subscriptions = new Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>();
                foreach (var subscription in _unitOfWork.Subscription.GetAll(s => s.CoProducerId == coProducer.Id))
                {
                    _logger.LogInformation("Accessing payment details for subscription - SubscriptionId: {SubscriptionId}, CoProducerId: {CoProducerId}, ProductOfferId: {ProductOfferId} at {Time}",
                        subscription.Id, subscription.CoProducerId, subscription.ProductOfferId, DateTime.UtcNow);

                    var payments = _unitOfWork.Payment.GetAll(p => p.SubscriptionId == subscription.Id);

                    _logger.LogInformation("Retrieved {PaymentCount} payments for subscription {SubscriptionId} at {Time}",
                        payments.Count(), subscription.Id, DateTime.UtcNow);

                    var subscriptionPayments = new Tuple<Subscription, IEnumerable<Payment>>(subscription, payments);
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

            _logger.LogInformation("Completed retrieving co-producer payments data - CoProducerCount: {CoProducerCount} at {Time}",
                result.Count, DateTime.UtcNow);

            return result;
        }

        public Dictionary<string, double> GetProducerToReceive()
        {
            _logger.LogInformation("Accessing producer accounts receivable data at {Time}", DateTime.UtcNow);

            var totals = GetProducerTotals();
            var result = new Dictionary<string, double>();

            foreach (var user in totals)
            {
                var amountToReceive = user.Value.Item1 - user.Value.Item2;
                result[user.Key] = amountToReceive;

                _logger.LogInformation("Calculated amount to receive for producer - ProducerId: {ProducerId}, Amount: {Amount} at {Time}",
                    user.Key, amountToReceive, DateTime.UtcNow);
            }

            _logger.LogInformation("Completed retrieving producer receivables data - ProducerCount: {ProducerCount} at {Time}",
                result.Count, DateTime.UtcNow);

            return result;
        }

        public Dictionary<string, double> GetCoProducerToPay()
        {
            _logger.LogInformation("Accessing co-producer accounts payable data at {Time}", DateTime.UtcNow);

            var totals = GetCoProducerTotals();
            var result = new Dictionary<string, double>();

            foreach (var user in totals)
            {
                var amountToPay = user.Value.Item1 - user.Value.Item2;
                result[user.Key] = amountToPay;

                _logger.LogInformation("Calculated amount to pay for co-producer - CoProducerId: {CoProducerId}, Amount: {Amount} at {Time}",
                    user.Key, amountToPay, DateTime.UtcNow);
            }

            _logger.LogInformation("Completed retrieving co-producer payables data - CoProducerCount: {CoProducerCount} at {Time}",
                result.Count, DateTime.UtcNow);

            return result;
        }

        public Dictionary<string, Tuple<double, double>> GetProducerTotals()
        {
            _logger.LogInformation("Accessing producer financial totals data at {Time}", DateTime.UtcNow);

            // Buscar produtores
            var producers = _unitOfWork.AppUser.GetAll(u => u.UserType.Equals(UserType.Producer));

            _logger.LogInformation("Retrieved {ProducerCount} producers for financial totals at {Time}",
                producers.Count(), DateTime.UtcNow);

            var productOffers = new Dictionary<AppUser, List<ProductOffer>>();
            foreach (var producer in producers)
            {
                _logger.LogInformation("Processing financial data for producer - Id: {ProducerId}, Name: {ProducerName} at {Time}",
                    producer.Id, producer.Name, DateTime.UtcNow);

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

                _logger.LogInformation("Calculated financial totals for producer - ProducerId: {ProducerId}, ExpectedAmount: {ExpectedAmount}, ReceivedAmount: {ReceivedAmount} at {Time}",
                    resultUser.Key.Id, subscriptionTotals, paymentTotals, DateTime.UtcNow);

                sums[resultUser.Key.Id] = new Tuple<double, double>(subscriptionTotals, paymentTotals);
            }

            _logger.LogInformation("Completed retrieving producer financial totals - ProducerCount: {ProducerCount} at {Time}",
                sums.Count, DateTime.UtcNow);

            return sums;
        }

        public Dictionary<string, Tuple<double, double>> GetCoProducerTotals()
        {
            _logger.LogInformation("Accessing co-producer financial totals data at {Time}", DateTime.UtcNow);

            // Buscar coprodutores
            var coProducers = _unitOfWork.AppUser.GetAll(u => u.UserType.Equals(UserType.CoProducer));

            _logger.LogInformation("Retrieved {CoProducerCount} co-producers for financial totals at {Time}",
                coProducers.Count(), DateTime.UtcNow);

            var result = new Dictionary<AppUser, Dictionary<Subscription, IEnumerable<Payment>>>();
            foreach (var coProducer in coProducers)
            {
                _logger.LogInformation("Processing financial data for co-producer - Id: {CoProducerId}, Name: {CoProducerName} at {Time}",
                    coProducer.Id, coProducer.Name, DateTime.UtcNow);

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

                _logger.LogInformation("Calculated financial totals for co-producer - CoProducerId: {CoProducerId}, ExpectedAmount: {ExpectedAmount}, PaidAmount: {PaidAmount} at {Time}",
                    resultUser.Key.Id, subscriptionTotals, paymentTotals, DateTime.UtcNow);

                sums[resultUser.Key.Id] = new Tuple<double, double>(subscriptionTotals, paymentTotals);
            }

            _logger.LogInformation("Completed retrieving co-producer financial totals - CoProducerCount: {CoProducerCount} at {Time}",
                sums.Count, DateTime.UtcNow);

            return sums;
        }

        public Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>> GetProducerCC()
        {
            _logger.LogInformation("Accessing producer current account data at {Time}", DateTime.UtcNow);

            // buscar produtores
            var producers = _unitOfWork.AppUser.GetAll(u => u.UserType.Equals(UserType.Producer));

            _logger.LogInformation("Retrieved {ProducerCount} producers for current account information at {Time}",
                producers.Count(), DateTime.UtcNow);

            var productOffers = new Dictionary<AppUser, List<ProductOffer>>();
            foreach (var producer in producers)
            {
                _logger.LogInformation("Processing current account data for producer - Id: {ProducerId}, Name: {ProducerName} at {Time}",
                    producer.Id, producer.Name, DateTime.UtcNow);

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
                        var subscriptionPayments = _unitOfWork.Payment.GetAll(p => p.SubscriptionId == subscription.Id);
                        payments.AddRange(subscriptionPayments);

                        _logger.LogInformation("Added subscription and {PaymentCount} payments to producer account - SubscriptionId: {SubscriptionId}, ProducerId: {ProducerId} at {Time}",
                            subscriptionPayments.Count(), subscription.Id, producerProductOffers.Key.Id, DateTime.UtcNow);
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

            _logger.LogInformation("Completed retrieving producer current account data - ProducerCount: {ProducerCount} at {Time}",
                result.Count, DateTime.UtcNow);

            return result;
        }

        public Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>> GetCoProducerCC()
        {
            _logger.LogInformation("Accessing co-producer current account data at {Time}", DateTime.UtcNow);

            // buscar coprodutores
            var coProducers = _unitOfWork.AppUser.GetAll(u => u.UserType.Equals(UserType.CoProducer));

            _logger.LogInformation("Retrieved {CoProducerCount} co-producers for current account information at {Time}",
                coProducers.Count(), DateTime.UtcNow);

            var result = new Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>>();
            foreach (var coProducer in coProducers)
            {
                _logger.LogInformation("Processing current account data for co-producer - Id: {CoProducerId}, Name: {CoProducerName} at {Time}",
                    coProducer.Id, coProducer.Name, DateTime.UtcNow);

                var subscriptions = new List<Subscription>();
                var payments = new List<Payment>();
                foreach (var subscription in _unitOfWork.Subscription.GetAll(s => s.CoProducerId == coProducer.Id))
                {
                    subscriptions.Add(subscription);
                    var subscriptionPayments = _unitOfWork.Payment.GetAll(p => p.SubscriptionId == subscription.Id);
                    payments.AddRange(subscriptionPayments);

                    _logger.LogInformation("Added subscription and {PaymentCount} payments to co-producer account - SubscriptionId: {SubscriptionId}, CoProducerId: {CoProducerId} at {Time}",
                        subscriptionPayments.Count(), subscription.Id, coProducer.Id, DateTime.UtcNow);
                }
                result.Add(coProducer.Id, new Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>(coProducer, subscriptions, payments));
            }

            // limpeza - fixing the collection modification during iteration issue
            var keysToRemove = result.Where(cs => cs.Value.Item2.Count() == 0).Select(cs => cs.Key).ToList();
            foreach (var key in keysToRemove)
            {
                result.Remove(key);
            }

            _logger.LogInformation("Completed retrieving co-producer current account data - CoProducerCount: {CoProducerCount} at {Time}",
                result.Count, DateTime.UtcNow);

            return result;
        }
    }
}
