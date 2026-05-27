using System.Runtime.InteropServices.JavaScript;
using Microsoft.EntityFrameworkCore;

namespace MVCProject.Models;

public class ApplicationDbContext : DbContext
{
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        
    }
    
    public DbSet<Question> Questions { get; set; }
    public DbSet<Course> Courses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>()
            .HasMany(c => c.Questions)
            .WithOne(c => c.Course)
            .HasForeignKey(q => q.CourseId);

    }
}