using E7gezhaa.API.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface IPhotographerService
    {
        Task<IEnumerable<PhotographerPackage>> GetAllPackagesAsync();
        Task<PhotographerPackage?> GetPackageByIdAsync(int id);
        Task<IEnumerable<PhotographerPackage>> GetPackagesByVendorIdAsync(string vendorId);
        Task<bool> AddPackageAsync(PhotographerPackage package);
        Task<bool> IsAvailableAsync(string vendorId, DateTime date);
    }
}