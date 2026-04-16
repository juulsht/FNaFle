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

            
            var existingDbChars = context.Characters.Include(x => x.VoiceLines).ToList();
            var jsonChars = new List<CharacterSeedDto>();
            
            if (File.Exists(charFile))
            {
                var json = File.ReadAllText(charFile);
                jsonChars = JsonSerializer.Deserialize<List<CharacterSeedDto>>(json) ?? new List<CharacterSeedDto>();
            }

            bool charsUpdated = false;
            foreach (var jChar in jsonChars)
            {
                var existingChar = existingDbChars.FirstOrDefault(c => c.Name.Equals(jChar.Name, StringComparison.OrdinalIgnoreCase));
                if (existingChar == null)
                {
                    var character = new Character
                    {
                        Name = jChar.Name,
                        Gender = jChar.Gender,
                        Generation = jChar.Generation,
                        Species = jChar.Species,
                        Location = jChar.Location,
                        Status = jChar.Status,
                        ImagePath = jChar.ImagePath,
                        VoiceLines = jChar.VoiceLines?.Select(v => new VoiceLine { Text = v }).ToList() ?? new List<VoiceLine>()
                    };
                    context.Characters.Add(character);
                    charsUpdated = true;
                }
                else
                {
                    
                    if (string.IsNullOrEmpty(existingChar.ImagePath) && !string.IsNullOrEmpty(jChar.ImagePath))
                    {
                        existingChar.ImagePath = jChar.ImagePath;
                        charsUpdated = true;
                    }
                    if (string.IsNullOrEmpty(existingChar.Location) && !string.IsNullOrEmpty(jChar.Location))
                    {
                        existingChar.Location = jChar.Location;
                        charsUpdated = true;
                    }
                }
            }

            if (charsUpdated)
            {
                context.SaveChanges();
                existingDbChars = context.Characters.Include(x => x.VoiceLines).ToList();
            }
            
            else if (existingDbChars.Count == 0)
            {
                var characters = new Character[]
                {
                    new Character { Name="Freddy", Gender="Male", Generation="Classic", Species="Bear", Location="Pizza Place", Status="Active", ImagePath="/images/profiles/Freddy.png" },
                    new Character { Name="Bonnie", Gender="Male", Generation="Classic", Species="Rabbit", Location="Pizza Place", Status="Active", ImagePath="/images/profiles/Bonnie.png" },
                    new Character { Name="Chica", Gender="Female", Generation="Classic", Species="Chicken", Location="Pizza Place", Status="Active", ImagePath="/images/profiles/Chica.png" },
                    new Character { Name="Foxy", Gender="Male", Generation="Classic", Species="Fox", Location="Pirate Cove", Status="Active", ImagePath="/images/profiles/Foxy.png" },
                };
                context.Characters.AddRange(characters);
                context.SaveChanges();
                existingDbChars = context.Characters.Include(x => x.VoiceLines).ToList();
            }

            if (existingDbChars.Count > jsonChars.Count)
            {
                var exportChars = existingDbChars.Select(c => new CharacterSeedDto
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
                File.WriteAllText(charFile, JsonSerializer.Serialize(exportChars, new JsonSerializerOptions { WriteIndented = true }));
            }


            var existingMaps = context.MapLocations.ToList();
            var jsonMaps = new List<MapLocationSeedDto>();

            if (File.Exists(mapFile))
            {
                var json = File.ReadAllText(mapFile);
                jsonMaps = JsonSerializer.Deserialize<List<MapLocationSeedDto>>(json) ?? new List<MapLocationSeedDto>();
            }

            bool mapsAdded = false;
            foreach (var jMap in jsonMaps)
            {
                if (!existingMaps.Any(m => m.CameraName == jMap.CameraName && m.GameName == jMap.GameName))
                {
                    context.MapLocations.Add(new MapLocation
                    {
                        ImageUrl = jMap.ImageUrl,
                        MapLayoutUrl = jMap.MapLayoutUrl,
                        GameName = jMap.GameName,
                        CameraName = jMap.CameraName
                    });
                    mapsAdded = true;
                }
            }

            if (mapsAdded)
            {
                context.SaveChanges();
                existingMaps = context.MapLocations.ToList();
            }
            else if (existingMaps.Count == 0)
            {
                var maps = new MapLocation[]
                {
                    new MapLocation {
                        ImageUrl = "/images/maps/Cam2B.png",
                        MapLayoutUrl = "/images/maps/fnaf1.png",
                        GameName = "FNaF 1",
                        CameraName = "CAM 2B"
                    }
                };
                context.MapLocations.AddRange(maps);
                context.SaveChanges();
                existingMaps = context.MapLocations.ToList();
            }

            if (existingMaps.Count > jsonMaps.Count)
            {
                var exportMaps = existingMaps.Select(m => new MapLocationSeedDto
                {
                    ImageUrl = m.ImageUrl,
                    MapLayoutUrl = m.MapLayoutUrl,
                    GameName = m.GameName,
                    CameraName = m.CameraName
                }).ToList();
                File.WriteAllText(mapFile, JsonSerializer.Serialize(exportMaps, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }
}
