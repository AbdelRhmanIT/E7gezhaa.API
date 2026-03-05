using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace E7gezhaa.API.DTOs
{
    // =================== AUTH DTOs ===================

    public class RegisterDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [MaxLength(100, ErrorMessage = "البريد الإلكتروني لا يتجاوز 100 حرف")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(8, ErrorMessage = "كلمة المرور لا تقل عن 8 أحرف")]
        [MaxLength(50, ErrorMessage = "كلمة المرور لا تتجاوز 50 حرف")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "كلمة المرور يجب أن تحتوي على حرف كبير وحرف صغير ورقم على الأقل")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقتين")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [MinLength(3, ErrorMessage = "الاسم لا يقل عن 3 أحرف")]
        [MaxLength(100, ErrorMessage = "الاسم لا يتجاوز 100 حرف")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [MaxLength(15, ErrorMessage = "رقم الهاتف لا يتجاوز 15 رقم")]
        public string? Phone { get; set; }

        [RegularExpression("^(Admin|Vendor|User)$",
            ErrorMessage = "الدور يجب أن يكون Admin أو Vendor أو User")]
        public string Role { get; set; } = "User";
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "الكود مطلوب")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [MinLength(8, ErrorMessage = "كلمة المرور لا تقل عن 8 أحرف")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "كلمة المرور يجب أن تحتوي على حرف كبير وحرف صغير ورقم على الأقل")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقتين")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "الـ Token مطلوب")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "الـ Refresh Token مطلوب")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [MinLength(8, ErrorMessage = "كلمة المرور لا تقل عن 8 أحرف")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "كلمة المرور يجب أن تحتوي على حرف كبير وحرف صغير ورقم على الأقل")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقتين")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    // =================== VENUE DTOs ===================

    public class VenueRequestDto
    {
        [Required(ErrorMessage = "اسم القاعة مطلوب")]
        [MinLength(3, ErrorMessage = "اسم القاعة لا يقل عن 3 أحرف")]
        [MaxLength(150, ErrorMessage = "اسم القاعة لا يتجاوز 150 حرف")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع القاعة مطلوب")]
        [RegularExpression("^(Wedding|Birthday|Conference|Exhibition|Other)$",
            ErrorMessage = "نوع القاعة يجب أن يكون: Wedding أو Birthday أو Conference أو Exhibition أو Other")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "السعر بالساعة مطلوب")]
        [Range(1, 1000000, ErrorMessage = "السعر يجب أن يكون بين 1 و 1,000,000")]
        public decimal PricePerHour { get; set; }

        [Required(ErrorMessage = "السعة مطلوبة")]
        [Range(1, 100000, ErrorMessage = "السعة يجب أن تكون بين 1 و 100,000 شخص")]
        public int Capacity { get; set; }

        [MaxLength(1000, ErrorMessage = "الوصف لا يتجاوز 1000 حرف")]
        public string? Description { get; set; }

        [MaxLength(300, ErrorMessage = "الموقع لا يتجاوز 300 حرف")]
        public string? Location { get; set; }

        [MaxLength(100, ErrorMessage = "الفئة لا تتجاوز 100 حرف")]
        public string? Category { get; set; }

        [Range(0, 100, ErrorMessage = "نسبة العربون يجب أن تكون بين 0 و 100")]
        public decimal DepositPercentage { get; set; } = 25;

        public int? LocationId { get; set; }

        [MaxLength(500, ErrorMessage = "المميزات لا تتجاوز 500 حرف")]
        public string? Features { get; set; }

        [Url(ErrorMessage = "رابط الموقع غير صحيح")]
        [MaxLength(200, ErrorMessage = "رابط الموقع لا يتجاوز 200 حرف")]
        public string? WebsiteUrl { get; set; }

        [Range(0, 1000000, ErrorMessage = "سعر الويكند يجب أن يكون بين 0 و 1,000,000")]
        public decimal? WeekendPrice { get; set; }

        [Range(-90, 90, ErrorMessage = "خط العرض يجب أن يكون بين -90 و 90")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "خط الطول يجب أن يكون بين -180 و 180")]
        public double? Longitude { get; set; }
    }

    public class TimeSlotRequestDto
    {
        [Required(ErrorMessage = "وقت البداية مطلوب")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "وقت النهاية مطلوب")]
        public DateTime EndTime { get; set; }

        [Range(-100000, 100000, ErrorMessage = "تعديل السعر يجب أن يكون بين -100,000 و 100,000")]
        public decimal PriceAdjustment { get; set; } = 0;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
                yield return new ValidationResult("وقت النهاية يجب أن يكون بعد وقت البداية", new[] { nameof(EndTime) });

            if (StartTime <= DateTime.UtcNow)
                yield return new ValidationResult("وقت البداية يجب أن يكون في المستقبل", new[] { nameof(StartTime) });

            var duration = (EndTime - StartTime).TotalHours;
            if (duration < 1)
                yield return new ValidationResult("مدة الموعد يجب أن تكون ساعة على الأقل", new[] { nameof(EndTime) });

            if (duration > 24)
                yield return new ValidationResult("مدة الموعد لا تتجاوز 24 ساعة", new[] { nameof(EndTime) });
        }
    }

    // =================== BOOKING DTOs ===================

    public class BookingRequestDto
    {
        [Required(ErrorMessage = "رقم القاعة مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "رقم القاعة غير صحيح")]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "رقم الموعد مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "رقم الموعد غير صحيح")]
        public int SlotId { get; set; }

        [Required(ErrorMessage = "نوع المناسبة مطلوب")]
        [RegularExpression("^(Wedding|Birthday|Conference|Exhibition|Other)$",
            ErrorMessage = "نوع المناسبة يجب أن يكون: Wedding أو Birthday أو Conference أو Exhibition أو Other")]
        public string EventType { get; set; } = "Wedding";
    }

    public class BookingDashboardDto
    {
        public int BookingId { get; set; }
        public string? VenueName { get; set; }
        public DateTime StartTime { get; set; }
        public string? Status { get; set; }
        public decimal TotalPrice { get; set; }
        public bool CanRate { get; set; }
        public string? PhotographerName { get; set; }
        public string? BeautyPackageName { get; set; }
        public List<string> ExtraItems { get; set; } = new();
    }

    // =================== PAYMENT DTOs ===================

    public class PaymentRequestDto
    {
        [Required(ErrorMessage = "رقم الحجز مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "رقم الحجز غير صحيح")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Range(1, 10000000, ErrorMessage = "المبلغ يجب أن يكون بين 1 و 10,000,000")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "العملة مطلوبة")]
        [RegularExpression("^(EGP|USD|EUR)$", ErrorMessage = "العملة يجب أن تكون EGP أو USD أو EUR")]
        public string Currency { get; set; } = "EGP";

        [Required(ErrorMessage = "مزود الدفع مطلوب")]
        [RegularExpression("^(Paymob|Cash|BankTransfer)$",
            ErrorMessage = "مزود الدفع يجب أن يكون Paymob أو Cash أو BankTransfer")]
        public string Provider { get; set; } = "Paymob";

        [MaxLength(100, ErrorMessage = "رقم العملية لا يتجاوز 100 حرف")]
        public string? TransactionId { get; set; }
    }

    // =================== PHOTOGRAPHER DTOs ===================

    public class PhotographerPackageDto
    {
        [Required(ErrorMessage = "اسم الباقة مطلوب")]
        [MinLength(3, ErrorMessage = "اسم الباقة لا يقل عن 3 أحرف")]
        [MaxLength(150, ErrorMessage = "اسم الباقة لا يتجاوز 150 حرف")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "الوصف لا يتجاوز 500 حرف")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(1, 1000000, ErrorMessage = "السعر يجب أن يكون بين 1 و 1,000,000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "مدة التصوير مطلوبة")]
        [Range(1, 24, ErrorMessage = "مدة التصوير يجب أن تكون بين 1 و 24 ساعة")]
        public int DurationInHours { get; set; }
    }

    public class PhotoBookingRequestDto
    {
        [Required(ErrorMessage = "رقم الباقة مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "رقم الباقة غير صحيح")]
        public int PackageId { get; set; }

        [Required(ErrorMessage = "تاريخ المناسبة مطلوب")]
        public DateTime EventDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EventDate <= DateTime.UtcNow)
                yield return new ValidationResult("تاريخ المناسبة يجب أن يكون في المستقبل", new[] { nameof(EventDate) });

            if (EventDate > DateTime.UtcNow.AddYears(2))
                yield return new ValidationResult("تاريخ المناسبة لا يتجاوز سنتين من الآن", new[] { nameof(EventDate) });
        }
    }

    // =================== BEAUTY DTOs ===================

    public class BeautyPackageDto
    {
        [Required(ErrorMessage = "اسم الباقة مطلوب")]
        [MinLength(3, ErrorMessage = "اسم الباقة لا يقل عن 3 أحرف")]
        [MaxLength(150, ErrorMessage = "اسم الباقة لا يتجاوز 150 حرف")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "الوصف لا يتجاوز 500 حرف")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(1, 1000000, ErrorMessage = "السعر يجب أن يكون بين 1 و 1,000,000")]
        public decimal Price { get; set; }
    }

    public class BeautyBookingDto
    {
        [Required(ErrorMessage = "رقم الباقة مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "رقم الباقة غير صحيح")]
        public int PackageId { get; set; }

        [Required(ErrorMessage = "تاريخ المناسبة مطلوب")]
        public DateTime EventDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EventDate <= DateTime.UtcNow)
                yield return new ValidationResult("تاريخ المناسبة يجب أن يكون في المستقبل", new[] { nameof(EventDate) });
        }
    }

    // =================== REVIEW DTOs ===================

    public class AddReviewDto
    {
        [Required(ErrorMessage = "رقم الحجز مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "رقم الحجز غير صحيح")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "التقييم مطلوب")]
        [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
        public decimal Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "التعليق لا يتجاوز 1000 حرف")]
        public string? Comment { get; set; }

        public string? VendorId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "رقم القاعة غير صحيح")]
        public int? VenueId { get; set; }
    }

    // =================== LOCATION DTOs ===================

    public class LocationDto
    {
        [Required(ErrorMessage = "اسم المحافظة مطلوب")]
        [MaxLength(100, ErrorMessage = "اسم المحافظة لا يتجاوز 100 حرف")]
        public string Governorate { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المدينة مطلوب")]
        [MaxLength(100, ErrorMessage = "اسم المدينة لا يتجاوز 100 حرف")]
        public string City { get; set; } = string.Empty;

        [MaxLength(300, ErrorMessage = "العنوان التفصيلي لا يتجاوز 300 حرف")]
        public string? AddressLines { get; set; }
    }

    // =================== PAGINATION ===================

    public class PaginationParams
    {
        private int _pageSize = 10;
        private const int MaxPageSize = 50;

        [Range(1, int.MaxValue, ErrorMessage = "رقم الصفحة يجب أن يكون أكبر من 0")]
        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
        }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}