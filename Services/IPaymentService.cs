using E7gezhaa.API.Entities;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface IPaymentService
    {
        // التعديل هنا: إضافة string userId عشان يتطابق مع الـ Implementation
        Task<bool> ProcessPaymentAsync(Payment payment, string userId);

        Task<Payment?> GetPaymentByBookingIdAsync(int bookingId);
    }
}