using E7gezhaa.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        public PaymentService(AppDbContext context) => _context = context;

        public async Task<bool> ProcessPaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Payment?> GetPaymentByBookingIdAsync(int bookingId) =>
            await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);
    }
}