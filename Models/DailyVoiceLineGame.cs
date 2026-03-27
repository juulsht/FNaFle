using System;

namespace FNaFle.Models
{
    public class DailyVoiceLineGame
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int VoiceLineId { get; set; }
        public VoiceLine VoiceLine { get; set; }
    }
}
