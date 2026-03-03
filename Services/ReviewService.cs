using E7gezhaa.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context) => _context = context;

        public async Task<(bool Success, string Message)> AddReviewAsync(Review review)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            bool success = false;
            string message = "";

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // التأكد من وجود حجز صحيح لليوزر
                    var bookingExists = await _context.Bookings
                        .AnyAsync(b => b.Id == review.BookingId && b.UserId == review.UserId);

                    if (!bookingExists)
                    {
                        success = false;
                        message = "عذراً، لا يمكنك تقييم خدمة لم تقم بحجزها.";
                        return;
                    }

                    // منع التكرار
                    var alreadyReviewed = await _context.Reviews
                        .AnyAsync(r => r.BookingId == review.BookingId);

                    if (alreadyReviewed)
                    {
                        success = false;
                        message = "لقد قمت بتقييم هذا الحجز مسبقاً.";
                        return;
                    }

                    _context.Reviews.Add(review);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    success = true;
                    message = "تم إضافة التقييم بنجاح.";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    success = false;
                    message = ex.InnerException?.Message ?? ex.Message;
                }
            });

            return (success, message);
        }

        public async Task<decimal> GetAverageRatingAsync(string vendorId)
        {
            var reviews = _context.Reviews.Where(r => r.VendorId == vendorId);
            if (!await reviews.AnyAsync()) return 0;
            return await reviews.AverageAsync(r => r.Rating);
        }
    }
}