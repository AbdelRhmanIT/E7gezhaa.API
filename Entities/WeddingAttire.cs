using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class WeddingAttire
    {
        [Key]
        public int Id { get; set; }

        // التعديل الجوهري: تم التغيير من int لـ string ليطابق الـ Identity User Id
        [Required]
        public string VendorId { get; set; } = string.Empty;

        [ForeignKey("VendorId")]
        public virtual Vendor? Vendor { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty; // Dress, Suit

        public string Size { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public string RentalOrSale { get; set; } = "Rental";

        // تثبيت الدقة العشرية للسعر لمنع تحذيرات الـ Migration
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public bool Available { get; set; } = true;
    }
}