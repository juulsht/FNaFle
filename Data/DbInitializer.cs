using FNaFle.Models;
using System.Linq;

namespace FNaFle.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Look for any map locations.
            if (context.MapLocations.Any())
            {
                return;   // DB has been seeded
            }

            var maps = new MapLocation[]
            {
                new MapLocation {
                    ImageUrl = "/images/maps/Cam2B.png",
                    GameName = "FNaF 1",
                    CameraName = "CAM 02"
                }
                
            };

            context.MapLocations.AddRange(maps);
            context.SaveChanges();
        }
    }
}