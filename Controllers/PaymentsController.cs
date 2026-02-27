using E7gezhaa.API.Entities;
using E7gezhaa.API.Services;
using Microsoft.AspNetCore.Authorization; // مهم جداً
using Microsoft.AspNetCore.Mvc;          // مهم جداً

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentsController(IPaymentService paymentService) => _paymentService = paymentService;

        [HttpPost]
        public async Task<IActionResult> Pay(Payment payment)
        {
            var success = await _paymentService.ProcessPaymentAsync(payment);
            return success ? Ok("تم تسجيل عملية الدفع بنجاح") : BadRequest("فشل تسجيل الدفع");
        }
    }
}