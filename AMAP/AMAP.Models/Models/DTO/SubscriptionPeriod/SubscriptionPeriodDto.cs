namespace AMAP.Entities.Models.DTO.SubscriptionPeriod
{
    public class SubscriptionPeriodDto
    {
        public string? Duration { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public IEnumerable<DeliveryDateDto>? DeliveryDates { get; set; }
    }
}
 
