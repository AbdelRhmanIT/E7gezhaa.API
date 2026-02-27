using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Runtime.CompilerServices;

namespace E7gezhaa.API.Services
{
    public interface IFileService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName);
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
            if (file == null || file.Length == 0) return null!;

            // 1. تحديد مسار السورس كود الفعلي (على سطح المكتب)
            // السطر ده بيجيب مسار ملف FileService.cs نفسه
            string currentFilePath = GetCurrentFilePath();

            // بنطلع خطوتين لورا: من Services إلى Project Root (E7gezhaa.API)
            string projectRoot = Directory.GetParent(currentFilePath!)!.Parent!.FullName;

            // 2. تحديد مسار wwwroot الأصلي داخل السورس
            string uploadsFolder = Path.Combine(projectRoot, "wwwroot", "uploads", folderName);

            // التأكد من وجود المجلد في السورس
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 3. توليد اسم فريد للصورة
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            // 4. النسخ الفعلي للملف من المتصفح للهارد ديسك
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 5. إرجاع اللينك الذي سيحفظ في الداتابيز
            return $"/uploads/{folderName}/{fileName}";
        }

        // ميثود مساعدة لجلب مسار الملف الحالي أثناء الكود
        private string GetCurrentFilePath([CallerFilePath] string? fileName = null) => fileName!;
    }
}