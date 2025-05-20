using AMAP.Entities.Utilities;

public class ProductOfferDto
{
    public int SubscriptionPeriodId { get; set; }

    public int ProductId { get; set; }

    public double Price { get; set; }

    public int Quantity { get; set; }

    public DateTime DeliveryDate { get; set; }

    public PaymentType PaymentType { get; set; }
}
