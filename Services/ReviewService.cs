using E7gezhaa.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context) => _context = context;

        public async Task<(bool Success, string Message)> AddReviewAsync(Review review)
        {
            // 1. التأكد من وجود حجز مؤكد وصحيح لليوزر ده
            var booking = await _context.Bookings
                .AnyAsync(b => b.Id == review.BookingId && b.UserId == review.UserId);

            if (!booking)
                return (false, "عذراً، لا يمكنك تقييم خدمة لم تقم بحجزها.");

            // 2. منع التكرار (تقييم واحد لكل حجز)
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.BookingId == review.BookingId);

            if (alreadyReviewed)
                return (false, "لقد قمت بتقييم هذا الحجز مسبقاً.");

            // 3. الحفظ
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return (true, "تم إضافة التقييم بنجاح.");
        }

        public async Task<decimal> GetAverageRatingAsync(string vendorId)
        {
            var reviews = _context.Reviews.Where(r => r.VendorId == vendorId);
            if (!await reviews.AnyAsync()) return 0;

            return await reviews.AverageAsync(r => r.Rating);
        }
    }
}