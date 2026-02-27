using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using E7gezhaa.API.Entities;
using E7gezhaa.API.DTOs;

namespace E7gezhaa.API.Services
{
    public interface IBookingService
    {
        // العمليات الحسابية
        decimal CalculateTotalAmount(decimal basePrice, decimal pricePerHour, DateTime start, DateTime end, string eventType, decimal adjustment);
        bool IsValidBookingDate(DateTime startTime);
        decimal CalculateDeposit(decimal totalAmount, decimal depositPercentage);

        // عملية الحجز (محمية بالـ Concurrency)
        Task<(bool Success, string Message)> AddBookingAsync(Booking booking, int timeSlotId);

        // عملية جلب بيانات الداشبورد
        Task<List<BookingDashboardDto>> GetUserBookingsAsync(string userId);
    }
}