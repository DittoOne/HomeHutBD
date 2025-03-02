using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeHutBD.Models
{
    [Table("Chats")]
    public class Chats
    {
        [Key]
        public int ChatId { get; set; }

        [ForeignKey("Sender")]
        public int SenderId { get; set; }

        [ForeignKey("Receiver")]
        public int ReceiverId { get; set; }

        [ForeignKey("Property")]
        public int PropertyId { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Navigation properties
        public Users Sender { get; set; }
        public Users Receiver { get; set; }
        public Properties Property { get; set; }
    }
}
