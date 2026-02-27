using E7gezhaa.API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface IAiRecommendationService
    {
        // ترشيح أفضل القاعات بناءً على التقييمات ونوع المناسبة
        Task<IEnumerable<Venue>> RecommendVenuesAsync(string eventType, int locationId);

        // ترشيح موردين بناءً على ميزانية معينة (AI Budgeting)
        Task<IEnumerable<Vendor>> RecommendVendorsByBudgetAsync(decimal maxBudget);
    }
}