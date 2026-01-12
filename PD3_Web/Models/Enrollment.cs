using System.ComponentModel.DataAnnotations;

namespace PD3_Web.Models;

public class Enrollment
{
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }
    public Student? Student { get; set; }

    [Required]
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    [Required]
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}


