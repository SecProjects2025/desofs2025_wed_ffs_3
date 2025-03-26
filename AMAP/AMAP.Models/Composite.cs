using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AMAP.Entities
{
    public class Composite
    {
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        [JsonIgnore]
        public Product Product { get; set; }

        public int SubProductId { get; set; }

        [ForeignKey("SubProductId")]
        [ValidateNever]
        [JsonIgnore]
        public Product SubProduct { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
