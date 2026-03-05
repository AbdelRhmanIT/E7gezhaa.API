using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using E7gezhaa.API.Entities;
using System.Linq;

namespace E7gezhaa.API.Entities
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<VendorService> VendorServices { get; set; }
        public DbSet<Style> Styles { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<VenueImage> VenueImages { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<AiModel> AiModels { get; set; }
        public DbSet<AiSuggestion> AiSuggestions { get; set; }
        public DbSet<AiRecommendationLog> AiRecommendationLogs { get; set; }
        public DbSet<PhotographerPackage> PhotographerPackages { get; set; }
        public DbSet<BeautyPackage> BeautyPackages { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Decimal Precision ---
            var decimalProperties = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

            foreach (var property in decimalProperties)
                property.SetColumnType("decimal(18,2)");

            // --- Soft Delete Global Filters ---
            modelBuilder.Entity<Venue>().HasQueryFilter(v => !v.IsDeleted);
            modelBuilder.Entity<Booking>().HasQueryFilter(b => !b.IsDeleted);

            // --- Booking Relations ---
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue).WithMany()
                .HasForeignKey(b => b.VenueId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User).WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // --- BookingItem -> Booking (Optional) ---
            modelBuilder.Entity<BookingItem>()
                .HasOne(bi => bi.Booking).WithMany(b => b.BookingItems)
                .HasForeignKey(bi => bi.BookingId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Payment -> Booking (Optional) ---
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking).WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // --- AiSuggestion -> Booking (Optional) ---
            modelBuilder.Entity<AiSuggestion>()
                .HasOne(a => a.Booking).WithMany()
                .HasForeignKey(a => a.BookingId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // --- Review Relations ---
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User).WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Booking).WithMany()
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.NoAction);

            // --- Venue -> Vendor ---
            modelBuilder.Entity<Venue>()
                .HasOne(v => v.Vendor).WithMany(v => v.Venues)
                .HasForeignKey(v => v.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Vendor -> Location ---
            modelBuilder.Entity<Vendor>()
                .HasOne(v => v.Location).WithMany()
                .HasForeignKey(v => v.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- TimeSlot -> Venue (Optional) ---
            modelBuilder.Entity<TimeSlot>()
                .HasOne(t => t.Space).WithMany(s => s.TimeSlots)
                .HasForeignKey(t => t.VenueId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // --- VenueImage -> Venue (Optional) ---
            modelBuilder.Entity<VenueImage>()
                .HasOne(i => i.Space).WithMany(s => s.Images)
                .HasForeignKey(i => i.VenueId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // --- RefreshToken -> User ---
            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.User).WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.Token).IsUnique();
        }
    }
}