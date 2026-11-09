using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Mvc.ViewModels
{
    public class EventFormVm
    {
        public int EventId { get; set; }

        public IFormFile? BannerFile { get; set; }

        [Required] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        [Required] public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);

        [Required] public string Location { get; set; } = string.Empty;

        [Required, Range(0.1, 1000)] public decimal DistanceKm { get; set; }

        [Required] public string EventType { get; set; } = "Run";

        public int? MaxParticipants { get; set; }
    }
}