using E7gezhaa.API.DTOs;
using E7gezhaa.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // =================== REGISTER ===================
        [HttpPost("register")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult> Register([FromBody] RegisterDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName,
                    Role = request.Role,
                    PhoneNumber = request.Phone
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded) return BadRequest(result.Errors);

                if (!await _roleManager.RoleExistsAsync(user.Role))
                    await _roleManager.CreateAsync(new IdentityRole(user.Role));

                await _userManager.AddToRoleAsync(user, user.Role);

                if (user.Role == "Vendor")
                {
                    _context.Vendors.Add(new Vendor { Id = user.Id, Name = user.FullName });
                    await _context.SaveChangesAsync();
                }

                return Ok(new { Message = "تم إنشاء الحساب بنجاح." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "حدث خطأ", Detail = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // =================== LOGIN ===================
        [HttpPost("login")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult> Login([FromBody] LoginDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return BadRequest(new { Message = "بيانات الدخول غير صحيحة." });

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = CreateToken(user, roles);
            var refreshToken = await GenerateRefreshToken(user.Id);

            return Ok(new
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });
        }

        // =================== REFRESH TOKEN ===================
        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var principal = GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
                return BadRequest(new { Message = "الـ Token غير صالح." });

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var storedRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && r.UserId == userId);

            if (storedRefreshToken == null || !storedRefreshToken.IsActive)
                return BadRequest(new { Message = "الـ Refresh Token غير صالح أو منتهي الصلاحية." });

            storedRefreshToken.IsUsed = true;
            _context.RefreshTokens.Update(storedRefreshToken);

            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound(new { Message = "المستخدم غير موجود." });

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = CreateToken(user, roles);
            var newRefreshToken = await GenerateRefreshToken(user.Id);

            return Ok(new
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });
        }

        // =================== REVOKE TOKEN ===================
        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<ActionResult> RevokeToken([FromBody] RefreshTokenDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var storedRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && r.UserId == userId);

            if (storedRefreshToken == null)
                return NotFound(new { Message = "الـ Refresh Token غير موجود." });

            storedRefreshToken.IsRevoked = true;
            _context.RefreshTokens.Update(storedRefreshToken);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم إلغاء الـ Token بنجاح." });
        }

        // =================== FORGOT PASSWORD ===================
        [HttpPost("forgot-password")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(request.Email);

            // دايما نرجع نفس الرسالة عشان نمنع User Enumeration Attack
            if (user == null)
                return Ok(new { Message = "لو البريد الإلكتروني مسجل، هتوصلك رسالة بكود إعادة التعيين." });

            // توليد Reset Token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(resetToken);

            // إرسال Email
            var emailSent = await SendResetPasswordEmail(user.Email!, user.FullName, encodedToken);

            if (!emailSent)
                Console.WriteLine($"[Password Reset] Token for {user.Email}: {encodedToken}");

            return Ok(new { Message = "لو البريد الإلكتروني مسجل، هتوصلك رسالة بكود إعادة التعيين." });
        }

        // =================== RESET PASSWORD ===================
        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return BadRequest(new { Message = "بيانات غير صحيحة." });

            var decodedToken = Uri.UnescapeDataString(request.Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { Message = "فشل تغيير كلمة المرور. تأكد من صحة الكود.", Errors = result.Errors });

            // إلغاء كل الـ Refresh Tokens القديمة عند تغيير الباسورد
            var oldTokens = _context.RefreshTokens.Where(r => r.UserId == user.Id);
            _context.RefreshTokens.RemoveRange(oldTokens);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم تغيير كلمة المرور بنجاح. يرجى تسجيل الدخول من جديد." });
        }

        // =================== CHANGE PASSWORD ===================
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound(new { Message = "المستخدم غير موجود." });

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { Message = "كلمة المرور الحالية غير صحيحة.", Errors = result.Errors });

            // إلغاء كل الـ Refresh Tokens القديمة
            var oldTokens = _context.RefreshTokens.Where(r => r.UserId == user.Id);
            _context.RefreshTokens.RemoveRange(oldTokens);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم تغيير كلمة المرور بنجاح." });
        }

        // =================== Private Methods ===================

        private string CreateToken(User user, IList<string> roles)
        {
            var tokenKey = _configuration["AppSettings:Token"]
                ?? "E7gezhaa_Super_Secret_JWT_Key_2026_!@#$%^&*()";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email ?? "Unknown"),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GenerateRefreshToken(string userId)
        {
            var oldTokens = _context.RefreshTokens
                .Where(r => r.UserId == userId && (r.IsUsed || r.IsRevoked || r.ExpiresAt <= DateTime.UtcNow));
            _context.RefreshTokens.RemoveRange(oldTokens);

            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var refreshToken = Convert.ToBase64String(randomBytes);

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return refreshToken;
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenKey = _configuration["AppSettings:Token"]
                ?? "E7gezhaa_Super_Secret_JWT_Key_2026_!@#$%^&*()";

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false
            };

            try
            {
                var principal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase) &&
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch { return null; }
        }

        private async Task<bool> SendResetPasswordEmail(string toEmail, string name, string token)
        {
            try
            {
                var apiKey = _configuration["SendGrid:ApiKey"];
                var fromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@e7gezhaa.com";
                var fromName = _configuration["SendGrid:FromName"] ?? "E7gezhaa";

                if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_SENDGRID_API_KEY")
                    return false;

                var resetUrl = $"https://e7gezhaa.com/reset-password?token={token}&email={Uri.EscapeDataString(toEmail)}";

                var emailBody = $@"
                <div dir='rtl' style='font-family: Arial; padding: 20px;'>
                    <h2 style='color: #1F4E79;'>إعادة تعيين كلمة المرور</h2>
                    <p>مرحباً {name}،</p>
                    <p>تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك.</p>
                    <p>انقر على الرابط التالي لإعادة تعيين كلمة المرور:</p>
                    <a href='{resetUrl}' style='background:#2E75B6;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;display:inline-block;margin:10px 0;'>
                        إعادة تعيين كلمة المرور
                    </a>
                    <p style='color:red;'>هذا الرابط صالح لمدة 24 ساعة فقط.</p>
                    <p>إذا لم تطلب إعادة التعيين، تجاهل هذه الرسالة.</p>
                    <hr/>
                    <p style='color:#888;font-size:12px;'>فريق E7gezhaa — نظام إدارة حجز الأفراح</p>
                </div>";

                var payload = new
                {
                    personalizations = new[] { new { to = new[] { new { email = toEmail, name } } } },
                    from = new { email = fromEmail, name = fromName },
                    subject = "إعادة تعيين كلمة المرور — E7gezhaa",
                    content = new[] { new { type = "text/html", value = emailBody } }
                };

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var response = await client.PostAsync(
                    "https://api.sendgrid.com/v3/mail/send",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                );

                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}