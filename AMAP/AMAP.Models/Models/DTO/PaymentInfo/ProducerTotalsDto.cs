namespace AMAP.Entities.Models.DTO.PaymentInfo
{
    public class ProducerTotalsDto
    {
        public UserInfoDto? User { get; set; }

        public Dictionary<int, SubscriptionTotalsDto>? Subscriptions { get; set; }
    }
}
