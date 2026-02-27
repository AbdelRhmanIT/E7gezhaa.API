using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeddingAttireController : ControllerBase
    {
        private readonly IWeddingAttireService _attireService;
        private readonly IFileService _fileService;
        private readonly AppDbContext _context;

        public WeddingAttireController(IWeddingAttireService attireService, IFileService fileService, AppDbContext context)
        {
            _attireService = attireService;
            _fileService = fileService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeddingAttire>>> GetAttires() => Ok(await _attireService.GetAllAttireAsync());

        [HttpPost]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<ActionResult<WeddingAttire>> PostAttire([FromForm] WeddingAttireRequestDto request, IFormFile? imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("يجب تسجيل الدخول");

            string? imageUrl = imageFile != null ? await _fileService.UploadImageAsync(imageFile, "attire") : null;

            var attire = new WeddingAttire
            {
                VendorId = userId,
                Name = request.Name,
                Type = request.Type,
                Size = request.Size,
                Color = request.Color,
                Price = request.Price,
                RentalOrSale = request.RentalOrSale,
                ImageUrl = imageUrl,
                Available = true
            };

            _context.WeddingAttires.Add(attire);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "تمت الإضافة بنجاح", Data = attire });
        }

        // --- ميثود الحجز (النخاع الشوكي للأتيليه) ---
        [HttpPost("rent")]
        [Authorize]
        public async Task<IActionResult> RentAttire([FromBody] RentRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var attire = await _attireService.GetByIdAsync(request.AttireId);

            if (attire == null) return NotFound("القطعة غير موجودة");

            // التأكد من التواريخ والتوافر
            if (!await _attireService.IsAvailableAsync(request.AttireId, request.StartDate, request.EndDate))
                return BadRequest("هذه القطعة محجوزة بالفعل في التواريخ المختارة");

            int days = (request.EndDate - request.StartDate).Days;
            if (days <= 0) days = 1; // حد أدنى يوم واحد

            decimal totalPrice = _attireService.CalculateRentalPrice(attire.Price, days);

            var booking = new AttireBooking
            {
                UserId = userId!,
                AttireId = request.AttireId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalPrice = totalPrice,
                Status = "Pending"
            };

            _context.AttireBookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم طلب الحجز بنجاح", Total = totalPrice, BookingId = booking.Id });
        }
    }

    // DTOs
    public class WeddingAttireRequestDto
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Size { get; set; } = "";
        public string Color { get; set; } = "";
        public decimal Price { get; set; }
        public string RentalOrSale { get; set; } = "Rental";
    }

    public class RentRequestDto
    {
        public int AttireId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}