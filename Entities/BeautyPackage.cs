using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class BeautyPackage
    {
        [Key]
        public int Id { get; set; }

        // تم إزالة [Required] لضمان قبول الـ Request في Postman 
        // لأن القيمة يتم سحبها من الـ Token في الـ Controller لضمان الأمان
        public string VendorId { get; set; } = string.Empty;

        [ForeignKey("VendorId")]
        public virtual Vendor? Vendor { get; set; }

        [Required(ErrorMessage = "اسم الباقة مطلوب")]
        public string Name { get; set; } = string.Empty; // مثال: باقة عروسة سوبريم

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool Available { get; set; } = true;
    }
}