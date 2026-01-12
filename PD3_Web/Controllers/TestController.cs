using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PD3_Web.Data;

namespace PD3_Web.Controllers;

public class TestController : Controller
{
    private readonly AppDbContext _db;
    public TestController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var students = await _db.Students.AsNoTracking().OrderBy(s => s.Id).ToListAsync();
        return Content($"Studentu skaits: {students.Count}");
    }
}

