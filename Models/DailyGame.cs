using System;

namespace FNaFle.Models
{
    public class DailyGame
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public DateTime Date { get; set; } // stores the day this character belongs to
    }
}

