using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class VenueImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        // الربط بالقاعة
        [Required]
        public int VenueId { get; set; }

        [ForeignKey("VenueId")]
        // غيرنا الاسم من Space لـ Venue عشان يطابق الـ AppDbContext اللي لسه باعتينه
        public Venue? Space { get; set; }
    }
}