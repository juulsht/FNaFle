using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FNaFle.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? ProfilePicturePath { get; set; }

        // Foreign keys for characters
        public int? FavChar1Id { get; set; }
        public int? FavChar2Id { get; set; }
        public int? FavChar3Id { get; set; }

        // Navigation properties
        [ForeignKey("FavChar1Id")]
        public virtual Character? FavChar1 { get; set; }

        [ForeignKey("FavChar2Id")]
        public virtual Character? FavChar2 { get; set; }

        [ForeignKey("FavChar3Id")]
        public virtual Character? FavChar3 { get; set; }
    }
}
