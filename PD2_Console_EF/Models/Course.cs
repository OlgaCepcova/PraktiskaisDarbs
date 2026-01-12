using System.ComponentModel.DataAnnotations;

namespace PD1_Console_EF.Models;

public class Course
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Title { get; set; } = "";

    [Range(1, 60)]
    public int Credits { get; set; }

    public List<Enrollment> Enrollments { get; set; } = new();
}

