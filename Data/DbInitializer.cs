using FNaFle.Models;
using System.Linq;

namespace FNaFle.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            
            if (context.MapLocations.Any())
            {
                return;   
            }

            var maps = new MapLocation[]
            {
                
                new MapLocation {
                    ImageUrl = "/images/maps/Cam2B.png",
                    MapLayoutUrl = "/images/maps/fnaf1.png", 
                    GameName = "FNaF 1",
                    CameraName = "CAM 2B"
                },
                
                
                new MapLocation {
                    ImageUrl = "/images/maps/Cam07_fnaf2.png",
                    MapLayoutUrl = "/images/layouts/fnaf2_map.png", 
                    GameName = "FNaF 2",
                    CameraName = "CAM 07"
                }
            };

            context.MapLocations.AddRange(maps);
            context.SaveChanges();
        }
    }
}