using E7gezhaa.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public class WeddingAttireService : IWeddingAttireService
    {
        private readonly AppDbContext _context;

        public WeddingAttireService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WeddingAttire>> GetAllAttireAsync() =>
            await _context.WeddingAttires.Where(a => a.Available).ToListAsync();

        public async Task<WeddingAttire?> GetByIdAsync(int id) =>
            await _context.WeddingAttires.FindAsync(id);

        public async Task<IEnumerable<WeddingAttire>> GetByVendorIdAsync(string vendorId) =>
            await _context.WeddingAttires.Where(a => a.VendorId == vendorId).ToListAsync();

        public async Task<IEnumerable<WeddingAttire>> GetByAttireTypeAsync(string type) =>
            await _context.WeddingAttires.Where(a => a.Type.ToLower() == type.ToLower() && a.Available).ToListAsync();

        // منطق الحساب باليوم
        public decimal CalculateRentalPrice(decimal dailyPrice, int days) =>
            days <= 1 ? dailyPrice : dailyPrice * days;

        // التأكد من أن القطعة غير محجوزة في هذه التواريخ
        public async Task<bool> IsAvailableAsync(int attireId, DateTime start, DateTime end)
        {
            return !await _context.AttireBookings
                .AnyAsync(b => b.AttireId == attireId &&
                          b.Status != "Cancelled" &&
                          ((start >= b.StartDate && start <= b.EndDate) ||
                           (end >= b.StartDate && end <= b.EndDate)));
        }
    }
}