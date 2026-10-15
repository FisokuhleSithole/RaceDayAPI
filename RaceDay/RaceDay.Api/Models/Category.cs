using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EntryFee { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}