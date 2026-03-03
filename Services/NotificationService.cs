using E7gezhaa.API.Entities;
using E7gezhaa.API.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly SendGridSettings _sendGridSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public NotificationService(
            AppDbContext context,
            IOptions<SendGridSettings> sendGridSettings,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _sendGridSettings = sendGridSettings.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> SendNotificationAsync(string userId, string message, string? email = null)
        {
            try
            {
                // 1. حفظ الإشعار في الداتا بيز
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    Type = "General"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // 2. لو في إيميل، بعت Email حقيقي
                if (!string.IsNullOrEmpty(email))
                {
                    await SendEmailAsync(
                        email,
                        "إشعار من احجزها",
                        BuildEmailBody(message)
                    );
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService Error]: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // لو مفيش SendGrid API Key، نعمل Log فقط
                if (string.IsNullOrEmpty(_sendGridSettings.ApiKey) ||
                    _sendGridSettings.ApiKey == "YOUR_SENDGRID_API_KEY")
                {
                    Console.WriteLine($"[Email Simulation] To: {toEmail} | Subject: {subject}");
                    return true;
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _sendGridSettings.ApiKey);

                var emailData = new
                {
                    personalizations = new[]
                    {
                        new { to = new[] { new { email = toEmail } } }
                    },
                    from = new
                    {
                        email = _sendGridSettings.FromEmail,
                        name = "احجزها - E7gezhaa"
                    },
                    subject = subject,
                    content = new[]
                    {
                        new { type = "text/html", value = body }
                    }
                };

                var json = JsonSerializer.Serialize(emailData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.sendgrid.com/v3/mail/send", content);

                return response.StatusCode == HttpStatusCode.Accepted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SendGrid Error]: {ex.Message}");
                return false;
            }
        }

        private string BuildEmailBody(string message)
        {
            return $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: #fff; border-radius: 10px; padding: 30px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: #1F4E79; color: white; padding: 20px; border-radius: 8px 8px 0 0; text-align: center; }}
        .body {{ padding: 20px; color: #333; font-size: 16px; line-height: 1.8; }}
        .footer {{ text-align: center; color: #999; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🎉 احجزها - E7gezhaa</h2>
        </div>
        <div class='body'>
            <p>{message}</p>
        </div>
        <div class='footer'>
            <p>شكراً لاستخدامك منصة احجزها لخدمات الأفراح والمناسبات</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}