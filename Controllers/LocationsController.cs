using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
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

        // جلب كل المواقع (المحافظات والمدن)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var locations = await _locationService.GetAllLocationsAsync();
            return Ok(locations);
        }

        // إضافة موقع جديد (للأدمن فقط)
        [HttpPost]
        public async Task<IActionResult> Create(Location location)
        {
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "تم إضافة الموقع بنجاح", Id = location.Id });
        }
    }
}