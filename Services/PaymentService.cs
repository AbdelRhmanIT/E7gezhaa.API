using E7gezhaa.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(AppDbContext context, ILogger<PaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> ProcessPaymentAsync(Payment payment, string userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            bool result = false;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. التحقق من الحجز وملكيته لليوزر
                    var booking = await _context.Bookings
                        .Include(b => b.Venue)
                        .FirstOrDefaultAsync(b => b.Id == payment.BookingId && b.UserId == userId);

                    if (booking == null)
                    {
                        _logger.LogWarning("Payment attempt for non-existent booking {BookingId} by user {UserId}", payment.BookingId, userId);
                        result = false;
                        return;
                    }

                    if (booking.Status == "Paid")
                    {
                        _logger.LogWarning("Payment attempt for already paid booking {BookingId}", payment.BookingId);
                        result = false;
                        return;
                    }

                    // 2. التحقق من المبلغ المدفوع
                    var expectedDeposit = booking.TotalPrice * (booking.Venue?.DepositPercentage ?? 25) / 100;
                    var minimumPayment = Math.Round(expectedDeposit, 2);
                    var paidAmount = Math.Round(payment.Amount, 2);

                    // لازم يدفع على الأقل العربون أو كامل المبلغ
                    if (paidAmount < minimumPayment)
                    {
                        _logger.LogWarning(
                            "Payment amount {PaidAmount} is less than minimum required {MinimumPayment} for booking {BookingId}",
                            paidAmount, minimumPayment, payment.BookingId);
                        result = false;
                        return;
                    }

                    // 3. لو دفع أكتر من المبلغ الكلي — مش منطقي
                    if (paidAmount > Math.Round(booking.TotalPrice, 2))
                    {
                        _logger.LogWarning(
                            "Payment amount {PaidAmount} exceeds total price {TotalPrice} for booking {BookingId}",
                            paidAmount, booking.TotalPrice, payment.BookingId);
                        result = false;
                        return;
                    }

                    // 4. تسجيل الدفع
                    payment.Status = "Completed";
                    payment.CreatedAt = DateTime.UtcNow;
                    _context.Payments.Add(payment);

                    // 5. تحديث حالة الحجز
                    booking.Status = "Paid";
                    _context.Bookings.Update(booking);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Payment of {Amount} EGP processed successfully for booking {BookingId}",
                        paidAmount, payment.BookingId);

                    result = true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Payment processing error for booking {BookingId}", payment.BookingId);
                    result = false;
                }
            });

            return result;
        }

        public async Task<Payment?> GetPaymentByBookingIdAsync(int bookingId) =>
            await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);
    }
}