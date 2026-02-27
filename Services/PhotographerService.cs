using E7gezhaa.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class PhotographerService : IPhotographerService
    {
        private readonly AppDbContext _context;

        public PhotographerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PhotographerPackage>> GetAllPackagesAsync() =>
            await _context.PhotographerPackages.Where(p => p.Available).ToListAsync();

        public async Task<PhotographerPackage?> GetPackageByIdAsync(int id) =>
            await _context.PhotographerPackages.FindAsync(id);

        public async Task<IEnumerable<PhotographerPackage>> GetPackagesByVendorIdAsync(string vendorId) =>
            await _context.PhotographerPackages.Where(p => p.VendorId == vendorId).ToListAsync();

        public async Task<bool> AddPackageAsync(PhotographerPackage package)
        {
            _context.PhotographerPackages.Add(package);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsAvailableAsync(string vendorId, DateTime date)
        {
            // التأكد إن المصور مش محجوز في اليوم ده في جدول الـ Booking الأساسي
            return !await _context.Bookings
                .AnyAsync(b => b.PhotographerPackage!.VendorId == vendorId &&
                          b.BookingDate.Date == date.Date &&
                          b.Status != "Cancelled");
        }
    }
}