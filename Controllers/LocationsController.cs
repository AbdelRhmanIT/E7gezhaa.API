using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly AppDbContext _context;

        public LocationsController(ILocationService locationService, AppDbContext context)
        {
            _locationService = locationService;
            _context = context;
        }

        /// <summary>
        /// جلب كل المواقع - متاح للجميع
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var locations = await _locationService.GetAllLocationsAsync();
            return Ok(locations);
        }

        /// <summary>
        /// جلب موقع بالـ ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
                return NotFound(new { Message = "الموقع غير موجود" });

            return Ok(location);
        }

        /// <summary>
        /// إضافة موقع جديد - ✅ للأدمن فقط
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")] // ← الإصلاح: كان بدون حماية
        public async Task<IActionResult> Create([FromBody] Location location)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = location.Id },
                new { Message = "تم إضافة الموقع بنجاح", Id = location.Id, Data = location });
        }

        /// <summary>
        /// تعديل موقع - للأدمن فقط
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Location location)
        {
            var existing = await _locationService.GetLocationByIdAsync(id);
            if (existing == null)
                return NotFound(new { Message = "الموقع غير موجود" });

            existing.Governorate = location.Governorate;
            existing.City = location.City;
            existing.AddressLines = location.AddressLines;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "تم التعديل بنجاح", Data = existing });
        }

        /// <summary>
        /// حذف موقع - للأدمن فقط
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
                return NotFound(new { Message = "الموقع غير موجود" });

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "تم حذف الموقع بنجاح" });
        }
    }
}