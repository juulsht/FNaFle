using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FNaFle.Models;
using Xunit;

namespace FNaFle.Tests
{
    public class ModelTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }

                [Fact]
        public void Character_Properties_WorkCorrectly()
        {
            var character = new Character
            {
                Id = 1,
                Name = "Freddy",
                Gender = "Male",
                Generation = "Gen 1",
                Species = "Bear",
                Location = "Pizzeria",
                Status = "Active",
                ImagePath = "/images/freddy.png"
            };

            Assert.Equal(1, character.Id);
            Assert.Equal("Freddy", character.Name);
            Assert.Equal("Male", character.Gender);
            Assert.Equal("Gen 1", character.Generation);
            Assert.Equal("Bear", character.Species);
            Assert.Equal("Pizzeria", character.Location);
            Assert.Equal("Active", character.Status);
            Assert.Equal("/images/freddy.png", character.ImagePath);
            Assert.NotNull(character.VoiceLines);
        }

        [Fact]
        public void Character_RequiredName_Validation()
        {
            var character = new Character { Name = null };
            var results = ValidateModel(character);
            Assert.Contains(results, r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Character_ImagePath_MaxLength_Validation()
        {
            var character = new Character { Name = "Test", ImagePath = new string('a', 301) };
            var results = ValidateModel(character);
            Assert.Contains(results, r => r.MemberNames.Contains("ImagePath"));
        }
        
                [Fact]
        public void UserProgress_Properties_WorkCorrectly()
        {
            var date = System.DateTime.UtcNow;
            var progress = new UserProgress
            {
                Id = 1,
                UserId = "user-123",
                LastGuessDate = date,
                HasGuessedCorrectlyToday = true,
                Streak = 5,
                HighestStreak = 10
            };

            Assert.Equal(1, progress.Id);
            Assert.Equal("user-123", progress.UserId);
            Assert.Equal(date, progress.LastGuessDate);
            Assert.True(progress.HasGuessedCorrectlyToday);
            Assert.Equal(5, progress.Streak);
            Assert.Equal(10, progress.HighestStreak);
        }
        
                [Fact]
        public void UserProfile_Properties_WorkCorrectly()
        {
            var profile = new UserProfile
            {
                Id = 1,
                UserId = "user-123",
                ProfilePicturePath = "/pics/me.jpg",
                FavChar1Id = 1,
                FavChar2Id = 2,
                FavChar3Id = 3
            };

            Assert.Equal(1, profile.Id);
            Assert.Equal("user-123", profile.UserId);
            Assert.Equal("/pics/me.jpg", profile.ProfilePicturePath);
            Assert.Equal(1, profile.FavChar1Id);
            Assert.Equal(2, profile.FavChar2Id);
            Assert.Equal(3, profile.FavChar3Id);
        }

        [Fact]
        public void UserProfile_RequiredUserId_Validation()
        {
            var profile = new UserProfile { UserId = null };
            var results = ValidateModel(profile);
            Assert.Contains(results, r => r.MemberNames.Contains("UserId"));
        }
        
                [Fact]
        public void VoiceLine_Properties_WorkCorrectly()
        {
            var voiceLine = new VoiceLine
            {
                Id = 1,
                Text = "Hello",
                CharacterId = 5
            };

            Assert.Equal(1, voiceLine.Id);
            Assert.Equal("Hello", voiceLine.Text);
            Assert.Equal(5, voiceLine.CharacterId);
        }
        
                [Fact]
        public void MapLocation_Properties_WorkCorrectly()
        {
            var location = new MapLocation
            {
                Id = 1,
                GameName = "FNaF 1",
                CameraName = "Office",
                ImageUrl = "/maps/office.png",
                MapLayoutUrl = "/maps/layout.png"
            };

            Assert.Equal(1, location.Id);
            Assert.Equal("FNaF 1", location.GameName);
            Assert.Equal("Office", location.CameraName);
            Assert.Equal("/maps/office.png", location.ImageUrl);
            Assert.Equal("/maps/layout.png", location.MapLayoutUrl);
        }
        
                [Fact]
        public void DailyGame_Properties_WorkCorrectly()
        {
            var date = System.DateTime.Today;
            var game = new DailyGame { Id = 1, CharacterId = 10, Date = date };
            Assert.Equal(1, game.Id);
            Assert.Equal(10, game.CharacterId);
            Assert.Equal(date, game.Date);
        }

        [Fact]
        public void DailyMapGame_Properties_WorkCorrectly()
        {
            var date = System.DateTime.Today;
            var game = new DailyMapGame { Id = 1, MapLocationId = 5, Date = date };
            Assert.Equal(1, game.Id);
            Assert.Equal(5, game.MapLocationId);
            Assert.Equal(date, game.Date);
        }

        [Fact]
        public void DailyVoiceLineGame_Properties_WorkCorrectly()
        {
            var date = System.DateTime.Today;
            var game = new DailyVoiceLineGame { Id = 1, VoiceLineId = 2, Date = date };
            Assert.Equal(1, game.Id);
            Assert.Equal(2, game.VoiceLineId);
            Assert.Equal(date, game.Date);
        }
        
                [Fact]
        public void RankedScore_Properties_WorkCorrectly()
        {
            var date = System.DateTime.UtcNow;
            var score = new RankedScore
            {
                Id = 1,
                Username = "user-1",
                TotalPoints = 150,
                CurrentStreak = 5,
                LastPlayedDate = date
            };
            Assert.Equal(1, score.Id);
            Assert.Equal("user-1", score.Username);
            Assert.Equal(150, score.TotalPoints);
            Assert.Equal(5, score.CurrentStreak);
            Assert.Equal(date, score.LastPlayedDate);
        }
        
                [Fact]
        public void EditProfileViewModel_Properties_WorkCorrectly()
        {
            var vm = new EditProfileViewModel
            {
                CurrentUsername = "old",
                NewUsername = "new",
                CurrentProfilePicturePath = "path",
                FavChar1Id = 1,
                FavChar2Id = 2,
                FavChar3Id = 3
            };
            Assert.Equal("old", vm.CurrentUsername);
            Assert.Equal("new", vm.NewUsername);
            Assert.Equal("path", vm.CurrentProfilePicturePath);
            Assert.Equal(1, vm.FavChar1Id);
            Assert.Equal(2, vm.FavChar2Id);
            Assert.Equal(3, vm.FavChar3Id);
            Assert.NotNull(vm.AvailableCharacters);
        }

        [Fact]
        public void ErrorViewModel_Properties_WorkCorrectly()
        {
            var vm = new ErrorViewModel { RequestId = "req-1" };
            Assert.Equal("req-1", vm.RequestId);
            Assert.True(vm.ShowRequestId);
            
            vm.RequestId = null;
            Assert.False(vm.ShowRequestId);
        }

        [Fact]
        public void LeaderboardUserViewModel_Properties_WorkCorrectly()
        {
            var vm = new LeaderboardUserViewModel
            {
                Username = "Player1",
                Streak = 5,
                ProfilePicturePath = "/pic.png"
            };
            Assert.Equal("Player1", vm.Username);
            Assert.Equal(5, vm.Streak);
            Assert.Equal("/pic.png", vm.ProfilePicturePath);
        }
        
                [Fact] public void Character_Id_SetGet() { var m = new Character { Id = 10 }; Assert.Equal(10, m.Id); }
        [Fact] public void Character_Name_SetGet() { var m = new Character { Name = "X" }; Assert.Equal("X", m.Name); }
        [Fact] public void Character_Gender_SetGet() { var m = new Character { Gender = "X" }; Assert.Equal("X", m.Gender); }
        [Fact] public void Character_Generation_SetGet() { var m = new Character { Generation = "X" }; Assert.Equal("X", m.Generation); }
        [Fact] public void Character_Species_SetGet() { var m = new Character { Species = "X" }; Assert.Equal("X", m.Species); }
        [Fact] public void Character_Location_SetGet() { var m = new Character { Location = "X" }; Assert.Equal("X", m.Location); }
        [Fact] public void Character_Status_SetGet() { var m = new Character { Status = "X" }; Assert.Equal("X", m.Status); }
        [Fact] public void Character_ImagePath_SetGet() { var m = new Character { ImagePath = "X" }; Assert.Equal("X", m.ImagePath); }

        [Fact] public void UserProgress_Id_SetGet() { var m = new UserProgress { Id = 10 }; Assert.Equal(10, m.Id); }
        [Fact] public void UserProgress_UserId_SetGet() { var m = new UserProgress { UserId = "X" }; Assert.Equal("X", m.UserId); }
        [Fact] public void UserProgress_Streak_SetGet() { var m = new UserProgress { Streak = 10 }; Assert.Equal(10, m.Streak); }
        [Fact] public void UserProgress_HighestStreak_SetGet() { var m = new UserProgress { HighestStreak = 20 }; Assert.Equal(20, m.HighestStreak); }

        [Fact] public void UserProfile_Id_SetGet() { var m = new UserProfile { Id = 10 }; Assert.Equal(10, m.Id); }
        [Fact] public void UserProfile_UserId_SetGet() { var m = new UserProfile { UserId = "X" }; Assert.Equal("X", m.UserId); }
        [Fact] public void UserProfile_PicPath_SetGet() { var m = new UserProfile { ProfilePicturePath = "X" }; Assert.Equal("X", m.ProfilePicturePath); }

        [Fact] public void VoiceLine_Id_SetGet() { var m = new VoiceLine { Id = 10 }; Assert.Equal(10, m.Id); }
        [Fact] public void VoiceLine_Text_SetGet() { var m = new VoiceLine { Text = "X" }; Assert.Equal("X", m.Text); }
        [Fact] public void VoiceLine_CharId_SetGet() { var m = new VoiceLine { CharacterId = 10 }; Assert.Equal(10, m.CharacterId); }

        [Fact] public void MapLocation_Id_SetGet() { var m = new MapLocation { Id = 10 }; Assert.Equal(10, m.Id); }
        [Fact] public void MapLocation_ImageUrl_SetGet() { var m = new MapLocation { ImageUrl = "X" }; Assert.Equal("X", m.ImageUrl); }
        [Fact] public void MapLocation_MapLayoutUrl_Works() { var m = new MapLocation { MapLayoutUrl = "test" }; Assert.Equal("test", m.MapLayoutUrl); }
        [Fact] public void MapLocation_GameName_SetGet() { var m = new MapLocation { GameName = "X" }; Assert.Equal("X", m.GameName); }
        [Fact] public void MapLocation_CameraName_SetGet() { var m = new MapLocation { CameraName = "X" }; Assert.Equal("X", m.CameraName); }

        [Fact] public void DailyGame_Id_SetGet() { var m = new DailyGame { Id = 10 }; Assert.Equal(10, m.Id); }
        [Fact] public void DailyGame_CharacterId_Works() { var m = new DailyGame { CharacterId = 5 }; Assert.Equal(5, m.CharacterId); }

        [Fact] public void RankedScore_Id_SetGet() { var m = new RankedScore { Id = 10 }; Assert.Equal(10, m.Id); }
        [Fact] public void RankedScore_Username_SetGet() { var m = new RankedScore { Username = "X" }; Assert.Equal("X", m.Username); }
        [Fact] public void RankedScore_TotalPoints_SetGet() { var m = new RankedScore { TotalPoints = 10 }; Assert.Equal(10, m.TotalPoints); }
        [Fact] public void RankedScore_CurrentStreak_SetGet() { var m = new RankedScore { CurrentStreak = 10 }; Assert.Equal(10, m.CurrentStreak); }

        [Fact] public void EditProfileViewModel_Usernames_SetGet() 
        { 
            var m = new EditProfileViewModel { CurrentUsername = "A", NewUsername = "B" }; 
            Assert.Equal("A", m.CurrentUsername); Assert.Equal("B", m.NewUsername); 
        }

        [Fact] public void DailyMapGame_Id_SetGet() { var m = new DailyMapGame { Id = 1 }; Assert.Equal(1, m.Id); }
        [Fact] public void DailyMapGame_LocationId_SetGet() { var m = new DailyMapGame { MapLocationId = 5 }; Assert.Equal(5, m.MapLocationId); }
        
        [Fact] public void DailyVoiceLineGame_Id_SetGet() { var m = new DailyVoiceLineGame { Id = 1 }; Assert.Equal(1, m.Id); }
        [Fact] public void DailyVoiceLineGame_VLId_SetGet() { var m = new DailyVoiceLineGame { VoiceLineId = 2 }; Assert.Equal(2, m.VoiceLineId); }
        
        [Fact] public void Character_VoiceLines_IsNotNull() { var m = new Character(); Assert.NotNull(m.VoiceLines); }
        [Fact] public void MapLocation_FullConstructor_Mock() { var m = new MapLocation { Id = 1, CameraName = "C", GameName = "G", ImageUrl = "I", MapLayoutUrl = "L" }; Assert.Equal(1, m.Id); }
        [Fact] public void UserProfile_Favs_SetGet() { var m = new UserProfile { FavChar1Id = 1, FavChar2Id = 2, FavChar3Id = 3 }; Assert.Equal(1, m.FavChar1Id); Assert.Equal(2, m.FavChar2Id); Assert.Equal(3, m.FavChar3Id); }
        [Fact] public void RankedScore_Date_SetGet() { var d = DateTime.UtcNow; var m = new RankedScore { LastPlayedDate = d }; Assert.Equal(d, m.LastPlayedDate); }
        [Fact] public void RankedScore_Streak_SetGet() { var m = new RankedScore { CurrentStreak = 50 }; Assert.Equal(50, m.CurrentStreak); }
        [Fact] public void UserProgress_Bool_SetGet() { var m = new UserProgress { HasGuessedCorrectlyToday = true }; Assert.True(m.HasGuessedCorrectlyToday); }
        [Fact] public void MapLocation_GameName_Check() { var m = new MapLocation { GameName = "FNaF" }; Assert.Equal("FNaF", m.GameName); }
        [Fact] public void Character_Status_Check() { var m = new Character { Status = "Broken" }; Assert.Equal("Broken", m.Status); }
        [Fact] public void VoiceLine_Text_Check() { var m = new VoiceLine { Text = "Line" }; Assert.Equal("Line", m.Text); }
        [Fact] public void UserProfile_Pic_Check() { var m = new UserProfile { ProfilePicturePath = "P" }; Assert.Equal("P", m.ProfilePicturePath); }
        [Fact] public void DailyGame_Date_Check() { var d = DateTime.Today; var m = new DailyGame { Date = d }; Assert.Equal(d, m.Date); }
        [Fact] public void DailyMapGame_Date_Check() { var d = DateTime.Today; var m = new DailyMapGame { Date = d }; Assert.Equal(d, m.Date); }
        [Fact] public void DailyVoiceLineGame_Date_Check() { var d = DateTime.Today; var m = new DailyVoiceLineGame { Date = d }; Assert.Equal(d, m.Date); }
        [Fact] public void EditProfile_Fav1_Check() { var m = new EditProfileViewModel { FavChar1Id = 1 }; Assert.Equal(1, m.FavChar1Id); }
            }
}
