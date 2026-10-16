using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts
{
    // ============================================
    // AUTH DTOs
    // ============================================
    public class RegisterRequest
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, RegularExpression("Organiser|Participant",
            ErrorMessage = "Role must be 'Organiser' or 'Participant'.")]
        public string Role { get; set; } = string.Empty;

        // Optional role-specific fields
        public string? OrganisationName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmergencyContact { get; set; }
    }

    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // ============================================
    // PROFILE DTOs
    // ============================================
    public class ProfileDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string? OrganisationName { get; set; }  // Organiser only
        public string? PhoneNumber { get; set; }
        public string? EmergencyContact { get; set; }  // Participant only
    }

    public class UpdateProfileRequest
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public string? OrganisationName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmergencyContact { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    // ============================================
    // EVENT DTOs
    // ============================================
    public class CreateEventRequest
    {
        [Required, StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required, StringLength(255)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
        public decimal DistanceKm { get; set; }   // ⭐ Required by feedback

        public string? RouteDetails { get; set; }

        [Required, RegularExpression("Run|Walk|Cycle")]
        public string EventType { get; set; } = string.Empty;

        public int? MaxParticipants { get; set; }
    }

    public class UpdateEventRequest : CreateEventRequest { }

    public class EventDto
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public string? RouteDetails { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int? MaxParticipants { get; set; }
        public int OrganiserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ============================================
    // CATEGORY DTOs
    // ============================================
    public class CreateCategoryRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required, Range(0, 100000)]
        public decimal EntryFee { get; set; }
    }

    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal EntryFee { get; set; }
    }

    // ============================================
    // ENROLMENT DTOs
    // ============================================
    public class CreateEnrolmentRequest
    {
        [Required]
        public int CategoryId { get; set; }
    }

    public class UpdateEnrolmentStatusRequest
    {
        [Required, RegularExpression("Pending|Confirmed|Cancelled")]
        public string Status { get; set; } = string.Empty;
    }

    public class EnrolmentDto
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // ============================================
    // RESULT DTOs
    // ============================================
    public class CreateResultRequest
    {
        [Required]
        public int EnrolmentId { get; set; }

        [Required, RegularExpression(@"^\d{1,2}:\d{2}:\d{2}$",
            ErrorMessage = "Finish time must be in format HH:MM:SS")]
        public string FinishTime { get; set; } = string.Empty;

        [Required, Range(1, int.MaxValue)]
        public int Position { get; set; }

        public string? Notes { get; set; }
    }

    public class ResultDto
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string FinishTime { get; set; } = string.Empty;
        public int Position { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}