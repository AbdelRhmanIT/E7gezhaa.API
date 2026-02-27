using E7gezhaa.API.Entities;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface IPaymentService
    {
        Task<bool> ProcessPaymentAsync(Payment payment);
        Task<Payment?> GetPaymentByBookingIdAsync(int bookingId);
    }
}