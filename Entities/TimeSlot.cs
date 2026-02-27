using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class TimeSlot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VenueId { get; set; }

        [ForeignKey("VenueId")]
        public virtual Venue? Space { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsBooked { get; set; } = false;
        public decimal PriceAdjustment { get; set; } = 0;

        [Timestamp]
        public byte[] RowVersion { get; set; } = new byte[0]; // تهيئة افتراضية
    }
}