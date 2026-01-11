using Microsoft.EntityFrameworkCore;
using PD1_Console_EF.Models;

namespace PD1_Console_EF.Data;

public class AppDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=pd1_console_ef.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // lai nav dubultie ieraksti vienam studentam tajā pašā kursā
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();
    }
}

