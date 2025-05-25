namespace AMAP.Entities.Models.DTO.PaymentInfo
{
    public class PaymentDto
    {
        public int SubscriptionId { get; set; }

        public double AmountPaid { get; set; }

        public DateTime PaymentDate { get; set; }
    }
}
