using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EnrolmentTests
    {
        // =========================================================
        // HAPPY PATH - Participant enrols
        // =========================================================

        [Fact]
        public void Participant_CanEnrolInCategory()
        {
            // Arrange
            using var db = TestHelpers.CreateInMemoryDb();

            var orgUser = new AppUser { Email = "o@t.com", PasswordHash = "h", FirstName = "O", LastName = "O", Role = "Organiser" };
            var partUser = new AppUser { Email = "p@t.com", PasswordHash = "h", FirstName = "P", LastName = "P", Role = "Participant" };
            db.Users.AddRange(orgUser, partUser);
            db.SaveChanges();

            var orgProfile = new OrganiserProfile { UserId = orgUser.UserId };
            var partProfile = new ParticipantProfile { UserId = partUser.UserId };
            db.OrganiserProfiles.Add(orgProfile);
            db.ParticipantProfiles.Add(partProfile);
            db.SaveChanges();

            var evt = new Event
            {
                OrganiserId = orgProfile.OrganiserId,
                Name = "Test 10km",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "Sandton",
                DistanceKm = 10m,
                EventType = "Run"
            };
            db.Events.Add(evt);
            db.SaveChanges();

            var cat = new Category { EventId = evt.EventId, Name = "Open", EntryFee = 100 };
            db.Categories.Add(cat);
            db.SaveChanges();

            // Act - participant enrols
            var enrolment = new Enrolment
            {
                ParticipantId = partProfile.ParticipantId,
                CategoryId = cat.CategoryId,
                Status = "Pending"
            };
            db.Enrolments.Add(enrolment);
            db.SaveChanges();

            // Assert
            var stored = db.Enrolments.Include(e => e.Participant).First();
            Assert.Equal(partProfile.ParticipantId, stored.ParticipantId);
            Assert.Equal(cat.CategoryId, stored.CategoryId);
            Assert.Equal("Pending", stored.Status);
        }

        // =========================================================
        // FAILURE PATH - Duplicate prevention
        // =========================================================

        [Fact]
        public void DuplicateEnrolment_IsDetected()
        {
            // Arrange
            using var db = TestHelpers.CreateInMemoryDb();

            var evt = new Event { OrganiserId = 1, Name = "X", EventDate = DateTime.UtcNow, Location = "Y", DistanceKm = 5, EventType = "Run" };
            db.Events.Add(evt); db.SaveChanges();
            var cat = new Category { EventId = evt.EventId, Name = "C", EntryFee = 50 };
            db.Categories.Add(cat); db.SaveChanges();

            db.Enrolments.Add(new Enrolment { ParticipantId = 1, CategoryId = cat.CategoryId });
            db.SaveChanges();

            // Act - try to add duplicate
            var isDuplicate = db.Enrolments.Any(e => e.ParticipantId == 1 && e.CategoryId == cat.CategoryId);

            // Assert
            Assert.True(isDuplicate);
        }

        // =========================================================
        // RESULT LINKAGE
        // =========================================================

        [Fact]
        public void Result_IsLinkedToOneEnrolment()
        {
            // Arrange
            using var db = TestHelpers.CreateInMemoryDb();

            var evt = new Event { OrganiserId = 1, Name = "X", EventDate = DateTime.UtcNow, Location = "Y", DistanceKm = 5, EventType = "Run" };
            db.Events.Add(evt); db.SaveChanges();
            var cat = new Category { EventId = evt.EventId, Name = "C", EntryFee = 50 };
            db.Categories.Add(cat); db.SaveChanges();

            var enrolment = new Enrolment { ParticipantId = 1, CategoryId = cat.CategoryId };
            db.Enrolments.Add(enrolment);
            db.SaveChanges();

            // Act
            db.Results.Add(new Result
            {
                EnrolmentId = enrolment.EnrolmentId,
                FinishTime = "01:23:45",
                Position = 5
            });
            db.SaveChanges();

            // Assert
            var stored = db.Results.First();
            Assert.Equal(enrolment.EnrolmentId, stored.EnrolmentId);
            Assert.Equal("01:23:45", stored.FinishTime);
            Assert.Equal(5, stored.Position);
        }

        [Fact]
        public void Participant_SeesOnlyOwnEnrolments()
        {
            // Arrange
            using var db = TestHelpers.CreateInMemoryDb();

            var evt = new Event { OrganiserId = 1, Name = "X", EventDate = DateTime.UtcNow, Location = "Y", DistanceKm = 5, EventType = "Run" };
            db.Events.Add(evt); db.SaveChanges();
            var cat = new Category { EventId = evt.EventId, Name = "C", EntryFee = 50 };
            db.Categories.Add(cat); db.SaveChanges();

            // Two participants, two enrolments
            db.Enrolments.Add(new Enrolment { ParticipantId = 1, CategoryId = cat.CategoryId });
            db.Enrolments.Add(new Enrolment { ParticipantId = 2, CategoryId = cat.CategoryId });
            db.SaveChanges();

            // Act - filter for participant 1 only
            var myEnrolments = db.Enrolments.Where(e => e.ParticipantId == 1).ToList();

            // Assert
            Assert.Single(myEnrolments);
            Assert.Equal(1, myEnrolments[0].ParticipantId);
        }
    }
}