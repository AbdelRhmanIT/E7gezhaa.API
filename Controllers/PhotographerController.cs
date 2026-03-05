using E7gezhaa.API.DTOs;
using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public async Task<IActionResult> GetPackages([FromQuery] PaginationParams pagination)
        {
            var allPackages = await _photoService.GetAllPackagesAsync();
            var list = allPackages.ToList();
            var totalCount = list.Count;
            var paged = list
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            return Ok(new PagedResult<object>
            {
                Data = paged,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            });
        }

        [HttpPost("add-package")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> AddPackage([FromBody] PhotographerPackageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var package = new PhotographerPackage
            {
                VendorId = userId!,
                Name = dto.Name,
                Description = dto.Description ?? "",
                Price = dto.Price,
                DurationInHours = dto.DurationInHours
            };

            await _photoService.AddPackageAsync(package);
            return Ok(new { Message = "تمت إضافة الباقة", Data = package });
        }

        [HttpPost("book")]
        [Authorize]
        public async Task<IActionResult> BookPhotographer([FromBody] PhotoBookingRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var package = await _photoService.GetPackageByIdAsync(request.PackageId);
            if (package == null) return NotFound(new { Message = "الباقة غير موجودة" });

            if (!await _photoService.IsAvailableAsync(package.VendorId, request.EventDate))
                return BadRequest(new { Message = "المصور محجوز في هذا التاريخ" });

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
}