using Microsoft.EntityFrameworkCore;
using PD1_Console_EF.Data;
using PD1_Console_EF.Models;

static string ReadRequired(string label)
{
    while (true)
    {
        Console.Write(label);
        var s = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(s)) return s;
        Console.WriteLine("Tukšs ievads nav atļauts.");
    }
}

static int ReadInt(string label, int? min = null, int? max = null)
{
    while (true)
    {
        Console.Write(label);
        var s = Console.ReadLine()?.Trim();

        if (int.TryParse(s, out var value))
        {
            if (min.HasValue && value < min.Value)
            {
                Console.WriteLine($"Skaitlim jābūt >= {min.Value}");
                continue;
            }
            if (max.HasValue && value > max.Value)
            {
                Console.WriteLine($"Skaitlim jābūt <= {max.Value}");
                continue;
            }
            return value;
        }

        Console.WriteLine("Ievadi veselu skaitli.");
    }
}

static void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("====== PD1 Console EF ======");
    Console.WriteLine("1) Rādīt visus studentus");
    Console.WriteLine("2) Rādīt visus kursus");
    Console.WriteLine("3) Rādīt visas reģistrācijas (Enrollments)");
    Console.WriteLine("4) Pievienot studentu");
    Console.WriteLine("5) Pievienot kursu");
    Console.WriteLine("6) Pievienot reģistrāciju (Students -> Kurss)");
    Console.WriteLine("7) LINQ: Meklēt studentu pēc vārda");
    Console.WriteLine("8) LINQ: Kursi ar min kredītiem");
    Console.WriteLine("9) LINQ: Reģistrācijas pēc StudentId");
    Console.WriteLine("10) LINQ: Studenti + reģistrāciju skaits");
    Console.WriteLine("11) LINQ: TOP kursi pēc reģistrāciju skaita");
    Console.WriteLine("12) LINQ: Pēdējās reģistrācijas");
    Console.WriteLine("0) Iziet");
    Console.WriteLine("==========================");
}

static async Task EnsureDatabaseAndSeedAsync(AppDbContext db)
{
    
    await db.Database.MigrateAsync();

    if (!await db.Students.AnyAsync())
    {
        db.Students.AddRange(
            new Student { FullName = "Anna Bērziņa", Age = 20 },
            new Student { FullName = "Jānis Kalniņš", Age = 22 }
        );
    }

    if (!await db.Courses.AnyAsync())
    {
        db.Courses.AddRange(
            new Course { Title = "Web tehnoloģijas", Credits = 6 },
            new Course { Title = "Datubāzes", Credits = 4 }
        );
    }

    await db.SaveChangesAsync();

    if (!await db.Enrollments.AnyAsync())
    {
        var s1 = await db.Students.FirstAsync();
        var c1 = await db.Courses.FirstAsync();
        db.Enrollments.Add(new Enrollment { StudentId = s1.Id, CourseId = c1.Id, EnrolledAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }
}

using var db = new AppDbContext();
await EnsureDatabaseAndSeedAsync(db);

while (true)
{
    PrintMenu();
    var choice = ReadRequired("Izvēle: ");

    switch (choice)
    {
        case "1":
        {
            var students = await db.Students.AsNoTracking().OrderBy(s => s.Id).ToListAsync();
            Console.WriteLine("\n---- STUDENTI ----");
            if (students.Count == 0) Console.WriteLine("(nav ierakstu)");
            foreach (var s in students)
                Console.WriteLine($"{s.Id}. {s.FullName} (Age: {s.Age})");
            break;
        }

        case "2":
        {
            var courses = await db.Courses.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
            Console.WriteLine("\n---- KURSI ----");
            if (courses.Count == 0) Console.WriteLine("(nav ierakstu)");
            foreach (var c in courses)
                Console.WriteLine($"{c.Id}. {c.Title} (Credits: {c.Credits})");
            break;
        }

        case "3":
        {
            var enrollments = await db.Enrollments
                .AsNoTracking()
                .Include(e => e.Student)
                .Include(e => e.Course)
                .OrderBy(e => e.Id)
                .ToListAsync();

            Console.WriteLine("\n---- REĢISTRĀCIJAS (ENROLLMENTS) ----");
            if (enrollments.Count == 0) Console.WriteLine("(nav ierakstu)");
            foreach (var e in enrollments)
                Console.WriteLine($"{e.Id}. {e.Student?.FullName} -> {e.Course?.Title} ({e.EnrolledAt:yyyy-MM-dd})");
            break;
        }

        case "4":
        {
            Console.WriteLine("\n---- PIEVIENOT STUDENTU ----");
            var name = ReadRequired("Vārds Uzvārds: ");
            var age = ReadInt("Vecums: ", 1, 120);

            db.Students.Add(new Student { FullName = name, Age = age });
            await db.SaveChangesAsync();
            Console.WriteLine("Students pievienots.");
            break;
        }

        case "5":
        {
            Console.WriteLine("\n---- PIEVIENOT KURSU ----");
            var title = ReadRequired("Kursa nosaukums: ");
            var credits = ReadInt("Kredītpunkti: ", 1, 60);

            db.Courses.Add(new Course { Title = title, Credits = credits });
            await db.SaveChangesAsync();
            Console.WriteLine("Kurss pievienots.");
            break;
        }

        case "6":
        {
            Console.WriteLine("\n---- PIEVIENOT REĢISTRĀCIJU ----");

            // parāda sarakstus, lai vieglāk izvēlēties ID
            var students = await db.Students.AsNoTracking().OrderBy(s => s.Id).ToListAsync();
            var courses = await db.Courses.AsNoTracking().OrderBy(c => c.Id).ToListAsync();

            if (students.Count == 0 || courses.Count == 0)
            {
                Console.WriteLine("Nav studentu vai kursu. Vispirms pievieno studentu un kursu.");
                break;
            }

            Console.WriteLine("\nStudenti:");
            foreach (var s in students) Console.WriteLine($"{s.Id}. {s.FullName}");

            Console.WriteLine("\nKursi:");
            foreach (var c in courses) Console.WriteLine($"{c.Id}. {c.Title}");

            var studentId = ReadInt("Ievadi StudentId: ", 1, int.MaxValue);
            var courseId = ReadInt("Ievadi CourseId: ", 1, int.MaxValue);

            // pārbaudes
            var studentExists = await db.Students.AnyAsync(s => s.Id == studentId);
            var courseExists = await db.Courses.AnyAsync(c => c.Id == courseId);

            if (!studentExists) { Console.WriteLine("StudentId nav atrasts."); break; }
            if (!courseExists) { Console.WriteLine("CourseId nav atrasts."); break; }

            // unikālais indekss neļaus dubultot, bet tiek parādīts paziņojums
            var already = await db.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
            if (already) { Console.WriteLine("Šāda reģistrācija jau eksistē."); break; }

            db.Enrollments.Add(new Enrollment { StudentId = studentId, CourseId = courseId, EnrolledAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
            Console.WriteLine("Reģistrācija pievienota.");
            break;
        }

        case "7":
            {
                Console.WriteLine("\n---- LINQ: MEKLĒT STUDENTU PĒC VĀRDA ----");
                var q = ReadRequired("Ievadi vārdu vai daļu no vārda: ").ToLower();

                var result = await db.Students.AsNoTracking()
                    .Where(s => s.FullName.ToLower().Contains(q))
                    .OrderBy(s => s.FullName)
                    .Select(s => new { s.Id, s.FullName, s.Age })
                    .ToListAsync();

                if (result.Count == 0) Console.WriteLine("(nav atrasts)");
                else
                    foreach (var s in result)
                        Console.WriteLine($"{s.Id}. {s.FullName} (Age: {s.Age})");

                break;
            }

        case "8":
            {
                Console.WriteLine("\n---- LINQ: KURSI AR MIN KREDĪTIEM ----");
                var minCredits = ReadInt("Min kredītpunkti: ", 1, 60);

                var result = await db.Courses.AsNoTracking()
                    .Where(c => c.Credits >= minCredits)
                    .OrderByDescending(c => c.Credits)
                    .Select(c => new { c.Id, c.Title, c.Credits })
                    .ToListAsync();

                if (result.Count == 0) Console.WriteLine("(nav atrasts)");
                else
                    foreach (var c in result)
                        Console.WriteLine($"{c.Id}. {c.Title} (Credits: {c.Credits})");

                break;
            }

        case "9":
            {
                Console.WriteLine("\n---- LINQ: REĢISTRĀCIJAS PĒC STUDENTID ----");
                var studentId = ReadInt("Ievadi StudentId: ", 1, int.MaxValue);

                var result = await db.Enrollments.AsNoTracking()
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .Where(e => e.StudentId == studentId)
                    .OrderByDescending(e => e.EnrolledAt)
                    .Select(e => new
                    {
                        e.Id,
                        Student = e.Student!.FullName,
                        Course = e.Course!.Title,
                        e.EnrolledAt
                    })
                    .ToListAsync();

                if (result.Count == 0) Console.WriteLine("(nav atrasts)");
                else
                    foreach (var e in result)
                        Console.WriteLine($"{e.Id}. {e.Student} -> {e.Course} ({e.EnrolledAt:yyyy-MM-dd})");

                break;
            }

        case "10":
            {
                Console.WriteLine("\n---- LINQ: STUDENTI + REĢISTRĀCIJU SKAITS ----");

                var result = await db.Students.AsNoTracking()
                    .Select(s => new
                    {
                        s.Id,
                        s.FullName,
                        s.Age,
                        EnrollmentsCount = s.Enrollments.Count
                    })
                    .OrderByDescending(x => x.EnrollmentsCount)
                    .ThenBy(x => x.FullName)
                    .ToListAsync();

                if (result.Count == 0) Console.WriteLine("(nav ierakstu)");
                else
                    foreach (var s in result)
                        Console.WriteLine($"{s.Id}. {s.FullName} (Age: {s.Age}) | Reģistrācijas: {s.EnrollmentsCount}");

                break;
            }

        case "11":
            {
                Console.WriteLine("\n---- LINQ: TOP KURSI PĒC REĢISTRĀCIJĀM ----");

                var result = await db.Courses.AsNoTracking()
                    .Select(c => new
                    {
                        c.Id,
                        c.Title,
                        c.Credits,
                        EnrollmentsCount = c.Enrollments.Count
                    })
                    .OrderByDescending(x => x.EnrollmentsCount)
                    .ThenByDescending(x => x.Credits)
                    .ToListAsync();

                if (result.Count == 0) Console.WriteLine("(nav ierakstu)");
                else
                    foreach (var c in result)
                        Console.WriteLine($"{c.Id}. {c.Title} (Credits: {c.Credits}) | Reģistrācijas: {c.EnrollmentsCount}");

                break;
            }

        case "12":
            {
                Console.WriteLine("\n---- LINQ: PĒDĒJĀS REĢISTRĀCIJAS ----");

                var result = await db.Enrollments.AsNoTracking()
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.EnrolledAt)
                    .Take(5)
                    .Select(e => new
                    {
                        e.Id,
                        Student = e.Student!.FullName,
                        Course = e.Course!.Title,
                        e.EnrolledAt
                    })
                    .ToListAsync();

                if (result.Count == 0) Console.WriteLine("(nav ierakstu)");
                else
                    foreach (var e in result)
                        Console.WriteLine($"{e.Id}. {e.Student} -> {e.Course} ({e.EnrolledAt:yyyy-MM-dd})");

                break;
            }

        case "0":
            Console.WriteLine("Paldies!");
            return;

        default:
            Console.WriteLine("Nepareiza izvēle.");
            break;
    }
}
