using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    /// <summary>
    /// Base user entity for both Organisers and Participants.
    /// </summary>
    public class AppUser
    {
        [Key]
        public int UserId { get; set; }

        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Role { get; set; } = string.Empty;  // "Organiser" or "Participant"

        [StringLength(500)]
        public string? ProfilePictureUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public OrganiserProfile? OrganiserProfile { get; set; }
        public ParticipantProfile? ParticipantProfile { get; set; }
    }
}