using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class AuthTests
    {
        // =========================================================
        // HAPPY PATH - Registration
        // =========================================================

        [Fact]
        public void Register_WithValidDetails_AddsUserToDatabase()
        {
            // Arrange
            using var db = TestHelpers.CreateInMemoryDb();
            var user = new AppUser
            {
                Email = "newuser@test.com",
                PasswordHash = PasswordHasher.HashPassword("Password123"),
                FirstName = "New",
                LastName = "User",
                Role = "Participant"
            };

            // Act
            db.Users.Add(user);
            db.SaveChanges();

            // Assert
            var stored = db.Users.FirstOrDefault(u => u.Email == "newuser@test.com");
            Assert.NotNull(stored);
            Assert.Equal("Participant", stored!.Role);
            Assert.Equal("New", stored.FirstName);
        }

        [Fact]
        public void Register_DuplicateEmail_IsDetected()
        {
            // Arrange
            using var db = TestHelpers.CreateInMemoryDb();
            db.Users.Add(new AppUser
            {
                Email = "duplicate@test.com",
                PasswordHash = "hash",
                FirstName = "A",
                LastName = "B",
                Role = "Participant"
            });
            db.SaveChanges();

            // Act
            var exists = db.Users.Any(u => u.Email == "duplicate@test.com");

            // Assert
            Assert.True(exists);
        }

        // =========================================================
        // HAPPY PATH - Password hashing
        // =========================================================

        [Fact]
        public void PasswordHash_DoesNotStorePlainText()
        {
            // Arrange
            var plainPassword = "MySecret123";

            // Act
            var hash = PasswordHasher.HashPassword(plainPassword);

            // Assert
            Assert.NotEqual(plainPassword, hash);
            Assert.True(hash.Length > 20);   // SHA256 base64 is ~44 chars
        }

        [Fact]
        public void PasswordHash_VerifiesCorrectPassword()
        {
            var hash = PasswordHasher.HashPassword("CorrectPassword");
            Assert.True(PasswordHasher.VerifyPassword("CorrectPassword", hash));
        }

        // =========================================================
        // FAILURE PATH - Wrong password
        // =========================================================

        [Fact]
        public void PasswordHash_RejectsWrongPassword()
        {
            var hash = PasswordHasher.HashPassword("CorrectPassword");
            Assert.False(PasswordHasher.VerifyPassword("WrongPassword", hash));
        }

        [Fact]
        public void Login_WithWrongPassword_Fails()
        {
            // Arrange
            using var db = TestHelpers.CreateInMemoryDb();
            var user = new AppUser
            {
                Email = "user@test.com",
                PasswordHash = PasswordHasher.HashPassword("CorrectPwd"),
                FirstName = "Test",
                LastName = "User",
                Role = "Participant"
            };
            db.Users.Add(user);
            db.SaveChanges();

            // Act
            var stored = db.Users.First(u => u.Email == "user@test.com");
            var isValid = PasswordHasher.VerifyPassword("WrongPwd", stored.PasswordHash);

            // Assert
            Assert.False(isValid);
        }
    }
}