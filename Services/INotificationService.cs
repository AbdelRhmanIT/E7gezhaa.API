using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface INotificationService
    {
        // إرسال إشعار بسيط (لليوزر أو للفيندور)
        Task<bool> SendNotificationAsync(string userId, string message);
    }
}