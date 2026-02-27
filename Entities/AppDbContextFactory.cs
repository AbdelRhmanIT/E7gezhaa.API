using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using E7gezhaa.API.Entities;

namespace E7gezhaa.API.Entities
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // السطر ده بيربط بالداتا بيز اللي هنكريتها
            optionsBuilder.UseSqlServer("Server=.;Database=E7gezhaDB;Trusted_Connection=True;TrustServerCertificate=True;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}