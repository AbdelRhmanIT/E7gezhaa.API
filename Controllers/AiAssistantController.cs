using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E7gezhaa.API.Controllers
{
    [Authorize] // القفل: لازم المستخدم يكون مسجل دخول عشان يستعمل مساعد الذكاء الاصطناعي
    [Route("api/[controller]")]
    [ApiController]
    public class AiAssistantController : ControllerBase
    {
        private readonly IAiRecommendationService _aiService;
        public AiAssistantController(IAiRecommendationService aiService) => _aiService = aiService;

        [HttpGet("recommend-venues")]
        public async Task<IActionResult> GetVenues(string eventType, int locationId)
        {
            // سحب ID اليوزر من الـ Token للتأكد من هوية السائل
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var recommendations = await _aiService.RecommendVenuesAsync(eventType, locationId);

            return Ok(new
            {
                UserId = userId,
                Message = "الذكاء الاصطناعي يرشح لك هذه الأماكن بناءً على التقييمات والموقع",
                Data = recommendations
            });
        }

        [HttpGet("recommend-vendors")]
        public async Task<IActionResult> GetVendors(decimal maxBudget)
        {
            // ترشيح الموردين بناءً على الميزانية اللي حددناها في الـ Service
            var recommendations = await _aiService.RecommendVendorsByBudgetAsync(maxBudget);

            return Ok(new
            {
                Message = $"إليك أفضل الموردين الذين يناسبون ميزانية قدرها {maxBudget}",
                Data = recommendations
            });
        }
    }
}