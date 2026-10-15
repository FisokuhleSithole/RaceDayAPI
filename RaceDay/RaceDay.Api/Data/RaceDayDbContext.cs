using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for the RaceDay system.
    /// Configures the SQL Server connection, entity relationships,
    /// unique constraints, and delete behaviours.
    /// </summary>
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options) { }

        // ============ DbSets ============
        public DbSet<AppUser> Users { get; set; }
        public DbSet<OrganiserProfile> OrganiserProfiles { get; set; }
        public DbSet<ParticipantProfile> ParticipantProfiles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<WeatherInfo> WeatherInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============ Unique constraints ============

            // Each email must be unique
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // One user can only have one Organiser profile
            modelBuilder.Entity<OrganiserProfile>()
                .HasIndex(o => o.UserId)
                .IsUnique();

            // One user can only have one Participant profile
            modelBuilder.Entity<ParticipantProfile>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            // Prevent duplicate enrolments (same participant + same category)
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.ParticipantId, e.CategoryId })
                .IsUnique();

            // ============ Relationships ============

            // One-to-One: Enrolment ↔ Result
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(e => e.Result)
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → OrganiserProfile (1:1)
            modelBuilder.Entity<OrganiserProfile>()
                .HasOne(o => o.User)
                .WithOne(u => u.OrganiserProfile)
                .HasForeignKey<OrganiserProfile>(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → ParticipantProfile (1:1)
            modelBuilder.Entity<ParticipantProfile>()
                .HasOne(p => p.User)
                .WithOne(u => u.ParticipantProfile)
                .HasForeignKey<ParticipantProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Organiser → Events (1:many)
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============ Cascade behaviour adjustments ============
            // Prevent multiple cascade paths to Enrolments.
            // Deleting a Participant or Category will NOT auto-delete enrolments.

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============ Multiple cascade path fix ============
            // Deleting a Participant or Category will NOT auto-delete enrolments.
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}