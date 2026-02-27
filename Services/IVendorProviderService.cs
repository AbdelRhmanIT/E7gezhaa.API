using E7gezhaa.API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface IVendorProviderService
    {
        // التعديل: تغيير int لـ string ليطابق الـ Identity والـ Service
        Task<IEnumerable<VendorService>> GetServicesByVendorIdAsync(string vendorId);

        // التعديل هنا: حافظنا على الـ ? لضمان عدم وجود Warning
        Task<VendorService?> GetServiceByIdAsync(int serviceId);
    }
}