using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMAP.Business.Interfaces;
using AMAP.Entities.Models.DTO.PaymentInfo;

namespace AMAP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(UserType.Manager))]
    public class PaymentsInfoController : Controller
    {
        private readonly IPaymentsInfoBc _paymentsInfoBc;

        public PaymentsInfoController(IPaymentsInfoBc paymentsInfoBc)
        {
            _paymentsInfoBc = paymentsInfoBc;
        }

        [HttpGet("GetCoProducerPayments")]
        public IActionResult GetCoProducerPayments()
        {
            var result = _paymentsInfoBc.GetCoProducerPayments();

            if (result.Count == 0)
            {
                return NotFound("No co-producer payments found.");
            }

            var mappedResult = result.ToDictionary(
                kvp => kvp.Key,
                kvp => new PaymentInfoDto
                {
                    User = new UserInfoDto { Name = kvp.Value.Item1.Name },

                    // Flatten all subscriptions from the nested dictionary
                    Subscriptions = kvp.Value.Item2.Values
                        .Select(tuple => new SubscriptionDto
                        {
                            ProductOfferId = tuple.Item1.ProductOfferId,
                            ExpectedQuantity = tuple.Item1.ExpectedQuantity,
                            PaymentType = tuple.Item1.PaymentType
                        }),

                    // Flatten all payments from all subscriptions
                    Payments = kvp.Value.Item2.Values
                        .SelectMany(tuple => tuple.Item2)
                        .Select(p => new PaymentDto
                        {
                            SubscriptionId = p.SubscriptionId,
                            AmountPaid = p.AmountPaid,
                            PaymentDate = p.PaymentDate
                        })
                }
            );

            return Ok(mappedResult);
        }

        [HttpGet("GetProducerPayments")]
        public IActionResult GetProducerPayments()
        {
            var result = _paymentsInfoBc.GetProducerPayments();

            if (result.Count == 0)
            {
                return NotFound("No producer payments found.");
            }

            var mappedResult = result.ToDictionary(
                kvp => kvp.Key,
                kvp => new PaymentInfoDto
                {
                    User = new UserInfoDto { Name = kvp.Value.Item1.Name },

                    // Flatten all subscriptions from the nested dictionary
                    Subscriptions = kvp.Value.Item2.Values
                        .Select(tuple => new SubscriptionDto
                        {
                            ProductOfferId = tuple.Item1.ProductOfferId,
                            ExpectedQuantity = tuple.Item1.ExpectedQuantity,
                            PaymentType = tuple.Item1.PaymentType
                        }),

                    // Flatten all payments from all subscriptions
                    Payments = kvp.Value.Item2.Values
                        .SelectMany(tuple => tuple.Item2)
                        .Select(p => new PaymentDto
                        {
                            SubscriptionId = p.SubscriptionId,
                            AmountPaid = p.AmountPaid,
                            PaymentDate = p.PaymentDate
                        })
                }
            );

            return Ok(mappedResult);
        }

        [HttpGet("GetProducerToReceive")]
        public IActionResult GetProducerToReceive()
        {
            var result = _paymentsInfoBc.GetProducerToReceive();

            if (result.Count == 0)
            {
                return NotFound("No producer payments to receive found.");
            }

            return Ok(result);
        }

        [HttpGet("GetCoProducerToPay")]
        public IActionResult GetCoProducerToPay()
        {
            var result = _paymentsInfoBc.GetCoProducerToPay();

            if (result.Count == 0)
            {
                return NotFound("No co-producer payments to pay found.");
            }

            return Ok(result);
        }

        [HttpGet("GetProducerTotals")]
        public IActionResult GetProducerPaymentsTotals()
        {
            var result = _paymentsInfoBc.GetProducerTotals();

            if (result.Count == 0)
            {
                return NotFound("No producer totals found.");
            }

            var producerCCData = _paymentsInfoBc.GetProducerCC();

            var mappedResult = result.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    Name = producerCCData.ContainsKey(kvp.Key) ? producerCCData[kvp.Key].Item1.Name : "Unknown Producer",
                    ExpectedAmount = kvp.Value.Item1,
                    ReceiveAmount = kvp.Value.Item2
                }
            );

            return Ok(mappedResult);
        }

        [HttpGet("GetCoProducerTotals")]
        public IActionResult GetCoProducerPaymentsTotals()
        {
            var result = _paymentsInfoBc.GetCoProducerTotals();

            if (result.Count == 0)
            {
                return NotFound("No co-producer totals found.");
            }

            var coProducerCCData = _paymentsInfoBc.GetCoProducerCC();

            var mappedResult = result.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    Name = coProducerCCData.ContainsKey(kvp.Key) ? coProducerCCData[kvp.Key].Item1.Name : "Unknown Co-Producer",
                    ExpectedAmount = kvp.Value.Item1,
                    PaidAmount = kvp.Value.Item2
                }
            );

            return Ok(mappedResult);
        }

        [HttpGet("GetProducerCC")]
        public IActionResult GetProducerCC()
        {
            var result = _paymentsInfoBc.GetProducerCC();

            if (result.Count == 0)
            {
                return NotFound("No producer data found.");
            }

            var mappedResult = result.ToDictionary(
                kvp => kvp.Key,
                kvp => new PaymentInfoDto
                {
                    User = new UserInfoDto { Name = kvp.Value.Item1.Name },
                    Subscriptions = kvp.Value.Item2.Select(s => new SubscriptionDto 
                    {
                        ProductOfferId = s.ProductOfferId,
                        ExpectedQuantity = s.ExpectedQuantity,
                        PaymentType = s.PaymentType
                    }),
                    Payments = kvp.Value.Item3.Select(p => new PaymentDto
                    {
                        SubscriptionId = p.SubscriptionId,
                        AmountPaid = p.AmountPaid,
                        PaymentDate = p.PaymentDate
                    })
                }
            );

            return Ok(mappedResult);
        }

        [HttpGet("GetCoProducerCC")]
        public IActionResult GetCoProducerCC()
        {
            var result = _paymentsInfoBc.GetCoProducerCC();

            if (result.Count == 0)
            {
                return NotFound("No co-producer data found.");
            }

            var mappedResult = result.ToDictionary(
                kvp => kvp.Key,
                kvp => new PaymentInfoDto
                {
                    User = new UserInfoDto { Name = kvp.Value.Item1.Name },
                    Subscriptions = kvp.Value.Item2.Select(s => new SubscriptionDto
                    {
                        ProductOfferId = s.ProductOfferId,
                        ExpectedQuantity = s.ExpectedQuantity,
                        PaymentType = s.PaymentType
                    }),
                    Payments = kvp.Value.Item3.Select(p => new PaymentDto
                    {
                        SubscriptionId = p.SubscriptionId,
                        AmountPaid = p.AmountPaid,
                        PaymentDate = p.PaymentDate
                    })
                }
            );

            return Ok(mappedResult);
        }
    }
}
