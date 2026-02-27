using E7gezhaa.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class VenueService : IVenueService
    {
        private readonly AppDbContext _context;

        public VenueService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venue>> GetRecommendedVenuesAsync(int count)
        {
            // حالياً: بنجيب أعلى القاعات تقييماً (Logic بسيط)
            // مستقبلاً: هنا هيركب موديل الـ AI
            return await _context.Venues
                .Include(v => v.Images)
                .OrderByDescending(v => v.PricePerHour) // كنموذج أولي
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> IsVenueAvailableAsync(int venueId, DateTime requestedTime)
        {
            return await _context.TimeSlots
                .AnyAsync(s => s.VenueId == venueId && s.StartTime == requestedTime && !s.IsBooked);
        }
    }
}