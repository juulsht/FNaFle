using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FNaFle.Models
{
    public class VoiceLine
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public int CharacterId { get; set; }

        [JsonIgnore]
        public Character Character { get; set; }
    }
}
