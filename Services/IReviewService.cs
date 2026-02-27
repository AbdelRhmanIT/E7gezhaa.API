using E7gezhaa.API.Entities;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface IReviewService
    {
        // إضافة تقييم مع التحقق من الحجز
        Task<(bool Success, string Message)> AddReviewAsync(Review review);

        // جلب متوسط التقييم لمورد معين (عشان تظهره في البروفايل)
        Task<decimal> GetAverageRatingAsync(string vendorId);
    }
}