using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotographerController : ControllerBase
    {
        private readonly IPhotographerService _photoService;
        private readonly AppDbContext _context;

        public PhotographerController(IPhotographerService photoService, AppDbContext context)
        {
            _photoService = photoService;
            _context = context;
        }

        [HttpGet("packages")]
        public async Task<IActionResult> GetPackages() => Ok(await _photoService.GetAllPackagesAsync());

        [HttpPost("add-package")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> AddPackage([FromBody] PhotographerPackageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var package = new PhotographerPackage
            {
                VendorId = userId!,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DurationInHours = dto.DurationInHours
            };

            await _photoService.AddPackageAsync(package);
            return Ok(new { Message = "تمت إضافة الباقة", Data = package });
        }

        [HttpPost("book")]
        [Authorize]
        public async Task<IActionResult> BookPhotographer([FromBody] PhotoBookingRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var package = await _photoService.GetPackageByIdAsync(request.PackageId);

            if (package == null) return NotFound("الباقة غير موجودة");

            if (!await _photoService.IsAvailableAsync(package.VendorId, request.EventDate))
                return BadRequest("المصور محجوز في هذا التاريخ");

            var booking = new Booking
            {
                UserId = userId!,
                PhotographerPackageId = package.Id,
                BookingDate = request.EventDate,
                TotalPrice = package.Price,
                Status = "Pending"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم الحجز بنجاح", Total = package.Price });
        }
    }

    public class PhotographerPackageDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public int DurationInHours { get; set; }
    }

    public class PhotoBookingRequest
    {
        public int PackageId { get; set; }
        public DateTime EventDate { get; set; }
    }
}