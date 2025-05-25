using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AMAP.Entities
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        public string? CoProducerId { get; set; }

        [ForeignKey("CoProducerId")]
        [ValidateNever]
        [JsonIgnore]
        public AppUser CoProducer { get; set; }

        public int ProductOfferId { get; set; }

        [ForeignKey(nameof(ProductOfferId))]
        [ValidateNever]
        [JsonIgnore]
        public ProductOffer ProductOffer { get; set; }

        [Required]
        public int ExpectedQuantity { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentType PaymentType { get; set; }

        [Required]
        public bool TermsAccepted { get; set; }

        [Required]
        public DateTime TermsAcceptedAt { get; set; }

        [Required]
        public bool EmailConfirmed { get; set; }

        public DateTime? EmailConfirmedAt { get; set; }

        public string? ConfirmationToken { get; set; }

        public string? DigitalSignature { get; set; }
    }
}


