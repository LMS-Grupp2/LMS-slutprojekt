using Domain.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<ActivityType> ActivityTypes { get; set; }
    public DbSet<CourseUser> CourseUsers { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CourseUser>()
            .HasIndex(cu => new { cu.CourseId, cu.UserId })
            .IsUnique();

        // Restrict: SQL Server rejects multiple cascade paths from AspNetUsers,
        // and deleting a user must not silently delete course content.
        builder.Entity<Submission>()
            .HasOne(s => s.Student)
            .WithMany()
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Feedback>()
            .HasOne(f => f.Teacher)
            .WithMany()
            .HasForeignKey(f => f.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Document>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Restrict: a type in use can't be deleted. Cascade would wipe every activity using it.
        builder.Entity<Activity>()
            .HasOne(a => a.ActivityType)
            .WithMany(t => t.Activities)
            .HasForeignKey(a => a.ActivityTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Type names must be unique, otherwise the dropdown shows duplicates and data splits between them.
        builder.Entity<ActivityType>()
            .HasIndex(t => t.Name)
            .IsUnique();

        // Seeded through migrations so every database (local, Azure, CI) has the default types.
        // Ids are hardcoded: Guid.NewGuid() would change on every build and make EF re-seed in each migration.
        builder.Entity<ActivityType>().HasData(
            new ActivityType
            {
                Id = Guid.Parse("{59FB72BF-FD37-48DC-8391-5EE265C0072C}"),
                Name = "Lecture",
                Description = "A teacher-led session where new material is presented."
            },
            new ActivityType
            {
                Id = Guid.Parse("{9ACBD254-D038-44B9-BA1F-EEEA65500778}"),
                Name = "E-Learning",
                Description = "Self-paced online material, such as a video course, done on your own time."
            },
            new ActivityType
            {
                Id = Guid.Parse("{8998F0FF-48B5-4245-943F-3579252E4D18}"),
                Name = "Exercise",
                Description = "A practice session where you apply what you've learned."
            },
            new ActivityType
            {
                Id = Guid.Parse("{C10E45B9-FCFE-468C-BC7E-FE29B4FB62EB}"),
                Name = "Assignment",
                Description = "A task to complete and hand in for review by the teacher."
            },
            new ActivityType
            {
                Id = Guid.Parse("{47C20D6B-DAF9-4180-A01C-092F5337AF72}"),
                Name = "Other",
                Description = "Any other scheduled activity, such as a kickoff or a meeting."
            }
        );

    }
}
