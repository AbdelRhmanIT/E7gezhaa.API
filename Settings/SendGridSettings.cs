namespace E7gezhaa.API.Settings
{
    public class SendGridSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = "noreply@e7gezhaa.com";
        public string FromName { get; set; } = "احجزها - E7gezhaa";
    }
}