using E7gezhaa.API.DTOs;
using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var query = _context.Venues
                .Include(v => v.Images)
                .Include(v => v.DetailedLocation)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var venues = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return Ok(new PagedResult<object>
            {
                Data = venues,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            });
        }

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

        [HttpPost]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> PostVenue([FromBody] VenueRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
                    errorMessage = ex.InnerException?.Message ?? ex.Message;
                }
            });

            if (errorMessage != null)
                return BadRequest(new { Message = "فشل إنشاء القاعة", Detail = errorMessage });

            return Ok(new { Message = "تمت إضافة القاعة بنجاح", Id = savedVenue!.Id, Data = savedVenue });
        }

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

        [HttpPost("{id}/timeslots")]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> AddTimeSlot(int id, [FromBody] TimeSlotRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound(new { Message = "القاعة غير موجودة" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (venue.VendorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            venue.IsDeleted = true;
            venue.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم حذف القاعة بنجاح." });
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreVenue(int id)
        {
            var venue = await _context.Venues
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.Id == id && v.IsDeleted);

            if (venue == null)
                return NotFound(new { Message = "القاعة غير موجودة أو غير محذوفة." });

            venue.IsDeleted = false;
            venue.DeletedAt = null;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم استعادة القاعة بنجاح." });
        }

        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDeletedVenues()
        {
            var deletedVenues = await _context.Venues
                .IgnoreQueryFilters()
                .Where(v => v.IsDeleted)
                .Include(v => v.DetailedLocation)
                .ToListAsync();

            return Ok(deletedVenues);
        }
    }
}