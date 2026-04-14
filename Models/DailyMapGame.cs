using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FNaFle.Models
{
    public class DailyMapGame
    {
        [Key]
        public int Id { get; set; }

        public int MapLocationId { get; set; }

        [ForeignKey("MapLocationId")]
        public MapLocation MapLocation { get; set; }

        public DateTime Date { get; set; }
    }
}
