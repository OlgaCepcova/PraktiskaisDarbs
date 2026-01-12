using System.ComponentModel.DataAnnotations;

namespace PD3_Web.Models;

public class Student
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string FullName { get; set; } = "";

    [Range(1, 120)]
    public int Age { get; set; }

    public List<Enrollment> Enrollments { get; set; } = new();
}

