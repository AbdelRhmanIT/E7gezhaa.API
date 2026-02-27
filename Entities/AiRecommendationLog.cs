using System;

public class AiRecommendationLog
{
    public int Id { get; set; }
    public int ModelId { get; set; } // الموديل اللي طلع التوصية
    public int BookingId { get; set; } // الحجز المرتبط بالتوصية
    public string RecommendationText { get; set; } = string.Empty; // النص اللي ظهر لليوزر
    public decimal ConfidenceScore { get; set; } // مدى ثقة الـ AI في التوصية دي
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}