using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VendorServicesController : ControllerBase
    {
        private readonly IVendorProviderService _vendorProviderService;
        private readonly AppDbContext _context;

        public VendorServicesController(IVendorProviderService vendorProviderService, AppDbContext context)
        {
            _vendorProviderService = vendorProviderService;
            _context = context;
        }

        [HttpGet("by-vendor/{vendorId}")]
        public async Task<IActionResult> GetByVendor(string vendorId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // حماية الملكية: لا أحد يرى خدمات غيره إلا الأدمن
            if (currentUserId != vendorId && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(await _vendorProviderService.GetServicesByVendorIdAsync(vendorId));
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> CreateService(VendorService service)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // الحل هنا: اتأكد إن الـ userId موجود قبل ما تعمل أي حاجة
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("يجب تسجيل الدخول لإضافة خدمة.");

            // دلوقتي الـ Compiler اتطمن إن الـ userId مش null
            service.VendorId = userId!;

            _context.VendorServices.Add(service);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تمت الإضافة بنجاح", ServiceId = service.Id });
        }
    }
}