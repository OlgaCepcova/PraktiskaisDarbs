using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PD3_Web.Data;
using PD3_Web.ViewModels;

namespace PD3_Web.Controllers;

public class QueriesController : Controller
{
    private readonly AppDbContext _db;
    public QueriesController(AppDbContext db) => _db = db;

    // Meklēt studentu pēc vārda
    [HttpGet]
    public IActionResult StudentSearch() => View();

    [HttpPost]
    public async Task<IActionResult> StudentSearch(string namePart)
    {
        namePart = (namePart ?? "").Trim();

        var students = await _db.Students.AsNoTracking()
            .Where(s => namePart != "" && s.FullName.Contains(namePart))
            .OrderBy(s => s.FullName)
            .ToListAsync();

        ViewBag.NamePart = namePart;
        return View(students);
    }

    // Kursi ar min kredītiem
    [HttpGet]
    public IActionResult CoursesMinCredits() => View();

    [HttpPost]
    public async Task<IActionResult> CoursesMinCredits(int minCredits)
    {
        var courses = await _db.Courses.AsNoTracking()
            .Where(c => c.Credits >= minCredits)
            .OrderBy(c => c.Credits)
            .ThenBy(c => c.Title)
            .ToListAsync();

        ViewBag.MinCredits = minCredits;
        return View(courses);
    }

    // Reģistrācijas pēc StudentId
    [HttpGet]
    public IActionResult EnrollmentsByStudent() => View();

    [HttpPost]
    public async Task<IActionResult> EnrollmentsByStudent(int studentId)
    {
        var enrollments = await _db.Enrollments.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        ViewBag.StudentId = studentId;
        return View(enrollments);
    }

    // Studenti + reģistrāciju skaits
    [HttpGet]
    public async Task<IActionResult> StudentEnrollmentCounts()
    {
        var data = await _db.Students.AsNoTracking()
            .Select(s => new StudentEnrollCountRow
            {
                StudentId = s.Id,
                FullName = s.FullName,
                Age = s.Age,
                EnrollmentCount = s.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ThenBy(x => x.FullName)
            .ToListAsync();

        return View(data);
    }

    // TOP kursi pēc reģistrāciju skaita
    [HttpGet]
    public async Task<IActionResult> TopCourses()
    {
        var data = await _db.Courses.AsNoTracking()
            .Select(c => new CourseTopRow
            {
                CourseId = c.Id,
                Title = c.Title,
                Credits = c.Credits,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ThenBy(x => x.Title)
            .ToListAsync();

        return View(data);
    }

    // Pēdējās reģistrācijas
    [HttpGet]
    public async Task<IActionResult> LatestEnrollments()
    {
        var data = await _db.Enrollments.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .OrderByDescending(e => e.EnrolledAt)
            .Take(10)
            .ToListAsync();

        return View(data);
    }
}


