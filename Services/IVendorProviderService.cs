using E7gezhaa.API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface IVendorProviderService
    {
        // مطابق تماماً للـ Implementation اللي عملناه
        Task<IEnumerable<VendorService>> GetServicesByVendorIdAsync(string vendorId);

        // مطابق للـ Implementation اللي عملناه
        Task<VendorService?> GetServiceByIdAsync(int serviceId);
    }
}