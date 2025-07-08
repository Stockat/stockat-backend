using System;
using System.ComponentModel.DataAnnotations;

namespace Stockat.Core.Entities
{
    public class ChatBotMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Can be user ID or anonymous ID

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } // "user" or "assistant"

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
} 