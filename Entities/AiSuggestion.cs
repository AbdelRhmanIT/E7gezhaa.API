using System;
using System.ComponentModel.DataAnnotations;

namespace E7gezhaa.API.Entities
{
    public class AiSuggestion
    {
        [Key] public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; } // الآن سيراها لأننا أضفنا Booking.cs
        public string SuggestionType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}