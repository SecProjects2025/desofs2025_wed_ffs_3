using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AMAP.Entities
{
    public class SubscriptionPeriod
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Duration is required.")]
        [StringLength(50, ErrorMessage = "Duration cannot be longer than 50 characters.")]
        public string? Duration { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }

        public IEnumerable<DeliveryDate> DeliveryDates { get; set; } = [];
    }
}
