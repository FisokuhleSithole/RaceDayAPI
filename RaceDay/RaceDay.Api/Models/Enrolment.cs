using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        [Required, StringLength(50)]
        public string Status { get; set; } = "Pending";  // Pending, Confirmed, Cancelled

        [ForeignKey(nameof(ParticipantId))]
        public ParticipantProfile? Participant { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        // One-to-one with Result
        public Result? Result { get; set; }
    }
}