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
                // FNaF 1 Example
                new MapLocation {
                    ImageUrl = "/images/maps/Cam2B.png",
                    MapLayoutUrl = "/images/maps/fnaf1.png", // The floor plan for FNaF 1
                    GameName = "FNaF 1",
                    CameraName = "CAM 2B"
                },
                
                // FNaF 2 Example (Added so you can test switching maps)
                new MapLocation {
                    ImageUrl = "/images/maps/Cam07_fnaf2.png",
                    MapLayoutUrl = "/images/layouts/fnaf2_map.png", // The floor plan for FNaF 2
                    GameName = "FNaF 2",
                    CameraName = "CAM 07"
                }
            };

            context.MapLocations.AddRange(maps);
            context.SaveChanges();
        }
    }
}