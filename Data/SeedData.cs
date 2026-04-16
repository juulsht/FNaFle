using FNaFle.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.IO;

namespace FNaFle.Data
{
    public class CharacterSeedDto
    {
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Generation { get; set; }
        public string Species { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string ImagePath { get; set; }
        public List<string> VoiceLines { get; set; }
    }

    public class MapLocationSeedDto
    {
        public string ImageUrl { get; set; }
        public string MapLayoutUrl { get; set; }
        public string GameName { get; set; }
        public string CameraName { get; set; }
    }

    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

            string charFile = "CharactersSeed.json";
            string mapFile = "MapLocationsSeed.json";

            // 1. IMPORT if DB is empty and JSON exists
            if (!context.Characters.Any() && File.Exists(charFile))
            {
                var json = File.ReadAllText(charFile);
                var dtos = JsonSerializer.Deserialize<List<CharacterSeedDto>>(json);
                if (dtos != null)
                {
                    foreach (var dto in dtos)
                    {
                        var character = new Character
                        {
                            Name = dto.Name,
                            Gender = dto.Gender,
                            Generation = dto.Generation,
                            Species = dto.Species,
                            Location = dto.Location,
                            Status = dto.Status,
                            ImagePath = dto.ImagePath,
                            VoiceLines = dto.VoiceLines?.Select(v => new VoiceLine { Text = v }).ToList() ?? new List<VoiceLine>()
                        };
                        context.Characters.Add(character);
                    }
                    context.SaveChanges();
                }
            }
            
            if (!context.MapLocations.Any() && File.Exists(mapFile))
            {
                var json = File.ReadAllText(mapFile);
                var dtos = JsonSerializer.Deserialize<List<MapLocationSeedDto>>(json);
                if (dtos != null)
                {
                    foreach (var dto in dtos)
                    {
                        context.MapLocations.Add(new MapLocation
                        {
                            ImageUrl = dto.ImageUrl,
                            MapLayoutUrl = dto.MapLayoutUrl,
                            GameName = dto.GameName,
                            CameraName = dto.CameraName
                        });
                    }
                    context.SaveChanges();
                }
            }

            // 2. EXPORT current DB to JSON so it stays updated
            var exportChars = context.Characters.Include(c => c.VoiceLines).Select(c => new CharacterSeedDto
            {
                Name = c.Name,
                Gender = c.Gender,
                Generation = c.Generation,
                Species = c.Species,
                Location = c.Location,
                Status = c.Status,
                ImagePath = c.ImagePath,
                VoiceLines = c.VoiceLines.Select(v => v.Text).ToList()
            }).ToList();
            
            if (exportChars.Any()) {
                File.WriteAllText(charFile, JsonSerializer.Serialize(exportChars, new JsonSerializerOptions { WriteIndented = true }));
            }

            var exportMaps = context.MapLocations.Select(m => new MapLocationSeedDto
            {
                ImageUrl = m.ImageUrl,
                MapLayoutUrl = m.MapLayoutUrl,
                GameName = m.GameName,
                CameraName = m.CameraName
            }).ToList();
            
            if (exportMaps.Any()) {
                File.WriteAllText(mapFile, JsonSerializer.Serialize(exportMaps, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }
}
