using E7gezhaa.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context) => _context = context;

        public async Task<bool> ProcessPaymentAsync(Payment payment, string userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            bool result = false;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // التحقق من الحجز وملكيته لليوزر
                    var booking = await _context.Bookings
                        .FirstOrDefaultAsync(b => b.Id == payment.BookingId && b.UserId == userId);

                    if (booking == null || booking.Status == "Paid")
                    {
                        result = false;
                        return;
                    }

                    // إضافة عملية الدفع
                    _context.Payments.Add(payment);

                    // تحديث حالة الحجز
                    booking.Status = "Paid";
                    _context.Bookings.Update(booking);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    result = true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Payment Error: {ex.InnerException?.Message ?? ex.Message}");
                    result = false;
                }
            });

            return result;
        }

        public async Task<Payment?> GetPaymentByBookingIdAsync(int bookingId) =>
            await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);
    }
}