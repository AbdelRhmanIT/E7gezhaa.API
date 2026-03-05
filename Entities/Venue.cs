using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class Venue
    {
        [Key]
        public int Id { get; set; }

        public string? VendorId { get; set; }
        [ForeignKey("VendorId")]
        public virtual Vendor? Vendor { get; set; }

        public int? LocationId { get; set; }
        [ForeignKey("LocationId")]
        public virtual Location? DetailedLocation { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string? Features { get; set; }
        public string? WebsiteUrl { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public decimal PricePerHour { get; set; }

        [NotMapped]
        public decimal BasePrice
        {
            get => PricePerHour;
            set => PricePerHour = value;
        }

        public decimal? WeekendPrice { get; set; }
        public decimal DepositPercentage { get; set; } = 25.0m;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        // ✅ Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // العلاقات
        public virtual ICollection<VenueImage> Images { get; set; } = new List<VenueImage>();
        public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}