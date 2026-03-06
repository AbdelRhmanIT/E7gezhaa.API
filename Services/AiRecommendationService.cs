using E7gezhaa.API.Entities;
using E7gezhaa.API.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace E7gezhaa.API.Services
{
    public class AiRecommendationService : IAiRecommendationService
    {
        private readonly AppDbContext _context;
        private readonly OpenAiSettings _openAiSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AiRecommendationService> _logger;
        private const int MaxRetries = 3;

        public AiRecommendationService(
            AppDbContext context,
            IOptions<OpenAiSettings> openAiSettings,
            IHttpClientFactory httpClientFactory,
            ILogger<AiRecommendationService> logger)
        {
            _context = context;
            _openAiSettings = openAiSettings.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Venue>> RecommendVenuesAsync(string eventType, int locationId)
        {
            var venues = await _context.Venues
                .Include(v => v.Reviews)
                .Include(v => v.Images)
                .Where(v => v.LocationId == locationId)
                .ToListAsync();

            if (!venues.Any())
            {
                _logger.LogInformation("No venues found for locationId {LocationId}", locationId);
                return Enumerable.Empty<Venue>();
            }

            if (IsAiEnabled())
                return await RecommendVenuesWithAiAsync(venues, eventType);

            _logger.LogInformation("AI not configured - using smart fallback for venues");
            return venues
                .OrderByDescending(v => v.Reviews.Any() ? v.Reviews.Average(r => (double)r.Rating) : 0)
                .ThenByDescending(v => v.Capacity)
                .ThenBy(v => v.PricePerHour)
                .Take(5);
        }

        public async Task<IEnumerable<Vendor>> RecommendVendorsByBudgetAsync(decimal maxBudget)
        {
            var vendors = await _context.Vendors
                .Include(v => v.VendorServices)
                .Include(v => v.Reviews)
                .Where(v => v.VendorServices.Any(s => s.BasePrice <= maxBudget))
                .ToListAsync();

            if (!vendors.Any())
            {
                _logger.LogInformation("No vendors found within budget {MaxBudget}", maxBudget);
                return Enumerable.Empty<Vendor>();
            }

            if (IsAiEnabled())
                return await RecommendVendorsWithAiAsync(vendors, maxBudget);

            _logger.LogInformation("AI not configured - using smart fallback for vendors");
            return vendors
                .OrderByDescending(v => v.Rating)
                .ThenByDescending(v => v.VendorServices.Count(s => s.BasePrice <= maxBudget))
                .Take(5);
        }

        // =================== AI Methods ===================

        private async Task<IEnumerable<Venue>> RecommendVenuesWithAiAsync(List<Venue> venues, string eventType)
        {
            try
            {
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

                var rankedIds = await CallOpenAiWithRetryAsync(prompt);

                if (!string.IsNullOrEmpty(rankedIds))
                {
                    var ids = rankedIds.Trim().Split(',')
                        .Select(id => int.TryParse(id.Trim(), out int parsed) ? parsed : -1)
                        .Where(id => id > 0)
                        .ToList();

                    if (ids.Any())
                    {
                        // حفظ التوصية في الداتابيز
                        await SaveAiSuggestionAsync("venue", eventType, rankedIds);

                        var result = ids
                            .Select(id => venues.FirstOrDefault(v => v.Id == id))
                            .Where(v => v != null)
                            .Cast<Venue>()
                            .ToList();

                        var remaining = venues.Where(v => !ids.Contains(v.Id)).ToList();
                        result.AddRange(remaining);
                        return result.Take(5);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI venues recommendation failed for eventType {EventType} - using fallback", eventType);
            }

            return venues
                .OrderByDescending(v => v.Reviews.Any() ? v.Reviews.Average(r => (double)r.Rating) : 0)
                .Take(5);
        }

        private async Task<IEnumerable<Vendor>> RecommendVendorsWithAiAsync(List<Vendor> vendors, decimal maxBudget)
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

                var rankedIds = await CallOpenAiWithRetryAsync(prompt);

                if (!string.IsNullOrEmpty(rankedIds))
                {
                    var ids = rankedIds.Trim().Split(',')
                        .Select(id => id.Trim())
                        .Where(id => !string.IsNullOrEmpty(id))
                        .ToList();

                    if (ids.Any())
                    {
                        await SaveAiSuggestionAsync("vendor", $"budget:{maxBudget}", rankedIds);

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
                _logger.LogError(ex, "AI vendors recommendation failed for budget {MaxBudget} - using fallback", maxBudget);
            }

            return vendors.OrderByDescending(v => v.Rating).Take(5);
        }

        // =================== Retry Logic ===================

        private async Task<string> CallOpenAiWithRetryAsync(string prompt)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Calling OpenAI API - Attempt {Attempt}/{MaxRetries}", attempt, MaxRetries);
                    return await CallOpenAiAsync(prompt);
                }
                catch (Exception ex) when (attempt < MaxRetries)
                {
                    _logger.LogWarning(ex, "OpenAI attempt {Attempt} failed, retrying...", attempt);
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2)); // Exponential backoff
                }
            }
            throw new Exception("OpenAI API failed after maximum retries");
        }

        private async Task<string> CallOpenAiAsync(string prompt)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _openAiSettings.ApiKey);
            client.Timeout = TimeSpan.FromSeconds(30);

            var requestBody = new
            {
                model = _openAiSettings.Model ?? "gpt-4o-mini",
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 100,
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenAI API Error {response.StatusCode}: {responseBody}");

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }

        // =================== Save Suggestion ===================

        private async Task SaveAiSuggestionAsync(string type, string query, string result)
        {
            try
            {
                var suggestion = new AiSuggestion
                {
                    SuggestionType = type,
                    Payload = $"Query: {query} | Result: {result}",
                    Score = 1.0m,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AiSuggestions.Add(suggestion);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save AI suggestion to database");
            }
        }

        private bool IsAiEnabled() =>
            !string.IsNullOrEmpty(_openAiSettings.ApiKey) &&
            _openAiSettings.ApiKey != "YOUR_OPENAI_API_KEY";
    }
}