using E7gezhaa.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq; // أضفنا هذا السطر لضمان عمل Average و Any بسلاسة
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class AiRecommendationService : IAiRecommendationService
    {
        private readonly AppDbContext _context;
        public AiRecommendationService(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Venue>> RecommendVenuesAsync(string eventType, int locationId)
        {
            // منطق AI مبدئي: جلب القاعات في الموقع المحدد اللي ليها أعلى تقييمات
            return await _context.Venues
                .Include(v => v.Reviews)
                .Where(v => v.LocationId == locationId)
                .OrderByDescending(v => v.Reviews.Any() ? v.Reviews.Average(r => (double)r.Rating) : 0)
                .Take(5) // ترشيح أفضل 5 فقط
                .ToListAsync();
        }

        public async Task<IEnumerable<Vendor>> RecommendVendorsByBudgetAsync(decimal maxBudget)
        {
            // ترشيح الموردين اللي عندهم خدمات تناسب ميزانية المستخدم
            // التعديل هنا: استخدام BasePrice بدل Price ليتوافق مع الـ Entity
            return await _context.Vendors
                .Include(v => v.VendorServices)
                .Where(v => v.VendorServices.Any(s => s.BasePrice <= maxBudget))
                .OrderByDescending(v => v.Rating)
                .ToListAsync();
        }
    }
}