using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        // 1. حجز القاعات (اختياري)
        public int? VenueId { get; set; }
        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }

        public int? TimeSlotId { get; set; }
        [ForeignKey("TimeSlotId")]
        public TimeSlot? TimeSlot { get; set; }

        // 2. حجز باقات المصورين (اختياري)
        public int? PhotographerPackageId { get; set; }
        [ForeignKey("PhotographerPackageId")]
        public PhotographerPackage? PhotographerPackage { get; set; }

        // 3. حجز باقات التجميل والكوافير (التعديل الجديد - اختياري)
        public int? BeautyPackageId { get; set; }
        [ForeignKey("BeautyPackageId")]
        public BeautyPackage? BeautyPackage { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

        // علاقة بنود الحجز الإضافية (مثل الفساتين أو خدمات فرعية)
        public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    }
}