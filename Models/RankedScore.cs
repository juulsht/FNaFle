using System;
using System.ComponentModel.DataAnnotations;

namespace FNaFle.Models
{
    public class RankedScore
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        public int TotalPoints { get; set; } 

        public int CurrentStreak { get; set; }

        public DateTime LastPlayedDate { get; set; } 
    }
}