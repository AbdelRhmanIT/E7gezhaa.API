using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace E7gezhaa.API.Services
{
    public interface IFileService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName);
        void DeleteImage(string imageUrl);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف غير صالح أو فارغ.");

            // ✅ الإصلاح: استخدام WebRootPath بدل CallerFilePath
            // WebRootPath يشير لـ wwwroot سواء كنت في Development أو Production
            var webRootPath = _environment.WebRootPath;

            // لو wwwroot مش موجود (بعض الـ hosting configurations)، نعمله
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var uploadsFolder = Path.Combine(webRootPath, "uploads", folderName);

            // إنشاء المجلد لو مش موجود
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // التحقق من نوع الملف (صور فقط)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                throw new ArgumentException("نوع الملف غير مسموح. يُسمح فقط بـ jpg, jpeg, png, gif, webp");

            // حد أقصى للحجم 5MB
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("حجم الملف يتجاوز الحد المسموح (5MB)");

            // اسم فريد للملف
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folderName}/{fileName}";
        }

        public void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath)) return;

            // تحويل الـ URL لمسار الملف
            var filePath = Path.Combine(webRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}