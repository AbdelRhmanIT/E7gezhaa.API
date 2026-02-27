using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class Vendor
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [ForeignKey("Id")]
        public virtual User? User { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string VendorType { get; set; } = string.Empty;

        // التعديل الجوهري: جعل الـ LocationId اختيارياً (Nullable)
        public int? LocationId { get; set; }

        [ForeignKey("LocationId")]
        public virtual Location? Location { get; set; }

        public string Phone { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Rating { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<VendorService> VendorServices { get; set; } = new List<VendorService>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Venue> Venues { get; set; } = new List<Venue>();
    }
}