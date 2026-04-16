namespace FNaFle.Models
{
    public class MapLocation
    {
        public int Id { get; set; }

        
        public string ImageUrl { get; set; } = string.Empty;


        public string MapLayoutUrl { get; set; } = string.Empty;

        public string GameName { get; set; } = string.Empty;   
        public string CameraName { get; set; } = string.Empty; 

        
    }
}