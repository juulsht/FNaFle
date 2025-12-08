using System;

namespace FNaFle.Models
{
    public class UserProgress
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime LastGuessDate { get; set; }
        public bool HasGuessedCorrectlyToday { get; set; }
        public int Streak { get; set; }
    }
}
