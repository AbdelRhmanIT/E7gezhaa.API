using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        // Dependency Injection للخدمة بدلاً من الـ Context مباشرة
        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// إضافة تقييم جديد بناءً على حجز حقيقي (النخاع الشوكي)
        /// </summary>
        [HttpPost("add-review")]
        [Authorize]
        public async Task<IActionResult> AddReview([FromBody] Review review)
        {
            // 1. سحب معرف المستخدم الحالي من الـ Token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("يجب تسجيل الدخول لإضافة تقييم.");

            // 2. حقن الـ UserId في كائن التقييم لضمان الأمان (عدم تزوير الهوية)
            review.UserId = userId;

            // 3. نطلب من الـ Service معالجة المنطق (التحقق من الحجز، منع التكرار، الحفظ)
            var (success, message) = await _reviewService.AddReviewAsync(review);

            if (!success)
            {
                return BadRequest(new { Message = message });
            }

            return Ok(new { Message = message });
        }

        /// <summary>
        /// جلب متوسط التقييم لمورد معين (قاعة، مصور، ميك أب)
        /// </summary>
        [HttpGet("average/{vendorId}")]
        public async Task<IActionResult> GetAverageRating(string vendorId)
        {
            var average = await _reviewService.GetAverageRatingAsync(vendorId);
            return Ok(new { VendorId = vendorId, AverageRating = average });
        }
    }
}