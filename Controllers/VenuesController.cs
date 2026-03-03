using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VenuesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;

        public VenuesController(AppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        /// <summary>
        /// جلب كل القاعات
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var venues = await _context.Venues
                .Include(v => v.Images)
                .Include(v => v.DetailedLocation)
                .ToListAsync();
            return Ok(venues);
        }

        /// <summary>
        /// جلب قاعة بالـ ID مع مواعيدها
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var venue = await _context.Venues
                .Include(v => v.Images)
                .Include(v => v.TimeSlots)
                .Include(v => v.Reviews)
                .Include(v => v.DetailedLocation)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venue == null)
                return NotFound(new { Message = "القاعة غير موجودة" });

            return Ok(venue);
        }

        /// <summary>
        /// إضافة قاعة - يقبل JSON أو Form-Data
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> PostVenue([FromBody] VenueRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var strategy = _context.Database.CreateExecutionStrategy();
            Venue? savedVenue = null;
            string? errorMessage = null;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var venue = new Venue
                    {
                        VendorId = User.IsInRole("Vendor") ? userId : null,
                        Name = request.Name,
                        Type = request.Type,
                        PricePerHour = request.PricePerHour,
                        Capacity = request.Capacity,
                        Description = request.Description ?? "",
                        Location = request.Location ?? "",
                        Category = request.Category ?? "",
                        DepositPercentage = request.DepositPercentage > 0 ? request.DepositPercentage : 25,
                        LocationId = request.LocationId,
                        Features = request.Features,
                        WebsiteUrl = request.WebsiteUrl,
                        WeekendPrice = request.WeekendPrice,
                        Latitude = request.Latitude,
                        Longitude = request.Longitude
                    };

                    _context.Venues.Add(venue);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    savedVenue = venue;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    errorMessage = ex.Message;
                }
            });

            if (errorMessage != null)
                return BadRequest(new { Message = "فشل إنشاء القاعة", Detail = errorMessage });

            return Ok(new { Message = "تمت إضافة القاعة بنجاح", Id = savedVenue!.Id, Data = savedVenue });
        }

        /// <summary>
        /// رفع صورة للقاعة
        /// </summary>
        [HttpPost("{id}/upload-image")]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> UploadImage(int id, IFormFile imageFile)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound(new { Message = "القاعة غير موجودة" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (venue.VendorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var imageUrl = await _fileService.UploadImageAsync(imageFile, "venues");
            _context.VenueImages.Add(new VenueImage { VenueId = id, ImageUrl = imageUrl });
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم رفع الصورة بنجاح", ImageUrl = imageUrl });
        }

        /// <summary>
        /// إضافة TimeSlot لقاعة
        /// </summary>
        [HttpPost("{id}/timeslots")]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> AddTimeSlot(int id, [FromBody] TimeSlotRequestDto request)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound(new { Message = "القاعة غير موجودة" });

            var slot = new TimeSlot
            {
                VenueId = id,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PriceAdjustment = request.PriceAdjustment,
                IsBooked = false
            };

            _context.TimeSlots.Add(slot);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تمت إضافة الموعد بنجاح", Id = slot.Id, Data = slot });
        }
    }

    public class VenueRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal PricePerHour { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Category { get; set; }
        public decimal DepositPercentage { get; set; } = 25;
        public int? LocationId { get; set; }
        public string? Features { get; set; }
        public string? WebsiteUrl { get; set; }
        public decimal? WeekendPrice { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class TimeSlotRequestDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal PriceAdjustment { get; set; } = 0;
    }
}