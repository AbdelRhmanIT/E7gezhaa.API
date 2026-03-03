using E7gezhaa.API.Entities;
using E7gezhaa.API.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class AiRecommendationService : IAiRecommendationService
    {
        private readonly AppDbContext _context;
        private readonly OpenAiSettings _openAiSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public AiRecommendationService(
            AppDbContext context,
            IOptions<OpenAiSettings> openAiSettings,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _openAiSettings = openAiSettings.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<Venue>> RecommendVenuesAsync(string eventType, int locationId)
        {
            // 1. جلب القاعات في الموقع المحدد مع تقييماتها
            var venues = await _context.Venues
                .Include(v => v.Reviews)
                .Include(v => v.Images)
                .Where(v => v.LocationId == locationId)
                .ToListAsync();

            if (!venues.Any())
                return Enumerable.Empty<Venue>();

            // 2. لو في OpenAI API Key، نستخدمه للتوصية الذكية
            if (!string.IsNullOrEmpty(_openAiSettings.ApiKey) &&
                _openAiSettings.ApiKey != "YOUR_OPENAI_API_KEY")
            {
                return await RecommendVenuesWithAiAsync(venues, eventType);
            }

            // 3. Fallback: منطق ذكي بدون AI - يرتب بالتقييم والسعة والسعر
            return venues
                .OrderByDescending(v => v.Reviews.Any()
                    ? v.Reviews.Average(r => (double)r.Rating)
                    : 0)
                .ThenByDescending(v => v.Capacity)
                .ThenBy(v => v.PricePerHour)
                .Take(5);
        }

        public async Task<IEnumerable<Vendor>> RecommendVendorsByBudgetAsync(decimal maxBudget)
        {
            // 1. جلب الموردين مع خدماتهم
            var vendors = await _context.Vendors
                .Include(v => v.VendorServices)
                .Include(v => v.Reviews)
                .Where(v => v.VendorServices.Any(s => s.BasePrice <= maxBudget))
                .ToListAsync();

            if (!vendors.Any())
                return Enumerable.Empty<Vendor>();

            // 2. لو في OpenAI، استخدمه
            if (!string.IsNullOrEmpty(_openAiSettings.ApiKey) &&
                _openAiSettings.ApiKey != "YOUR_OPENAI_API_KEY")
            {
                return await RecommendVendorsWithAiAsync(vendors, maxBudget);
            }

            // 3. Fallback: ترتيب بالتقييم وعدد الخدمات في الميزانية
            return vendors
                .OrderByDescending(v => v.Rating)
                .ThenByDescending(v => v.VendorServices.Count(s => s.BasePrice <= maxBudget))
                .Take(5);
        }

        // =================== AI Methods ===================

        private async Task<IEnumerable<Venue>> RecommendVenuesWithAiAsync(
            List<Venue> venues, string eventType)
        {
            try
            {
                // بناء وصف القاعات للـ AI
                var venuesDescription = string.Join("\n", venues.Select((v, i) =>
                    $"{i + 1}. ID:{v.Id} - {v.Name} - السعة:{v.Capacity} - السعر/ساعة:{v.PricePerHour} - " +
                    $"التقييم:{(v.Reviews.Any() ? v.Reviews.Average(r => (double)r.Rating).ToString("F1") : "لا يوجد")}"));

                var prompt = $@"أنت مساعد ذكي لنظام حجز قاعات الأفراح والمناسبات.
المستخدم يبحث عن قاعة لمناسبة: {eventType}

القاعات المتاحة:
{venuesDescription}

رتّب أفضل 5 قاعات مناسبة لهذه المناسبة. 
ردّ فقط بقائمة IDs مرتبة هكذا (بدون أي نص إضافي):
ID1,ID2,ID3,ID4,ID5";

                var rankedIds = await CallOpenAiAsync(prompt);

                if (!string.IsNullOrEmpty(rankedIds))
                {
                    var ids = rankedIds.Trim().Split(',')
                        .Select(id => int.TryParse(id.Trim(), out int parsed) ? parsed : -1)
                        .Where(id => id > 0)
                        .ToList();

                    if (ids.Any())
                    {
                        var result = ids
                            .Select(id => venues.FirstOrDefault(v => v.Id == id))
                            .Where(v => v != null)
                            .Cast<Venue>()
                            .ToList();

                        // نضيف أي قاعات ما اتذكرتش الـ AI
                        var remaining = venues.Where(v => !ids.Contains(v.Id)).ToList();
                        result.AddRange(remaining);
                        return result.Take(5);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI Venues Error]: {ex.Message} - Falling back to smart sort");
            }

            // Fallback
            return venues
                .OrderByDescending(v => v.Reviews.Any() ? v.Reviews.Average(r => (double)r.Rating) : 0)
                .Take(5);
        }

        private async Task<IEnumerable<Vendor>> RecommendVendorsWithAiAsync(
            List<Vendor> vendors, decimal maxBudget)
        {
            try
            {
                var vendorsDescription = string.Join("\n", vendors.Select((v, i) =>
                    $"{i + 1}. ID:{v.Id} - {v.Name} - النوع:{v.VendorType} - التقييم:{v.Rating} - " +
                    $"خدمات ضمن الميزانية:{v.VendorServices.Count(s => s.BasePrice <= maxBudget)}"));

                var prompt = $@"أنت مساعد ذكي لنظام حجز خدمات الأفراح.
الميزانية المتاحة: {maxBudget} جنيه

الموردون المتاحون:
{vendorsDescription}

رتّب أفضل 5 موردين بناءً على التقييم وعدد الخدمات ضمن الميزانية.
ردّ فقط بقائمة IDs هكذا (بدون أي نص إضافي):
ID1,ID2,ID3,ID4,ID5";

                var rankedIds = await CallOpenAiAsync(prompt);

                if (!string.IsNullOrEmpty(rankedIds))
                {
                    var ids = rankedIds.Trim().Split(',')
                        .Select(id => { var s = id.Trim(); return s; })
                        .Where(id => !string.IsNullOrEmpty(id))
                        .ToList();

                    if (ids.Any())
                    {
                        var result = ids
                            .Select(id => vendors.FirstOrDefault(v => v.Id == id))
                            .Where(v => v != null)
                            .Cast<Vendor>()
                            .ToList();

                        var remaining = vendors.Where(v => !ids.Contains(v.Id)).ToList();
                        result.AddRange(remaining);
                        return result.Take(5);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI Vendors Error]: {ex.Message} - Falling back to smart sort");
            }

            return vendors
                .OrderByDescending(v => v.Rating)
                .Take(5);
        }

        private async Task<string> CallOpenAiAsync(string prompt)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _openAiSettings.ApiKey);

            var requestBody = new
            {
                model = _openAiSettings.Model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = 100,
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://api.openai.com/v1/chat/completions", content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenAI API Error: {responseBody}");

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }
    }
}