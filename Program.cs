using E7gezhaa.API.Entities;
using E7gezhaa.API.Middleware;
using E7gezhaa.API.Services;
using E7gezhaa.API.Settings; // <-- تأكد من هذا الـ Namespace
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. الربط بالداتا بيز
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// --- إعدادات الـ Paymob ---
builder.Services.Configure<PaymobSettings>(builder.Configuration.GetSection("Paymob"));

// --- تسجيل الخدمات ---
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IVendorProviderService, VendorProviderService>();
builder.Services.AddScoped<IWeddingAttireService, WeddingAttireService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAiRecommendationService, AiRecommendationService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPhotographerService, PhotographerService>();
builder.Services.AddScoped<IBeautyService, BeautyService>();

// تسجيل خدمة الدفع كـ HttpClient لاستخدامها مع Paymob
builder.Services.AddHttpClient<IPaymentService, PaymobService>();

// --- إضافة نظام الـ Identity ---
builder.Services.AddIdentity<User, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();



// 2. حل مشكلة الـ Circular Reference
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 3. إعدادات الـ JWT
var jwtKey = builder.Configuration.GetSection("AppSettings:Token").Value;
if (string.IsNullOrEmpty(jwtKey)) throw new Exception("JWT Token Key is missing in appsettings.json!");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- تأسيس الأدوار (Role Seeding) ---
using (var scope = app.Services.CreateScope())
{
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "Admin", "Vendor", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "حدث خطأ أثناء تأسيس الأدوار (Roles Seeding)");
    }
}

// تفعيل درع الحماية
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();