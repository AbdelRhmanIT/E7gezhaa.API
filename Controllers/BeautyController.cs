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
    public class BeautyController : ControllerBase
    {
        private readonly IBeautyService _beautyService;

        public BeautyController(IBeautyService beautyService)
        {
            _beautyService = beautyService;
        }

        [HttpPost("add-package")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> AddPackage([FromBody] BeautyPackage package)
        {
            var vendorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _beautyService.AddPackageAsync(package, vendorId!);
            return Ok(new { Message = "تمت إضافة الباقة", Data = result });
        }

        [HttpPost("book")]
        [Authorize]
        public async Task<IActionResult> Book([FromBody] BeautyBookingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _beautyService.BookBeautySessionAsync(dto.PackageId, dto.EventDate, userId!);

            if (booking == null) return NotFound("الباقة غير موجودة");

            return Ok(new { Message = "تم الحجز بنجاح", BookingId = booking.Id, Total = booking.TotalPrice });
        }
    }

    public class BeautyBookingDto
    {
        public int PackageId { get; set; }
        public DateTime EventDate { get; set; }
    }
}