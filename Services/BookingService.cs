using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using E7gezhaa.API.Entities;
using E7gezhaa.API.DTOs;

namespace E7gezhaa.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public decimal CalculateTotalAmount(decimal basePrice, decimal pricePerHour, DateTime start, DateTime end, string eventType, decimal adjustment)
        {
            var hours = (decimal)(end - start).TotalHours;
            decimal total = basePrice + (pricePerHour * hours) + adjustment;
            if (eventType == "Wedding") total += 1000;
            return total;
        }

        public bool IsValidBookingDate(DateTime startTime) => startTime > DateTime.UtcNow;

        public decimal CalculateDeposit(decimal totalAmount, decimal depositPercentage)
        {
            if (depositPercentage <= 0) return totalAmount * 0.25m;
            return totalAmount * (depositPercentage / 100);
        }

        public async Task<(bool Success, string Message)> AddBookingAsync(Booking booking, int timeSlotId)
        {
            try
            {
                var timeSlot = await _context.TimeSlots.FindAsync(timeSlotId);
                if (timeSlot == null || timeSlot.IsBooked)
                    return (false, "عذراً، هذا الموعد غير متاح أو تم حجزه بالفعل.");

                timeSlot.IsBooked = true;
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return (true, "تم الحجز بنجاح!");
            }
            catch (DbUpdateConcurrencyException)
            {
                return (false, "عذراً، الموعد ده اتحجز في اللحظة دي بالضبط. يرجى اختيار موعد آخر.");
            }
        }

        public async Task<List<BookingDashboardDto>> GetUserBookingsAsync(string userId)
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Venue)
                .Include(b => b.TimeSlot)
                .Include(b => b.PhotographerPackage)
                .Include(b => b.BeautyPackage)
                .Include(b => b.BookingItems)
                .Select(b => new BookingDashboardDto
                {
                    BookingId = b.Id,
                    VenueName = b.Venue != null ? b.Venue.Name : "قاعة غير محددة",
                    StartTime = b.TimeSlot != null ? b.TimeSlot.StartTime : DateTime.MinValue,
                    Status = b.Status ?? "Pending",
                    TotalPrice = b.TotalPrice,
                    CanRate = b.Status == "Completed",

                    PhotographerName = b.PhotographerPackage != null ? b.PhotographerPackage.Name : "لا يوجد",
                    BeautyPackageName = b.BeautyPackage != null ? b.BeautyPackage.Name : "لا يوجد",

                    // تحويل الـ BookingItems لـ List of strings للقراءة
                    ExtraItems = b.BookingItems.Select(bi => $"{bi.ItemType} (Ref: {bi.ItemId})").ToList()
                })
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }
    }
}