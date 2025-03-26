using AMAP.Entities.Utilities;
using AMAP.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMAP.DataAccess.Repository.IRepository;

namespace AMAP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(UserType.Manager))]
    public class PaymentsInfoController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentsInfoController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // numero 5 -> arvore e resultado
        [HttpGet("GetProducerPayments")]
        public IActionResult GetProducerPayments()
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
            foreach (var producerSubscriptions in result)
            {
                if (producerSubscriptions.Value.Item2.Count == 0)
                {
                    result.Remove(producerSubscriptions.Key);
                }
            }

            // FRONTEND PERCORRER LISTA E SOMAR 
            return Ok(result);
        }

        [HttpGet("GetCoProducerPayments")]
        public IActionResult GetCoProducerPayments()
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

            // limpeza
            foreach (var coProducerSubscriptions in result)
            {
                if (coProducerSubscriptions.Value.Item2.Count() == 0)
                {
                    result.Remove(coProducerSubscriptions.Key);
                }
            }

            // FRONTEND PERCORRER LISTA E SOMAR
            return Ok(result);
        }

        [HttpGet("GetProducerToReceive")]
        public IActionResult GetProducerToReceive()
        {
            var totals = GetProducerTotals();
            var result = new Dictionary<string, double>();
            foreach (var user in totals)
            {
                result[user.Key] = user.Value.Item1 - user.Value.Item2;
            }

            return Ok(result);
        }

        [HttpGet("GetCoProducerToPay")]
        public IActionResult GetCoProducerToPay()
        {
            var totals = GetCoProducerTotals();
            var result = new Dictionary<string, double>();
            foreach (var user in totals)
            {
                result[user.Key] = user.Value.Item1 - user.Value.Item2;
            }

            return Ok(result);
        }

        // numero 8 -> listagens e totais
        [HttpGet("GetProducerCC")]
        public IActionResult GetProducerCC()
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

            // limpeza
            foreach (var producerSubscriptions in result)
            {
                if (producerSubscriptions.Value.Item2.Count() == 0)
                {
                    result.Remove(producerSubscriptions.Key);
                }
            }

            return Ok(result);
        }

        [HttpGet("GetCoProducerCC")]
        public IActionResult GetCoProducerCC()
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

            // limpeza
            foreach (var coProducerSubscriptions in result)
            {
                if (coProducerSubscriptions.Value.Item2.Count() == 0)
                {
                    result.Remove(coProducerSubscriptions.Key);
                }
            }

            // FRONTEND PERCORRER LISTA E SOMAR
            return Ok(result);
        }

        [HttpGet("GetProducerTotals")]
        public IActionResult GetProducerPaymentsTotals()
        {
            return Ok(GetProducerTotals());
        }

        [HttpGet("GetCoProducerTotals")]
        public IActionResult GetCoProducerPaymentsTotals()
        {
            return Ok(GetCoProducerTotals());
        }

        private Dictionary<string, Tuple<double, double>> GetProducerTotals()
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

        private Dictionary<string, Tuple<double, double>> GetCoProducerTotals()
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
    }
}
