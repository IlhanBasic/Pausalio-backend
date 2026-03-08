using Microsoft.EntityFrameworkCore;

namespace Pausalio.Functions
{
    public class UserProfile
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
    }

    public class UserBusinessProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserProfile User { get; set; } = null!;
        public Guid BusinessProfileId { get; set; }
        public BusinessProfile BusinessProfile { get; set; } = null!;
    }

    public class BusinessProfile
    {
        public Guid Id { get; set; }
        public string BusinessName { get; set; } = null!;
        public ICollection<UserBusinessProfile> UserBusinessProfiles { get; set; } = new List<UserBusinessProfile>();
        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    }

    public class Reminder
    {
        public Guid Id { get; set; }
        public Guid BusinessProfileId { get; set; }
        public BusinessProfile BusinessProfile { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class PausalioFunctionsDbContext : DbContext
    {
        public PausalioFunctionsDbContext(DbContextOptions<PausalioFunctionsDbContext> options)
            : base(options) { }

        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<BusinessProfile> BusinessProfiles { get; set; }
        public DbSet<UserBusinessProfile> UserBusinessProfiles { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserBusinessProfile>()
                .HasOne(ubp => ubp.User)
                .WithMany()
                .HasForeignKey(ubp => ubp.UserId);

            modelBuilder.Entity<UserBusinessProfile>()
                .HasOne(ubp => ubp.BusinessProfile)
                .WithMany(bp => bp.UserBusinessProfiles)
                .HasForeignKey(ubp => ubp.BusinessProfileId);

            modelBuilder.Entity<Reminder>()
                .HasOne(r => r.BusinessProfile)
                .WithMany(bp => bp.Reminders)
                .HasForeignKey(r => r.BusinessProfileId);
        }
    }
}