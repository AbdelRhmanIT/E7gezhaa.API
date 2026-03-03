using E7gezhaa.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

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

        public AuthController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserDto request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var user = new User
                    {
                        UserName = request.Email,
                        Email = request.Email,
                        FullName = request.FullName ?? "",
                        Role = request.Role ?? "User"
                    };

                    var result = await _userManager.CreateAsync(user, request.Password);

                    if (result.Succeeded)
                    {
                        if (!await _roleManager.RoleExistsAsync(user.Role))
                            await _roleManager.CreateAsync(new IdentityRole(user.Role));

                        await _userManager.AddToRoleAsync(user, user.Role);

                        if (user.Role == "Vendor")
                        {
                            var vendor = new Vendor
                            {
                                Id = user.Id,
                                Name = user.FullName
                            };
                            _context.Vendors.Add(vendor);
                            await _context.SaveChangesAsync();
                        }

                        await transaction.CommitAsync();
                        return Ok(new { Message = "تم إنشاء الحساب بنجاح." });
                    }

                    return BadRequest(result.Errors);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var realError = ex.InnerException?.Message ?? ex.Message;
                    return StatusCode(500, new { Error = "حدث خطأ في قاعدة البيانات", Detail = realError });
                }
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return BadRequest("بيانات الدخول غير صحيحة.");

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new { Token = CreateToken(user, roles) });
        }

        private string CreateToken(User user, IList<string> roles)
        {
            // ✅ الإصلاح: نفس طريقة القراءة في Program.cs
            var tokenKey = _configuration["AppSettings:Token"]
                ?? "E7gezhaa_Super_Secret_JWT_Key_2026_ForProduction!";

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
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class UserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }
}