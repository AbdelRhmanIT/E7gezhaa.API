public class Location
{
    public int Id { get; set; }
    public string Governorate { get; set; } = string.Empty; // المحافظة
    public string City { get; set; } = string.Empty;        // المدينة
    public string AddressLines { get; set; } = string.Empty; // العنوان بالتفصيل
}