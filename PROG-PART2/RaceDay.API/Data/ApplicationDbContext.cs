using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets - these represent tables in the database
        public DbSet<User> Users { get; set; }
        public DbSet<OrganiserProfile> OrganiserProfiles { get; set; }
        public DbSet<ParticipantProfile> ParticipantProfiles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================================
            // USER CONFIGURATION
            // =============================================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            // =============================================
            // ORGANISER PROFILE CONFIGURATION
            // =============================================
            modelBuilder.Entity<OrganiserProfile>(entity =>
            {
                entity.HasKey(e => e.ProfileId);
                entity.HasIndex(e => e.UserId).IsUnique();

                entity.HasOne(e => e.User)
                    .WithOne(e => e.OrganiserProfile)
                    .HasForeignKey<OrganiserProfile>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =============================================
            // PARTICIPANT PROFILE CONFIGURATION
            // =============================================
            modelBuilder.Entity<ParticipantProfile>(entity =>
            {
                entity.HasKey(e => e.ProfileId);
                entity.HasIndex(e => e.UserId).IsUnique();

                entity.HasOne(e => e.User)
                    .WithOne(e => e.ParticipantProfile)
                    .HasForeignKey<ParticipantProfile>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =============================================
            // EVENT CONFIGURATION
            // =============================================
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.EventId);

                entity.Property(e => e.EventType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasOne(e => e.Organiser)
                    .WithMany(o => o.Events)
                    .HasForeignKey(e => e.OrganiserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =============================================
            // CATEGORY CONFIGURATION
            // =============================================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);

                entity.HasOne(e => e.Event)
                    .WithMany(ev => ev.Categories)
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =============================================
            // ENROLMENT CONFIGURATION
            // =============================================
            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.HasKey(e => e.EnrolmentId);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Registered");

                entity.HasIndex(e => e.BibNumber).IsUnique();

                entity.HasOne(e => e.Participant)
                    .WithMany(p => p.Enrolments)
                    .HasForeignKey(e => e.ParticipantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Event)
                    .WithMany(ev => ev.Enrolments)
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Enrolments)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =============================================
            // RESULT CONFIGURATION
            // =============================================
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(e => e.ResultId);
                entity.HasIndex(e => e.EnrolmentId).IsUnique();

                entity.HasOne(e => e.Enrolment)
                    .WithOne(en => en.Result)
                    .HasForeignKey<Result>(e => e.EnrolmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}