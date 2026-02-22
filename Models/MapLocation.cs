namespace FNaFle.Models
{
    public class MapLocation
    {
        public int Id { get; set; }

        // The specific room photo (the "Hint" the user sees)
        public string ImageUrl { get; set; } = string.Empty;

        // The floor plan for the specific game (e.g., "/images/layouts/fnaf1_map.png")
        // All locations from the same game will likely share the same MapLayoutUrl
        public string MapLayoutUrl { get; set; } = string.Empty;

        public string GameName { get; set; } = string.Empty;   // e.g., "FNaF 1"
        public string CameraName { get; set; } = string.Empty; // e.g., "CAM 1A"

        /* Optional: If you want to use exact click coordinates later 
           instead of just buttons, you could add these:
           public int XPercent { get; set; } 
           public int YPercent { get; set; } 
        */
    }
}