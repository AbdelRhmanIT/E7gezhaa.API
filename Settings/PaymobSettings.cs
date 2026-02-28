namespace E7gezhaa.API.Settings
{
    public class PaymobSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string IframeId { get; set; } = string.Empty; // ده اللي بيفتح شاشة الدفع
    }
}