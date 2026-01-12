using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PD3_Web.Data;
using PD3_Web.Dtos;

namespace PD3_Web.Api;

[ApiController]
[Route("api/[controller]")]
public class QueriesApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public QueriesApiController(AppDbContext db) => _db = db;

    // 3x POST (ar ievadi)

    // 1) Meklēt studentu pēc vārda daļas
    // POST: /api/queriesapi/students/search
    [HttpPost("students/search")]
    public async Task<IActionResult> StudentSearch([FromBody] StudentSearchRequest req)
    {
        var namePart = (req?.NamePart ?? "").Trim();
        if (string.IsNullOrWhiteSpace(namePart))
            return BadRequest("NamePart nedrīkst būt tukšs.");

        var students = await _db.Students.AsNoTracking()
            .Where(s => s.FullName.Contains(namePart))
            .OrderBy(s => s.FullName)
            .Select(s => new { s.Id, s.FullName, s.Age })
            .ToListAsync();

        return Ok(students);
    }

    // 2) Kursi ar min kredītiem
    // POST: /api/queriesapi/courses/min-credits
    [HttpPost("courses/min-credits")]
    public async Task<IActionResult> CoursesMinCredits([FromBody] CoursesMinCreditsRequest req)
    {
        if (req == null) return BadRequest("Body ir obligāts.");
        if (req.MinCredits < 1) return BadRequest("MinCredits jābūt >= 1.");

        var courses = await _db.Courses.AsNoTracking()
            .Where(c => c.Credits >= req.MinCredits)
            .OrderBy(c => c.Credits)
            .ThenBy(c => c.Title)
            .Select(c => new { c.Id, c.Title, c.Credits })
            .ToListAsync();

        return Ok(courses);
    }

    // 3) Reģistrācijas pēc StudentId
    // POST: /api/queriesapi/enrollments/by-student
    [HttpPost("enrollments/by-student")]
    public async Task<IActionResult> EnrollmentsByStudent([FromBody] EnrollmentsByStudentRequest req)
    {
        if (req == null) return BadRequest("Body ir obligāts.");
        if (req.StudentId <= 0) return BadRequest("StudentId jābūt > 0.");

        var data = await _db.Enrollments.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.StudentId == req.StudentId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new
            {
                e.Id,
                Student = e.Student!.FullName,
                Course = e.Course!.Title,
                e.EnrolledAt
            })
            .ToListAsync();

        return Ok(data);
    }

    // 3x GET (bez ievades)

    // 4) Studenti + reģistrāciju skaits
    // GET: /api/queriesapi/students/enrollment-counts
    [HttpGet("students/enrollment-counts")]
    public async Task<IActionResult> StudentEnrollmentCounts()
    {
        var data = await _db.Students.AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.FullName,
                s.Age,
                EnrollmentCount = s.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ThenBy(x => x.FullName)
            .ToListAsync();

        return Ok(data);
    }

    // 5) TOP kursi pēc reģistrāciju skaita
    // GET: /api/queriesapi/courses/top
    [HttpGet("courses/top")]
    public async Task<IActionResult> TopCourses()
    {
        var data = await _db.Courses.AsNoTracking()
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Credits,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ThenBy(x => x.Title)
            .ToListAsync();

        return Ok(data);
    }

    // 6) Pēdējās 10 reģistrācijas
    // GET: /api/queriesapi/enrollments/latest
    [HttpGet("enrollments/latest")]
    public async Task<IActionResult> LatestEnrollments()
    {
        var data = await _db.Enrollments.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .OrderByDescending(e => e.EnrolledAt)
            .Take(10)
            .Select(e => new
            {
                e.Id,
                Student = e.Student!.FullName,
                Course = e.Course!.Title,
                e.EnrolledAt
            })
            .ToListAsync();

        return Ok(data);
    }

    // DELETE

    // 7) Dzēst reģistrāciju pēc Id
    // DELETE: /api/queriesapi/enrollments/{id}
    [HttpDelete("enrollments/{id:int}")]
    public async Task<IActionResult> DeleteEnrollment(int id)
    {
        var e = await _db.Enrollments.FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound("Enrollment nav atrasts.");

        _db.Enrollments.Remove(e);
        await _db.SaveChangesAsync();
        return Ok(new { Message = "Dzēsts", EnrollmentId = id });
    }
}

