using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AMAP.Entities
{
    public class Delivery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SubscriptionId { get; set; }

        [ForeignKey("SubscriptionId")]
        [ValidateNever]
        [JsonIgnore]
        public Subscription? Subscription { get; set; }

        [Required]
        public int DeliveredQuantity { get; set; }
    }
}
