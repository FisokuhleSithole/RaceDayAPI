using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class OrganiserProfile
    {
        [Key]
        public int OrganiserId { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(255)]
        public string? OrganisationName { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser? User { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}