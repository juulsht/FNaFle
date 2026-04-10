using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace FNaFle.Models
{
    public class EditProfileViewModel
    {
        public string CurrentUsername { get; set; }
        public string NewUsername { get; set; }
        
        public IFormFile? ProfilePicture { get; set; }
        public string? CurrentProfilePicturePath { get; set; }

        public int? FavChar1Id { get; set; }
        public int? FavChar2Id { get; set; }
        public int? FavChar3Id { get; set; }

        public List<Character> AvailableCharacters { get; set; } = new List<Character>();
    }
}
