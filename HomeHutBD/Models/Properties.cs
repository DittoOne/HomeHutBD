using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeHutBD.Models
{
    [Table("Properties")]
    public class Properties
    {
        [Key]
        public int PropertyId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required, StringLength(255)]
        public string Title { get; set; }

        public float AreaSqft { get; set; }

        [Required]
        public string Address { get; set; }

        public int Bathrooms { get; set; }

        public int Bedrooms { get; set; }

        [Required]
        public string Type { get; set; }  // 'Apartment', 'Duplex', 'Building'

        [Required]
        public string Purpose { get; set; }  // 'Rent', 'Buy'

        public string ImageUrl { get; set; }

        public string FloorPlan { get; set; }

        public DateTime LastUpdate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(12,2)")]
        public decimal Price { get; set; }

        // We keep the Column attribute so that the DB column is still named "nid_verification".
        // **Note:** We removed the [ForeignKey("VerificationRequest")] attribute
        // to avoid conflicts with our Fluent API configuration.
        [Column("nid_verification")]
        public int VerificationId { get; set; }

        // Navigation properties
        public Users User { get; set; }
        public VerificationRequests VerificationRequest { get; set; }
    }
}
