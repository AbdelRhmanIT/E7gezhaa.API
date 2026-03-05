using E7gezhaa.API.DTOs;
using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly AppDbContext _context;

        public PaymentsController(IPaymentService paymentService, AppDbContext context)
        {
            _paymentService = paymentService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Pay([FromBody] PaymentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "يجب تسجيل الدخول لإتمام عملية الدفع." });

            // التحقق من الحجز وإرجاع تفاصيل المبلغ
            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.UserId == userId);

            if (booking == null)
                return NotFound(new { Message = "الحجز غير موجود أو لا ينتمي لحسابك." });

            if (booking.Status == "Paid")
                return BadRequest(new { Message = "هذا الحجز مدفوع بالفعل." });

            var depositPercentage = booking.Venue?.DepositPercentage ?? 25;
            var minimumPayment = Math.Round(booking.TotalPrice * depositPercentage / 100, 2);

            if (request.Amount < minimumPayment)
                return BadRequest(new
                {
                    Message = $"المبلغ المدفوع أقل من الحد الأدنى المطلوب.",
                    MinimumRequired = minimumPayment,
                    TotalPrice = booking.TotalPrice,
                    DepositPercentage = depositPercentage,
                    PaidAmount = request.Amount
                });

            if (request.Amount > booking.TotalPrice)
                return BadRequest(new
                {
                    Message = "المبلغ المدفوع أكبر من إجمالي الحجز.",
                    TotalPrice = booking.TotalPrice,
                    PaidAmount = request.Amount
                });

            var payment = new Payment
            {
                BookingId = request.BookingId,
                Amount = request.Amount,
                Currency = request.Currency,
                Provider = request.Provider,
                Status = "Pending",
                TransactionId = request.TransactionId ?? "",
                CreatedAt = DateTime.UtcNow
            };

            var success = await _paymentService.ProcessPaymentAsync(payment, userId);

            return success
                ? Ok(new
                {
                    Message = "تم تسجيل عملية الدفع بنجاح.",
                    Amount = request.Amount,
                    BookingStatus = "Paid"
                })
                : BadRequest(new { Message = "فشل تسجيل الدفع. حاول مرة أخرى." });
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetPaymentByBooking(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null)
                return NotFound(new { Message = "الحجز غير موجود." });

            var payment = await _paymentService.GetPaymentByBookingIdAsync(bookingId);
            if (payment == null)
                return NotFound(new { Message = "لا يوجد دفع لهذا الحجز." });

            return Ok(payment);
        }
    }
}