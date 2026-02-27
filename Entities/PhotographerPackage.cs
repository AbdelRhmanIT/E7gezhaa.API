using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class PhotographerPackage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string VendorId { get; set; } = string.Empty; // ربط بالـ Vendor (المصور)

        [ForeignKey("VendorId")]
        public virtual Vendor? Vendor { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // مثال: "باقة الفرح الكاملة"

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int DurationInHours { get; set; } // مدة السيشن

        public bool Available { get; set; } = true;
    }
}