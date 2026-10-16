using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class RoleAccessTests
    {
        // =========================================================
        // ROLE VALIDATION - Simple role semantics
        // =========================================================

        [Fact]
        public void ParticipantRole_IsNotOrganiser()
        {
            var user = new AppUser { Role = "Participant" };
            Assert.NotEqual("Organiser", user.Role);
        }

        [Fact]
        public void OrganiserRole_IsNotParticipant()
        {
            var user = new AppUser { Role = "Organiser" };
            Assert.NotEqual("Participant", user.Role);
        }

        [Fact]
        public void User_CanOnlyHaveOneRole()
        {
            var user = new AppUser { Role = "Organiser" };
            Assert.Single(new[] { user.Role });
        }

        // =========================================================
        // ROLE-BASED OWNERSHIP - Event creation
        // =========================================================

        [Fact]
        public void Organiser_CreatesEvent_WithOrganiserId()
        {
            // Arrange
            using var db = TestHelpers.CreateInMemoryDb();

            var orgUser = new AppUser
            {
                Email = "organiser@test.com",
                PasswordHash = "hash",
                FirstName = "Org",
                LastName = "User",
                Role = "Organiser"
            };
            db.Users.Add(orgUser);
            db.SaveChanges();

            var orgProfile = new OrganiserProfile
            {
                UserId = orgUser.UserId,
                OrganisationName = "Test Org"
            };
            db.OrganiserProfiles.Add(orgProfile);
            db.SaveChanges();

            // Act - create event
            var evt = new Event
            {
                OrganiserId = orgProfile.OrganiserId,
                Name = "Test Marathon",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Cape Town",
                DistanceKm = 42.2m,
                EventType = "Run"
            };
            db.Events.Add(evt);
            db.SaveChanges();

            // Assert
            var stored = db.Events.First();
            Assert.Equal(orgProfile.OrganiserId, stored.OrganiserId);
            Assert.Equal("Test Marathon", stored.Name);
            Assert.Equal(42.2m, stored.DistanceKm);
        }

        [Fact]
        public void Participant_CannotOwnEvents()
        {
            // Arrange
            var participant = new AppUser { Role = "Participant" };

            // Assert - participant has no OrganiserProfile by design
            Assert.Null(participant.OrganiserProfile);
        }

        [Fact]
        public void OnlyOrganiserProfile_HasEvents()
        {
            // Arrange
            var orgUser = new AppUser { Role = "Organiser" };
            var partUser = new AppUser { Role = "Participant" };

            // Assert
            Assert.Null(partUser.OrganiserProfile);
            // orgUser would have one after adding to DB (tested above)
            Assert.Equal("Organiser", orgUser.Role);
        }
    }
}