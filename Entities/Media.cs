using System;

public class Media
{
    public int Id { get; set; }
    public string OwnerType { get; set; } = string.Empty; // "Venue", "Attire", "Service"
    public int OwnerId { get; set; } // ID الحاجه اللي الصورة تبعاها
    public string Url { get; set; } = string.Empty;
    public string? Label { get; set; } // مثلاً "واجهة القاعة", "صورة الفستان من الخلف"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}