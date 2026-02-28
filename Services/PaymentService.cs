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
            // بدء الـ Transaction لضمان سلامة الداتا (ACID)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // التحقق من الحجز والتأكد من ملكيته لليوزر
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.Id == payment.BookingId && b.UserId == userId);

                if (booking == null || booking.Status == "Paid")
                    return false;

                // إضافة عملية الدفع
                _context.Payments.Add(payment);

                // تحديث حالة الحجز
                booking.Status = "Paid";
                _context.Bookings.Update(booking);

                // حفظ التغييرات
                await _context.SaveChangesAsync();

                // التأكيد النهائي
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                // في حالة أي خطأ، بنرجع الداتا زي ما كانت
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<Payment?> GetPaymentByBookingIdAsync(int bookingId) =>
            await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);
    }
}