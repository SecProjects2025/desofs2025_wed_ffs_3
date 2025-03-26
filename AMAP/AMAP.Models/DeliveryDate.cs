using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AMAP.Entities
{
    public class DeliveryDate
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        public DateTime Date { get; set; }  
        
        public int SubscriptionPeriodId { get; set; }

        [ForeignKey("SubscriptionPeriodId")]
        [ValidateNever]
        [JsonIgnore]
        public SubscriptionPeriod SubscriptionPeriod { get; set; }
    }
}