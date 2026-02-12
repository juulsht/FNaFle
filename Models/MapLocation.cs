namespace FNaFle.Models
{
    public class MapLocation
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty; // Path to the zoomed image
        public string GameName { get; set; } = string.Empty; // e.g., "FNaF 1"
        public string CameraName { get; set; } = string.Empty; // e.g., "CAM 1A"
    }
}
