using AMAP.Entities.Utilities;
using System.Text.Json.Serialization;

public class SubscriptionDto
{
    public int ProductOfferId { get; set; }

    public int ExpectedQuantity { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentType PaymentType { get; set; }
}
