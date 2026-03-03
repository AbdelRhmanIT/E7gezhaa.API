using E7gezhaa.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class BeautyService : IBeautyService
    {
        private readonly AppDbContext _context;

        public BeautyService(AppDbContext context) => _context = context;

        public async Task<BeautyPackage> AddPackageAsync(BeautyPackage package, string vendorId)
        {
            package.VendorId = vendorId;
            _context.BeautyPackages.Add(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<IEnumerable<BeautyPackage>> GetAllPackagesAsync()
        {
            return await _context.BeautyPackages.Include(p => p.Vendor).ToListAsync();
        }

        public async Task<Booking?> BookBeautySessionAsync(int packageId, DateTime date, string userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            Booking? result = null;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var package = await _context.BeautyPackages.FindAsync(packageId);
                    if (package == null) return;

                    var booking = new Booking
                    {
                        UserId = userId,
                        BeautyPackageId = packageId,
                        BookingDate = date,
                        TotalPrice = package.Price,
                        Status = "Pending",
                        VenueId = null
                    };

                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    result = booking;
                }
                catch
                {
                    await transaction.RollbackAsync();
                }
            });

            return result;
        }
    }
}