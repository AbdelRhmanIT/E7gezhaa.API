using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorServicesController : ControllerBase
    {
        private readonly IVendorProviderService _vendorProviderService;
        private readonly AppDbContext _context;

        public VendorServicesController(IVendorProviderService vendorProviderService, AppDbContext context)
        {
            _vendorProviderService = vendorProviderService;
            _context = context;
        }

        // جلب كل الخدمات لمورد معين (التعديل: vendorId أصبح string)
        [HttpGet("by-vendor/{vendorId}")]
        public async Task<IActionResult> GetByVendor(string vendorId)
        {
            var services = await _vendorProviderService.GetServicesByVendorIdAsync(vendorId);
            return Ok(services);
        }

        // إضافة خدمة جديدة (للموردين والآدمين فقط)
        [HttpPost]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> CreateService(VendorService service)
        {
            _context.VendorServices.Add(service);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "تم إضافة الخدمة بنجاح", ServiceId = service.Id });
        }
    }
}