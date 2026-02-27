using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E7gezhaa.API.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        // ربط التنبيه باليوزر (صاحب التنبيه)
        [Required]
        public string UserId { get; set; } = string.Empty; // القيمة الافتراضية تمنع الـ Warning

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty; // القيمة الافتراضية تمنع الـ Warning

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // يفضل استخدام UtcNow في التنبيهات

        public bool IsRead { get; set; } = false;

        // نوع التنبيه (اختياري: لتلوين التنبيه في الـ Front-end مثلاً)
        public string Type { get; set; } = "General";
    }
}