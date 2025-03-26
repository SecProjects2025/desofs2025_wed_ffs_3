using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AMAP.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [ValidateNever]
        public string ProducerId { get; set; }

        [ForeignKey("ProducerId")]
        [ValidateNever]
        [JsonIgnore]
        public AppUser Producer { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot be longer than 100 characters.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Product description is required.")]
        [StringLength(500, ErrorMessage = "Product description cannot be longer than 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Product photo is required.")]
        public string? Photo { get; set; }

        [Required(ErrorMessage = "Reference price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Reference price must be greater than zero.")]
        public double ReferencePrice { get; set; }

        [Required(ErrorMessage = "Delivery units are required.")]
        public string? DeliveryUnits { get; set; }

        public ICollection<Composite> Composites { get; set; } = [];

        [JsonIgnore]
        public bool IsDeleted { get; set; } = false;
    }
}
