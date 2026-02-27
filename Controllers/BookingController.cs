using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E7gezhaa.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IBookingService _bookingService;

        public BookingController(AppDbContext context, IBookingService bookingService)
        {
            _context = context;
            _bookingService = bookingService;
        }

        // 1. الإضافة الجديدة: جلب حجوزات المستخدم للداشبورد
        [HttpGet("my-dashboard")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("يجب تسجيل الدخول");

            // ننده على الميثود اللي أضفناها في الـ BookingService
            var bookings = await _bookingService.GetUserBookingsAsync(userId);

            return Ok(bookings);
        }

        // 2. الكود الأساسي بتاعك: ConfirmBooking (بدون أي تعديل)
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmBooking([FromBody] BookingRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("يجب تسجيل الدخول");

            var venue = await _context.Venues.FindAsync(request.VenueId);
            var slot = await _context.TimeSlots.FirstOrDefaultAsync(s => s.Id == request.SlotId && s.VenueId == request.VenueId);

            if (venue == null || slot == null || slot.IsBooked)
                return BadRequest("الموعد غير متاح أو البيانات خاطئة");

            if (!_bookingService.IsValidBookingDate(slot.StartTime))
                return BadRequest("لا يمكن الحجز في تاريخ قديم");

            decimal totalPrice = _bookingService.CalculateTotalAmount(
                venue.BasePrice, venue.PricePerHour, slot.StartTime, slot.EndTime, request.EventType, slot.PriceAdjustment);

            decimal deposit = _bookingService.CalculateDeposit(totalPrice, venue.DepositPercentage);

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    slot.IsBooked = true;

                    var booking = new Booking
                    {
                        UserId = userId,
                        VenueId = request.VenueId,
                        TimeSlotId = request.SlotId,
                        BookingDate = DateTime.UtcNow,
                        TotalPrice = totalPrice,
                        Status = "Pending"
                    };

                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        BookingId = booking.Id,
                        Total = totalPrice,
                        DepositRequired = deposit,
                        Message = "تم إنشاء الحجز بنجاح، يرجى دفع العربون لتأكيده"
                    });
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    return Conflict("عذراً، هذا الموعد تم حجزه في اللحظة الحالية من قبل مستخدم آخر. يرجى اختيار موعد آخر.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"حدث خطأ أثناء معالجة الحجز: {ex.Message}");
                }
            });
        }
    }

    public class BookingRequestDto
    {
        public int VenueId { get; set; }
        public int SlotId { get; set; }
        public string EventType { get; set; } = "Wedding";
    }
}