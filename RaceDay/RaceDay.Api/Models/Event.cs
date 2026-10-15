using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int OrganiserId { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required, StringLength(255)]
        public string Location { get; set; } = string.Empty;

        // ⭐ Required field per Part 1 feedback
        [Required]
        [Column(TypeName = "decimal(6,2)")]
        public decimal DistanceKm { get; set; }

        public string? RouteDetails { get; set; }

        [Required, StringLength(50)]
        public string EventType { get; set; } = string.Empty;  // Run, Walk, Cycle

        public int? MaxParticipants { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(OrganiserId))]
        public OrganiserProfile? Organiser { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<WeatherInfo> WeatherInfos { get; set; } = new List<WeatherInfo>();
    }
}