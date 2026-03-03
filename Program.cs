using E7gezhaa.API.Entities;
using E7gezhaa.API.Middleware;
using E7gezhaa.API.Services;
using E7gezhaa.API.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//////////////////////////////////////////////////////////////
// DATABASE
//////////////////////////////////////////////////////////////

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
        }));

//////////////////////////////////////////////////////////////
// IDENTITY
//////////////////////////////////////////////////////////////

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

//////////////////////////////////////////////////////////////
// JWT
//////////////////////////////////////////////////////////////

var jwtKey = builder.Configuration["AppSettings:Token"]
    ?? "E7gezhaa_Super_Secret_JWT_Key_2026_ForProduction!";

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
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

//////////////////////////////////////////////////////////////
// SETTINGS
//////////////////////////////////////////////////////////////

builder.Services.Configure<PaymobSettings>(builder.Configuration.GetSection("Paymob"));
builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGrid"));
builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAI"));

//////////////////////////////////////////////////////////////
// HTTP CLIENTS
//////////////////////////////////////////////////////////////

builder.Services.AddHttpClient<PaymobService>();
builder.Services.AddScoped<IPaymentService, PaymentService>(); 
builder.Services.AddHttpClient();

//////////////////////////////////////////////////////////////
// SERVICES
//////////////////////////////////////////////////////////////

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IVendorProviderService, VendorProviderService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAiRecommendationService, AiRecommendationService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPhotographerService, PhotographerService>();
builder.Services.AddScoped<IBeautyService, BeautyService>();

//////////////////////////////////////////////////////////////
// CONTROLLERS
//////////////////////////////////////////////////////////////

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

//////////////////////////////////////////////////////////////
// OPENAPI + SCALAR (بديل Swagger - متوافق مع .NET 10)
//////////////////////////////////////////////////////////////

builder.Services.AddOpenApi();

//////////////////////////////////////////////////////////////
// CORS
//////////////////////////////////////////////////////////////

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

//////////////////////////////////////////////////////////////
// ROLE SEEDING
//////////////////////////////////////////////////////////////

using (var scope = app.Services.CreateScope())
{
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "Admin", "Vendor", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "خطأ أثناء تأسيس الأدوار");
    }
}

//////////////////////////////////////////////////////////////
// MIDDLEWARE PIPELINE
//////////////////////////////////////////////////////////////

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // http://localhost:5000/scalar/v1
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();