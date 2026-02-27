using E7gezhaa.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq; // أضفنا دي عشان الـ Where تشتغل صح
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class VendorProviderService : IVendorProviderService
    {
        private readonly AppDbContext _context;

        public VendorProviderService(AppDbContext context)
        {
            _context = context;
        }

        // التعديل هنا: غيرنا النوع لـ string عشان يطابق الـ Identity User Id
        public async Task<IEnumerable<VendorService>> GetServicesByVendorIdAsync(string vendorId)
        {
            return await _context.VendorServices
                .Where(s => s.VendorId == vendorId) // دلوقتي المقارنة string مع string
                .ToListAsync();
        }

        public async Task<VendorService?> GetServiceByIdAsync(int serviceId)
        {
            // هنا الـ serviceId بيفضل int زي ما هو لأنه Primary Key بتاع الجدول نفسه
            return await _context.VendorServices
                .FirstOrDefaultAsync(s => s.Id == serviceId);
        }
    }
}