using E7gezhaa.API.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface IBeautyService
    {
        Task<BeautyPackage> AddPackageAsync(BeautyPackage package, string vendorId);
        Task<IEnumerable<BeautyPackage>> GetAllPackagesAsync();
        Task<Booking?> BookBeautySessionAsync(int packageId, DateTime date, string userId);
    }
}