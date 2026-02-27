using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        // شيلنا الـ [Required] عشان الـ API يقبل الطلب
        // إحنا بنحقن الـ UserId من الـ Token جوه الكنترولر عشان الأمان
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public string? VendorId { get; set; }

        [ForeignKey("VendorId")]
        public virtual Vendor? Vendor { get; set; }

        public int? VenueId { get; set; }

        [ForeignKey("VenueId")]
        public virtual Venue? Venue { get; set; }

        public int? BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }

        [Range(1, 5)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}