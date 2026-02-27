using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VenuesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IVenueService _venueService;
        private readonly IFileService _fileService;

        public VenuesController(AppDbContext context, IVenueService venueService, IFileService fileService)
        {
            _context = context;
            _venueService = venueService;
            _fileService = fileService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<ActionResult<Venue>> PostVenue([FromForm] Venue venue, IFormFile? imageFile)
        {
            // 1. استخراج الـ User ID من الـ Token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("التوكن غير صالح أو انتهت صلاحيته.");

            // 2. ربط القاعة بالمورد برمجياً (تخطي الـ Validation Error)
            venue.VendorId = userId;

            // 3. حفظ بيانات القاعة الأساسية
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();

            // 4. رفع الصورة إذا وجدت وربطها بالقاعة
            if (imageFile != null)
            {
                // نستخدم الـ FileService اللي مسجلة في الـ Program.cs
                var imageUrl = await _fileService.UploadImageAsync(imageFile, "venues");

                var venueImage = new VenueImage
                {
                    VenueId = venue.Id,
                    ImageUrl = imageUrl // تأكد أن الاسم يطابق الـ Entity (ImageUrl)
                };

                _context.VenueImages.Add(venueImage);
                await _context.SaveChangesAsync();
            }

            // إرجاع النتيجة 201 Created
            return CreatedAtAction(nameof(GetVenue), new { id = venue.Id }, venue);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Venue>> GetVenue(int id)
        {
            var venue = await _context.Venues
                .Include(v => v.Images)
                .Include(v => v.TimeSlots)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venue == null) return NotFound("القاعة غير موجودة.");

            return venue;
        }
    }
}