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
    public class VenuesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;

        public VenuesController(AppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<ActionResult<Venue>> PostVenue([FromForm] Venue venue, IFormFile? imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                venue.VendorId = userId; // ربط إجباري
                _context.Venues.Add(venue);
                await _context.SaveChangesAsync();

                if (imageFile != null)
                {
                    var imageUrl = await _fileService.UploadImageAsync(imageFile, "venues");
                    _context.VenueImages.Add(new VenueImage { VenueId = venue.Id, ImageUrl = imageUrl });
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok(venue);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return BadRequest("فشل إنشاء القاعة.");
            }
        }
    }
}