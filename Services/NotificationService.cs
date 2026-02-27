using System;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class NotificationService : INotificationService
    {
        public async Task<bool> SendNotificationAsync(string userId, string message)
        {
            // حالياً: محاكاة إرسال إشعار
            // مستقبلاً: هنا نضع كود إرسال Email أو SignalR
            Console.WriteLine($"[Notification] To User: {userId}, Message: {message}");

            return await Task.FromResult(true);
        }
    }
}