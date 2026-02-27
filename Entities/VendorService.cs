using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class VendorService
    {
        [Key] public int Id { get; set; }

        // التعديل: تغيير النوع لـ string ليتوافق مع الـ Vendor الجديد والـ Identity
        [Required]
        public string VendorId { get; set; } = string.Empty;

        [ForeignKey("VendorId")]
        public virtual Vendor? Vendor { get; set; }

        public string ServiceCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")] // لإلغاء الـ Warning بتاع الدقة العشرية
        public decimal BasePrice { get; set; }

        public string Category { get; set; } = string.Empty;
    }
}