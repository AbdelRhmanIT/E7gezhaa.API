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
    public class BeautyController : ControllerBase
    {
        private readonly IBeautyService _beautyService;

        public BeautyController(IBeautyService beautyService)
        {
            _beautyService = beautyService;
        }

        [HttpPost("add-package")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> AddPackage([FromBody] BeautyPackageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vendorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var package = new BeautyPackage
            {
                VendorId = vendorId!,
                Name = dto.Name,
                Description = dto.Description ?? "",
                Price = dto.Price,
                Available = true
            };

            var result = await _beautyService.AddPackageAsync(package, vendorId!);
            return Ok(new { Message = "تمت إضافة الباقة", Data = result });
        }

        [HttpPost("book")]
        [Authorize]
        public async Task<IActionResult> Book([FromBody] BeautyBookingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _beautyService.BookBeautySessionAsync(dto.PackageId, dto.EventDate, userId!);

            if (booking == null)
                return NotFound(new { Message = "الباقة غير موجودة" });

            return Ok(new { Message = "تم الحجز بنجاح", BookingId = booking.Id, Total = booking.TotalPrice });
        }
    }
}