using E7gezhaa.API.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface IWeddingAttireService
    {
        Task<IEnumerable<WeddingAttire>> GetAllAttireAsync();
        Task<IEnumerable<WeddingAttire>> GetByAttireTypeAsync(string type);
        Task<WeddingAttire?> GetByIdAsync(int id);
        Task<IEnumerable<WeddingAttire>> GetByVendorIdAsync(string vendorId);

        // 🛑 السطر اللي كان ناقص وموقف الـ Build
        Task<bool> IsAvailableAsync(int attireId, DateTime start, DateTime end);

        // ميثود الحسابات
        decimal CalculateRentalPrice(decimal dailyPrice, int days);
    }
}