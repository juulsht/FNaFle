using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FNaFle.Models
{
    public class Character
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Gender { get; set; }       
        public string Generation { get; set; }   
        public string Species { get; set; }      
        public string Location { get; set; }     
        public string Status { get; set; }       

        [MaxLength(300)]
        public string? ImagePath { get; set; }   

        [JsonIgnore]
        public ICollection<VoiceLine> VoiceLines { get; set; } = new List<VoiceLine>();
    }
}
