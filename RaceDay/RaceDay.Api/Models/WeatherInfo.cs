using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class WeatherInfo
    {
        [Key]
        public int WeatherId { get; set; }

        [Required]
        public int EventId { get; set; }

        public DateTime ForecastDate { get; set; }

        [StringLength(100)]
        public string? Condition { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TemperatureCelsius { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? WindSpeedKmph { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }
    }
}