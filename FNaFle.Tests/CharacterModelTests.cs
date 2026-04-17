using FNaFle.Models;
using Xunit;

namespace FNaFle.Tests
{
    public class CharacterModelTests
    {
        [Fact]
        public void Character_Properties_CanBeSetAndGet()
        {
            var character = new Character();
            var name = "Freddy Fazbear";
            var species = "Bear";

            character.Name = name;
            character.Species = species;

            Assert.Equal(name, character.Name);
            Assert.Equal(species, character.Species);
        }
    }
}
