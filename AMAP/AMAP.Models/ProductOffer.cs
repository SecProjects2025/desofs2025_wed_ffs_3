using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AMAP.Entities
{
    public class ProductOffer
    {
        [Key]
        public int Id { get; set; }

        public int SubscriptionPeriodId { get; set; }

        [ForeignKey("SubscriptionPeriodId")]
        [ValidateNever]
        [JsonIgnore]
        public SubscriptionPeriod SubscriptionPeriod { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        [JsonIgnore]
        public Product Product { get; set; }

        [Required]
        [Range(0, (double.MaxValue))]
        public double Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        public DateTime DeliveryDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentType PaymentType { get; set; }
    }
}
