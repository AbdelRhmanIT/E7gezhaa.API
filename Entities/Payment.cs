using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

        // أضفنا "الوصلة" للحجز عشان الـ AppDbContext يشوفها
        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }

        [Column(TypeName = "decimal(18,2)")] // عشان نضمن دقة الفلوس ونلغي الـ Warning
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";
        public string Provider { get; set; } = string.Empty; // Stripe, Fawry
        public string Status { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}