using E7gezhaa.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminDashboardController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// إحصائيات عامة للنظام
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalVenues = await _context.Venues.CountAsync();
            var totalBookings = await _context.Bookings.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalVendors = await _context.Vendors.CountAsync();

            var totalRevenue = await _context.Payments
                .Where(p => p.Status == "Completed")
                .SumAsync(p => p.Amount);

            var pendingBookings = await _context.Bookings
                .CountAsync(b => b.Status == "Pending");

            var confirmedBookings = await _context.Bookings
                .CountAsync(b => b.Status == "Paid");

            var cancelledBookings = await _context.Bookings
                .CountAsync(b => b.Status == "Cancelled");

            var totalPhotographerBookings = await _context.Bookings
                .CountAsync(b => b.PhotographerPackageId != null);

            var totalBeautyBookings = await _context.Bookings
                .CountAsync(b => b.BeautyPackageId != null);

            return Ok(new
            {
                Users = new
                {
                    Total = totalUsers,
                    Vendors = totalVendors,
                    Customers = totalUsers - totalVendors
                },
                Venues = new
                {
                    Total = totalVenues,
                    Deleted = await _context.Venues.IgnoreQueryFilters().CountAsync(v => v.IsDeleted)
                },
                Bookings = new
                {
                    Total = totalBookings,
                    Pending = pendingBookings,
                    Confirmed = confirmedBookings,
                    Cancelled = cancelledBookings,
                    Photographer = totalPhotographerBookings,
                    Beauty = totalBeautyBookings
                },
                Revenue = new
                {
                    Total = totalRevenue,
                    Currency = "EGP"
                }
            });
        }

        /// <summary>
        /// إيرادات آخر 30 يوم يومياً
        /// </summary>
        [HttpGet("revenue/daily")]
        public async Task<IActionResult> GetDailyRevenue()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var dailyRevenue = await _context.Payments
                .Where(p => p.Status == "Completed" && p.CreatedAt >= thirtyDaysAgo)
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(p => p.Amount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(dailyRevenue);
        }

        /// <summary>
        /// أكثر القاعات حجزاً
        /// </summary>
        [HttpGet("venues/top")]
        public async Task<IActionResult> GetTopVenues()
        {
            var topVenues = await _context.Bookings
                .Where(b => b.VenueId != null)
                .GroupBy(b => b.VenueId)
                .Select(g => new
                {
                    VenueId = g.Key,
                    BookingsCount = g.Count(),
                    TotalRevenue = g.Sum(b => b.TotalPrice)
                })
                .OrderByDescending(x => x.BookingsCount)
                .Take(10)
                .ToListAsync();

            var venueIds = topVenues.Select(t => t.VenueId).ToList();
            var venues = await _context.Venues
                .Where(v => venueIds.Contains(v.Id))
                .ToListAsync();

            var result = topVenues.Select(t => new
            {
                t.VenueId,
                VenueName = venues.FirstOrDefault(v => v.Id == t.VenueId)?.Name ?? "غير معروف",
                t.BookingsCount,
                t.TotalRevenue
            });

            return Ok(result);
        }

        /// <summary>
        /// أحدث الحجوزات
        /// </summary>
        [HttpGet("bookings/recent")]
        public async Task<IActionResult> GetRecentBookings([FromQuery] int count = 10)
        {
            if (count > 50) count = 50;

            var recentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Venue)
                .Include(b => b.Payment)
                .OrderByDescending(b => b.BookingDate)
                .Take(count)
                .Select(b => new
                {
                    b.Id,
                    UserName = b.User != null ? b.User.FullName : "غير معروف",
                    UserEmail = b.User != null ? b.User.Email : "",
                    VenueName = b.Venue != null ? b.Venue.Name : "خدمة أخرى",
                    b.BookingDate,
                    b.TotalPrice,
                    b.Status,
                    PaymentStatus = b.Payment != null ? b.Payment.Status : "لم يتم الدفع"
                })
                .ToListAsync();

            return Ok(recentBookings);
        }

        /// <summary>
        /// إحصائيات المستخدمين
        /// </summary>
        [HttpGet("users/stats")]
        public async Task<IActionResult> GetUsersStats()
        {
            var last30Days = DateTime.UtcNow.AddDays(-30);
            var last7Days = DateTime.UtcNow.AddDays(-7);

            var newUsersLast30Days = await _context.Users
                .CountAsync(u => u.CreatedAt >= last30Days);

            var newUsersLast7Days = await _context.Users
                .CountAsync(u => u.CreatedAt >= last7Days);

            var topSpenders = await _context.Bookings
                .Where(b => b.Status == "Paid")
                .GroupBy(b => b.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalSpent = g.Sum(b => b.TotalPrice),
                    BookingsCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToListAsync();

            var userIds = topSpenders.Select(t => t.UserId).ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            var topSpendersResult = topSpenders.Select(t => new
            {
                t.UserId,
                UserName = users.FirstOrDefault(u => u.Id == t.UserId)?.FullName ?? "غير معروف",
                t.TotalSpent,
                t.BookingsCount
            });

            return Ok(new
            {
                NewUsersLast7Days = newUsersLast7Days,
                NewUsersLast30Days = newUsersLast30Days,
                TopSpenders = topSpendersResult
            });
        }

        /// <summary>
        /// تغيير حالة حجز (Admin)
        /// </summary>
        [HttpPut("bookings/{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateStatusDto request)
        {
            var validStatuses = new[] { "Pending", "Paid", "Confirmed", "Cancelled", "Completed" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest(new { Message = "حالة غير صحيحة. القيم المتاحة: Pending, Paid, Confirmed, Cancelled, Completed" });

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound(new { Message = "الحجز غير موجود." });

            booking.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"تم تحديث حالة الحجز إلى {request.Status}" });
        }

        /// <summary>
        /// حذف حجز (Soft Delete - Admin)
        /// </summary>
        [HttpDelete("bookings/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound(new { Message = "الحجز غير موجود." });

            booking.IsDeleted = true;
            booking.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم حذف الحجز بنجاح." });
        }
    }

    public class UpdateStatusDto
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "الحالة مطلوبة")]
        public string Status { get; set; } = string.Empty;
    }
}