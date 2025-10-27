using System.ComponentModel.DataAnnotations;

namespace FNaFle.Models
{
    public class Character
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Gender { get; set; }       // Male / Female / Unknown
        public string Generation { get; set; }   // Classic / Toy / Glamrock / etc.
        public string Species { get; set; }      // Bear / Rabbit / etc.
        public string Location { get; set; }     // Pizza Place / Sister Location / etc.
        public string Status { get; set; }       // Active / Phantom / Decommissioned
    }
}
