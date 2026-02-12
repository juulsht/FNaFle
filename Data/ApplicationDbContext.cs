using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FNaFle.Models;

namespace FNaFle.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Character> Characters { get; set; }
        public DbSet<DailyGame> DailyGames { get; set; }
        public DbSet<UserProgress> UserProgress { get; set; }
        public DbSet<MapLocation> MapLocations { get; set; }

    }
}
