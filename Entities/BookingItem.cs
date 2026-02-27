using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class BookingItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        // تم التأكيد على الـ Navigation Property لربطها بالـ List الموجودة في كلاس Booking
        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }

        [Required]
        public string ItemType { get; set; } = string.Empty; // "Venue", "Attire", "Service"

        [Required]
        public int ItemId { get; set; } // ID القاعة أو الفستان

        [Required]
        [Column(TypeName = "decimal(18,2)")] // تأكيد إضافي لمنع تحذير الـ Decimal هنا أيضاً
        public decimal Price { get; set; } // السعر وقت الحجز

        public string? Notes { get; set; }
    }
}