using AMAP.Entities.Models.DTO.Subscription;

namespace AMAP.Entities.Models.DTO.PaymentInfo
{
    public class PaymentInfoDto
    {
        public UserInfoDto? User { get; set; }

        public IEnumerable<SubscriptionDto>? Subscriptions { get; set; }

        public IEnumerable<PaymentDto>? Payments { get; set; }
    }
}
