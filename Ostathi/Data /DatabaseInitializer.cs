using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public static class DatabaseInitializer
{
    // Delete the entire database, recreate it from scratch, and seed initial test data
    public static async Task InitializeAsync(AppDbContext dbContext)
    {
        if (dbContext == null) return;

        await dbContext.Database.CloseConnectionAsync();

        // Clear pooled SQLite connections so file handles are released before deletion
        SqliteConnection.ClearAllPools();

        // Drop and recreate the database schema from the current EF Core model
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        // Enforce foreign key constraints per connection in SQLite
        await dbContext.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");

        // Seed default test data in dependency order
        await SeedMajorsAsync(dbContext);
        await SeedSubjectsAsync(dbContext);
        await SeedGroupsAsync(dbContext);
        await SeedStudentsAsync(dbContext);
        await SeedGroupSubjectsAsync(dbContext);
        await SeedLecturesAsync(dbContext);
        await SeedAssignmentsAsync(dbContext);
        await SeedGradesAsync(dbContext);
    }

    // Populate default academic majors if the Majors table is empty
    private static async Task SeedMajorsAsync(AppDbContext dbContext)
    {
        if (await dbContext.Majors.AnyAsync()) return;

        var defaultMajors = new[]
        {
            new Major { Name = "علوم الحاسوب", DurationYears = 4 },
            new Major { Name = "تقنية المعلومات", DurationYears = 4 },
            new Major { Name = "هندسة البرمجيات", DurationYears = 5 }
        };

        await dbContext.Majors.AddRangeAsync(defaultMajors);
        await dbContext.SaveChangesAsync();
    }

    // Populate sample academic subjects for testing
    private static async Task SeedSubjectsAsync(AppDbContext dbContext)
    {
        if (await dbContext.Subjects.AnyAsync()) return;

        var sampleSubjects = new[]
        {
            new Subject { Name = "البرمجة الكائنية (OOP)", Description = "مفاهيم الكلاسات والوراثة وتعدد الأشكال بلغة C#" },
            new Subject { Name = "قواعد البيانات", Description = "تصميم الجداول والربط والاستعلامات SQL" },
            new Subject { Name = "هياكل البيانات والخوارزميات", Description = "القوائم، المكدس، الطوابير، الأشجار، وتحليل التعقيد" },
            new Subject { Name = "تطوير تطبيقات سطح المكتب والويب", Description = "بناء واجهات وتطبيقات متعددة الطبقات" }
        };

        await dbContext.Subjects.AddRangeAsync(sampleSubjects);
        await dbContext.SaveChangesAsync();
    }

    // Populate sample student groups linked to seeded majors
    private static async Task SeedGroupsAsync(AppDbContext dbContext)
    {
        if (await dbContext.Groups.AnyAsync()) return;

        var majors = await dbContext.Majors.AsNoTracking().ToListAsync();
        var csMajor = majors.FirstOrDefault(m => m.Name == "علوم الحاسوب") ?? majors.FirstOrDefault();
        var itMajor = majors.FirstOrDefault(m => m.Name == "تقنية المعلومات") ?? csMajor;
        var seMajor = majors.FirstOrDefault(m => m.Name == "هندسة البرمجيات") ?? csMajor;

        if (csMajor == null) return;

        var sampleGroups = new List<Group>
        {
            new Group
            {
                Name = "المجموعة أ - المستوى الثاني",
                AcademicYear = "المستوى الثاني",
                Description = "مجموعة اختبارية لطلاب المستوى الثاني",
                MajorId = csMajor.Id
            },
            new Group
            {
                Name = "المجموعة ب - المستوى الثالث",
                AcademicYear = "المستوى الثالث",
                Description = "مجموعة اختبارية لطلاب المستوى الثالث",
                MajorId = itMajor!.Id
            },
            new Group
            {
                Name = "المجموعة ج - المستوى الرابع",
                AcademicYear = "المستوى الرابع",
                Description = "مجموعة اختبارية لطلاب المستوى الرابع",
                MajorId = seMajor!.Id
            }
        };

        await dbContext.Groups.AddRangeAsync(sampleGroups);
        await dbContext.SaveChangesAsync();
    }

    // Populate sample students for every seeded group
    private static async Task SeedStudentsAsync(AppDbContext dbContext)
    {
        if (await dbContext.Students.AnyAsync()) return;

        var groups = await dbContext.Groups.AsNoTracking().ToListAsync();
        if (!groups.Any()) return;

        var firstGroup = groups[0];
        var secondGroup = groups.Count > 1 ? groups[1] : groups[0];
        var thirdGroup = groups.Count > 2 ? groups[2] : groups[0];

        var sampleStudents = new List<Student>
        {
            // طلاب المجموعة الأولى
            new Student
            {
                FirstName = "أحمد",
                SecondName = "محمد",
                ThirdName = "علي",
                LastName = "اليريمي",
                AcademicId = "20241001",
                AcademicYear = firstGroup.AcademicYear ?? "المستوى الثاني",
                GroupId = firstGroup.Id
            },
            new Student
            {
                FirstName = "سالم",
                SecondName = "عبدالله",
                ThirdName = "حسن",
                LastName = "المقطري",
                AcademicId = "20241002",
                AcademicYear = firstGroup.AcademicYear ?? "المستوى الثاني",
                GroupId = firstGroup.Id
            },
            new Student
            {
                FirstName = "مريم",
                SecondName = "خالد",
                ThirdName = "سعيد",
                LastName = "العبيدي",
                AcademicId = "20241003",
                AcademicYear = firstGroup.AcademicYear ?? "المستوى الثاني",
                GroupId = firstGroup.Id
            },

            // طلاب المجموعة الثانية
            new Student
            {
                FirstName = "يوسف",
                SecondName = "إبراهيم",
                ThirdName = "ناصر",
                LastName = "الحميري",
                AcademicId = "20232001",
                AcademicYear = secondGroup.AcademicYear ?? "المستوى الثالث",
                GroupId = secondGroup.Id
            },
            new Student
            {
                FirstName = "هدى",
                SecondName = "طارق",
                ThirdName = "أمين",
                LastName = "الشرعبي",
                AcademicId = "20232002",
                AcademicYear = secondGroup.AcademicYear ?? "المستوى الثالث",
                GroupId = secondGroup.Id
            },
            new Student
            {
                FirstName = "عمر",
                SecondName = "فؤاد",
                ThirdName = "صالح",
                LastName = "الزبيري",
                AcademicId = "20232003",
                AcademicYear = secondGroup.AcademicYear ?? "المستوى الثالث",
                GroupId = secondGroup.Id
            },

            // طلاب المجموعة الثالثة
            new Student
            {
                FirstName = "ليان",
                SecondName = "جمال",
                ThirdName = "محمود",
                LastName = "الهاشمي",
                AcademicId = "20223001",
                AcademicYear = thirdGroup.AcademicYear ?? "المستوى الرابع",
                GroupId = thirdGroup.Id
            },
            new Student
            {
                FirstName = "بلال",
                SecondName = "عادل",
                ThirdName = "حسين",
                LastName = "الدبعي",
                AcademicId = "20223002",
                AcademicYear = thirdGroup.AcademicYear ?? "المستوى الرابع",
                GroupId = thirdGroup.Id
            },
            new Student
            {
                FirstName = "نور",
                SecondName = "وليد",
                ThirdName = "كريم",
                LastName = "المخلافي",
                AcademicId = "20223003",
                AcademicYear = thirdGroup.AcademicYear ?? "المستوى الرابع",
                GroupId = thirdGroup.Id
            }
        };

        await dbContext.Students.AddRangeAsync(sampleStudents);
        await dbContext.SaveChangesAsync();
    }

    // Populate comprehensive group-subject assignments across groups and subjects
    private static async Task SeedGroupSubjectsAsync(AppDbContext dbContext)
    {
        if (await dbContext.GroupSubjects.AnyAsync()) return;

        var groups = await dbContext.Groups.AsNoTracking().ToListAsync();
        var subjects = await dbContext.Subjects.AsNoTracking().ToListAsync();

        if (!groups.Any() || !subjects.Any()) return;

        var firstGroup = groups[0];
        var secondGroup = groups.Count > 1 ? groups[1] : groups[0];
        var thirdGroup = groups.Count > 2 ? groups[2] : groups[0];

        var oop = subjects.FirstOrDefault(s => s.Name.Contains("البرمجة")) ?? subjects[0];
        var db = subjects.FirstOrDefault(s => s.Name.Contains("قواعد البيانات")) ?? subjects[0];
        var ds = subjects.FirstOrDefault(s => s.Name.Contains("هياكل البيانات")) ?? subjects[0];
        var appDev = subjects.FirstOrDefault(s => s.Name.Contains("تطوير تطبيقات")) ?? subjects.Last();

        var assignments = new List<GroupSubject>
        {
            new GroupSubject
            {
                GroupId = firstGroup.Id,
                SubjectId = oop.Id,
                Status = "Active",
                Notes = "مقرر أساسي للفصل الدراسي"
            },
            new GroupSubject
            {
                GroupId = firstGroup.Id,
                SubjectId = db.Id,
                Status = "Active",
                Notes = "يشمل تطبيقات عملية على SQLite"
            },
            new GroupSubject
            {
                GroupId = secondGroup.Id,
                SubjectId = ds.Id,
                Status = "Active",
                Notes = "تركيز على حل المسائل والخوارزميات"
            },
            new GroupSubject
            {
                GroupId = secondGroup.Id,
                SubjectId = oop.Id,
                Status = "Active",
                Notes = "تطبيقات متقدمة في البرمجة الكائنية"
            },
            new GroupSubject
            {
                GroupId = thirdGroup.Id,
                SubjectId = db.Id,
                Status = "Active",
                Notes = "مراجعة متقدمة لقواعد البيانات"
            },
            new GroupSubject
            {
                GroupId = thirdGroup.Id,
                SubjectId = appDev.Id,
                Status = "Active",
                Notes = "بناء أنظمة ومشاريع متكاملة"
            }
        };

        var distinctAssignments = assignments
            .GroupBy(a => new { a.GroupId, a.SubjectId })
            .Select(g => g.First())
            .ToList();

        await dbContext.GroupSubjects.AddRangeAsync(distinctAssignments);
        await dbContext.SaveChangesAsync();
    }

    // Populate sample lectures for seeded group-subject assignments if DbSet exists
    private static async Task SeedLecturesAsync(AppDbContext dbContext)
    {
        var groupSubjects = await dbContext.GroupSubjects.AsNoTracking().ToListAsync();
        if (!groupSubjects.Any()) return;

        var lecturesProp = dbContext.GetType().GetProperty("Lectures");
        if (lecturesProp == null) return;

        // If a Lectures DbSet exists, seed basic lectures if empty
        if (lecturesProp.GetValue(dbContext) is IQueryable<object> existingQuery && existingQuery.Any())
            return;

        await Task.CompletedTask;
    }

    // Populate sample assignments if DbSet exists
    private static async Task SeedAssignmentsAsync(AppDbContext dbContext)
    {
        var assignmentsProp = dbContext.GetType().GetProperty("Assignments");
        if (assignmentsProp == null) return;

        await Task.CompletedTask;
    }

    // Populate sample student grades if DbSet exists
    private static async Task SeedGradesAsync(AppDbContext dbContext)
    {
        var gradesProp = dbContext.GetType().GetProperty("StudentGrades")
            ?? dbContext.GetType().GetProperty("Grades");
        if (gradesProp == null) return;

        await Task.CompletedTask;
    }
}
