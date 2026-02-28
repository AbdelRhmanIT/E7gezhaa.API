using E7gezhaa.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class VendorProviderService : IVendorProviderService
    {
        private readonly AppDbContext _context;
        public VendorProviderService(AppDbContext context) => _context = context;

        // تنفيذ الوظيفة الأولى المطلوبة في الـ Interface
        public async Task<IEnumerable<VendorService>> GetServicesByVendorIdAsync(string vendorId)
        {
            return await _context.VendorServices
                .Where(s => s.VendorId == vendorId)
                .ToListAsync();
        }

        // تنفيذ الوظيفة الثانية (اللي كان ناقصة)
        public async Task<VendorService?> GetServiceByIdAsync(int serviceId)
        {
            return await _context.VendorServices
                .FirstOrDefaultAsync(s => s.Id == serviceId);
        }
    }
}