using FNaFle.Models;
using Microsoft.EntityFrameworkCore;

namespace FNaFle.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate(); // Ensures DB is created and latest migrations applied

            // If there are already characters, do nothing
            if (context.Characters.Any())
            {
                return;
            }

            var characters = new Character[]
            {
                new Character { Name="Freddy", Gender="Male", Generation="Classic", Species="Bear", Location="Pizza Place", Status="Active" },
                new Character { Name="Bonnie", Gender="Male", Generation="Classic", Species="Rabbit", Location="Pizza Place", Status="Active" },
                new Character { Name="Chica", Gender="Female", Generation="Classic", Species="Chicken", Location="Pizza Place", Status="Active" },
                new Character { Name="Foxy", Gender="Male", Generation="Classic", Species="Fox", Location="Pirate Cove", Status="Active" },
                new Character { Name="Toy Freddy", Gender="Male", Generation="Toy", Species="Bear", Location="Pizza Place", Status="Active" },
                new Character { Name="Toy Bonnie", Gender="Male", Generation="Toy", Species="Rabbit", Location="Pizza Place", Status="Active" },
                new Character { Name="Mangle", Gender="Other", Generation="Toy", Species="Fox", Location="Sister Location", Status="Decommissioned" },
                new Character { Name="Balloon Boy", Gender="Male", Generation="Toy", Species="Human/Animatronic", Location="Pizza Place", Status="Active" },
            };

            context.Characters.AddRange(characters);
            context.SaveChanges();
        }
    }
}
