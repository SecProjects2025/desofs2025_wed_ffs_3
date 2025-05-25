using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMAP.Entities
{
    public class PaymentValidationHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PaymentId { get; set; }

        [ForeignKey("PaymentId")]
        public Payment Payment { get; set; } // Reference to the Payment that was validated

        [Required]
        public string? ProducerId { get; set; }

        // Note: Requires a Producer entity for ForeignKey if implemented
        // [ForeignKey("ProducerId")]
        // public Producer Producer { get; set; } // Reference to the Producer who validated

        [Required]
        public DateTime ValidationTimestamp { get; set; }

        // Optional: could add a field for the state change or comments
        // public string? Notes { get; set; }
    }
}