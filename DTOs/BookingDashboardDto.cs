using System;

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
    }
}