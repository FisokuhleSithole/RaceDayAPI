using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    /// <summary>
    /// Separate Result entity, replacing the fields that were previously inside Enrolment.
    /// Captures the participant's finish time and position.
    /// </summary>
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        [Required, StringLength(20)]
        public string FinishTime { get; set; } = string.Empty;  // HH:MM:SS

        [Required]
        public int Position { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(EnrolmentId))]
        public Enrolment? Enrolment { get; set; }
    }
}