using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface INotificationService
    {
        // إرسال إشعار + بريد إلكتروني
        Task<bool> SendNotificationAsync(string userId, string message, string? email = null);

        // إرسال بريد إلكتروني مباشر
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
}