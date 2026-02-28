using System;
using System.Collections.Generic;

namespace E7gezhaa.API.DTOs
{
    public class BookingDashboardDto
    {
        public int BookingId { get; set; }
        public string? VenueName { get; set; }
        public DateTime StartTime { get; set; }
        public string? Status { get; set; }
        public decimal TotalPrice { get; set; }
        public bool CanRate { get; set; }

        // الخدمات الأساسية
        public string? PhotographerName { get; set; }
        public string? BeautyPackageName { get; set; }

        // الخدمات الإضافية (الـ BookingItems)
        public List<string> ExtraItems { get; set; } = new();
    }
}