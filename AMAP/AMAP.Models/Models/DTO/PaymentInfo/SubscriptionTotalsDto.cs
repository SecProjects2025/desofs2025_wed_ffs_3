using AMAP.Entities.Models.DTO.Subscription;

namespace AMAP.Entities.Models.DTO.PaymentInfo
{
    public class SubscriptionTotalsDto
    {
        public SubscriptionDto? Subscription { get; set; }

        public IEnumerable<PaymentDto>? Payments { get; set; }
    }
}
