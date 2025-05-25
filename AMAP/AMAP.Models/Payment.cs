using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using AMAP.Entities.Utilities;

namespace AMAP.Entities
{
    public class Payment
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
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be non-negative.")]
        public double AmountPaid { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public bool Validated { get; set; } = false;

        public DateTime? ValidationDate { get; set; }

        public string? ValidatedByProducerId { get; set; }

        // Note: Requires a Producer entity for ForeignKey if implemented
        // [ForeignKey("ValidatedByProducerId")]
        // [ValidateNever]
        // [JsonIgnore]
        // public Producer? ValidatedByProducer { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentType PaymentMethod { get; set; }
    }
}
