using System.ComponentModel.DataAnnotations;

namespace PD1_Console_EF.Models;

public class Student
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string FullName { get; set; } = "";

    [Range(1, 120)]
    public int Age { get; set; }

    public List<Enrollment> Enrollments { get; set; } = new();
}

