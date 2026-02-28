using E7gezhaa.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace E7gezhaa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WebhookController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("paymob")]
        public async Task<IActionResult> PaymobWebhook([FromBody] dynamic payload)
        {
            // استخرج القيمة برا الـ Query الأول
            string orderIdString = payload.obj.order.id.ToString();
            var success = payload.obj.success;

            if (success == "true")
            {
                // دلوقتي الـ orderIdString ده عبارة عن string عادي الـ EF هيفهمه
                var payment = await _context.Payments
                    .Include(p => p.Booking)
                    .FirstOrDefaultAsync(p => p.TransactionId == orderIdString);

                if (payment != null && payment.Booking != null)
                {
                    payment.Status = "Paid";
                    payment.Booking.Status = "Confirmed";
                    await _context.SaveChangesAsync();
                }
            }
            return Ok();
        }
    }
}