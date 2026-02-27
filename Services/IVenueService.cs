using E7gezhaa.API.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface IVenueService
    {
        // جلب أفضل القاعات (تمهيداً للـ AI)
        Task<IEnumerable<Venue>> GetRecommendedVenuesAsync(int count);

        // التحقق من توافر القاعة في وقت معين
        Task<bool> IsVenueAvailableAsync(int venueId, DateTime requestedTime);
    }
}