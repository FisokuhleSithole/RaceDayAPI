using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class ParticipantProfile
    {
        [Key]
        public int ParticipantId { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? EmergencyContact { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser? User { get; set; }

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}