using E7gezhaa.API.Entities;
using E7gezhaa.API.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // <--- أضف السطر ده

namespace E7gezhaa.API.Services
{
    public class PaymobService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly PaymobSettings _settings;
        private readonly AppDbContext _context;

        public PaymobService(HttpClient httpClient, IOptions<PaymobSettings> settings, AppDbContext context)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _context = context;
        }

        public async Task<bool> ProcessPaymentAsync(Payment payment, string userId)
        {
            try
            {
                // 1. Auth
                var authToken = await GetAuthTokenAsync();

                // 2. Order
                var orderId = await CreateOrderAsync(authToken, payment.Amount);

                // 3. Payment Key (هذا هو الكود اللي هيستخدمه الفرونت إيند)
                var paymentKey = await GetPaymentKeyAsync(authToken, orderId, payment.Amount);

                // حفظ الـ Payment Key في الـ TransactionId
                payment.TransactionId = paymentKey;
                payment.Provider = "Paymob";

                // هنا بنعتمد على الـ logic اللي كتبناه في الـ PaymentService الأصلي
                // لو حابب تدمجهم، ممكن نعدل الـ Logic ده ليحفظ في الـ DB
                return true;
            }
            catch (Exception ex) // <-- هتلاقي السطر ده موجود عندك
            {
                // <-- عدل الجزء ده ليصبح كالتالي عشان الـ Warning يختفي:
                Console.WriteLine($"Error in payment: {ex.Message}");
                return false;
            }
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens", new { api_key = _settings.ApiKey });
            var data = await response.Content.ReadFromJsonAsync<dynamic>();
            return data?.GetProperty("token").GetString() ?? "";
        }

        private async Task<string> CreateOrderAsync(string authToken, decimal amount)
        {
            var orderData = new
            {
                auth_token = authToken,
                delivery_needed = "false",
                amount_cents = (int)(amount * 100), // Paymob بيحتاج المبلغ بالسنت
                currency = "EGP"
            };
            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/ecommerce/orders", orderData);
            var data = await response.Content.ReadFromJsonAsync<dynamic>();
            return data?.GetProperty("id").GetInt32().ToString() ?? "";
        }

        private async Task<string> GetPaymentKeyAsync(string authToken, string orderId, decimal amount)
        {
            var keyData = new
            {
                auth_token = authToken,
                amount_cents = (int)(amount * 100),
                expiration = 3600,
                order_id = orderId,
                billing_data = new { first_name = "User", last_name = "User", email = "test@test.com", phone_number = "01000000000" },
                currency = "EGP",
                integration_id = _settings.IframeId
            };
            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/acceptance/payment_keys", keyData);
            var data = await response.Content.ReadFromJsonAsync<dynamic>();
            return data?.GetProperty("token").GetString() ?? "";
        }

        public async Task<Payment?> GetPaymentByBookingIdAsync(int bookingId)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }
    }
}