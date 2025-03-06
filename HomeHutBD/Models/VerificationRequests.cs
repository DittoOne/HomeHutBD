using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeHutBD.Models
{
    [Table("VerificationRequests")]
    public class VerificationRequests
    {
        [Key]
        public int VerificationId { get; set; }  // Maps to verification_id

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required, StringLength(20)]
        public string NidNumber { get; set; }

        [Required]
        public string VerificationStatus { get; set; } = "Pending";  // 'Pending', 'Approved', 'Rejected'

        public DateTime RequestDate { get; set; } = DateTime.Now;

        [ForeignKey("Admin")]
        public int? ApprovedBy { get; set; }

        // Navigation properties
        public Users User { get; set; }
        public Admin Admin { get; set; }
    }
}
