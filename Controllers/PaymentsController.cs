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
    [Authorize] // ممنوع دخول أي حد مش مسجل
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> Pay(Payment payment)
        {
            // استخراج الـ UserId من الـ Token بتاع اليوزر
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("يجب تسجيل الدخول لإتمام عملية الدفع.");

            // تنفيذ العملية عبر الـ Service
            var success = await _paymentService.ProcessPaymentAsync(payment, userId);

            return success
                ? Ok(new { message = "تم تسجيل عملية الدفع بنجاح" })
                : BadRequest(new { message = "فشل تسجيل الدفع، تأكد من بيانات الحجز أو حالة الدفع." });
        }
    }
}