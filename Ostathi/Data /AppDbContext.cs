using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public class AppDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Major> Majors => Set<Major>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Subject> Subjects => Set<Subject>(); // Added Subjects table
    public DbSet<GroupSubject> GroupSubjects => Set<GroupSubject>(); // Added GroupSubject relation table
    public DbSet<Lecture> Lectures => Set<Lecture>();
    public DbSet<Attendance> Attendances => Set<Attendance>();

    private readonly string _dbPath;

    public AppDbContext()
    {
        var databasePath = FileSystem.AppDataDirectory;
        _dbPath = Path.Combine(databasePath, "Ostathi.db3");
        
        // 1. Delete database completely on every startup for testing
        Database.EnsureDeleted();
        
        // 2. Create a fresh clean database with tables
        Database.EnsureCreated();
        
        // 3. Seed fresh initial data
        SeedDatabase();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Filename={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationship between Groups and Majors
        modelBuilder.Entity<Group>()
            .HasOne<Major>()
            .WithMany()
            .HasForeignKey(g => g.MajorId);

        // Configure Many-to-Many relationship configuration for GroupSubject
        modelBuilder.Entity<GroupSubject>()
            .HasOne(gs => gs.Group)
            .WithMany()
            .HasForeignKey(gs => gs.GroupId);

        modelBuilder.Entity<GroupSubject>()
            .HasOne(gs => gs.Subject)
            .WithMany()
            .HasForeignKey(gs => gs.SubjectId);
    }

    // Method to insert fresh initial data on every startup
    private void SeedDatabase()
    {
        try
        {
            // 1. Seed Majors
            Majors.AddRange(
                new Major { Id = 1, Name = "هندسة البرمجيات" },
                new Major { Id = 2, Name = "علوم الحاسوب" }
            );
            SaveChanges();

            // 2. Seed Groups
            Groups.AddRange(
                new Group { Id = 1, Name = "مجموعة C#", MajorId = 1 },
                new Group { Id = 2, Name = "مجموعة قواعد البيانات", MajorId = 1 },
                new Group { Id = 3, Name = "مجموعة MAUI", MajorId = 2 }
            );
            SaveChanges();

            // 3. Seed Subjects
            Subjects.AddRange(
                new Subject { Id = 1, Name = "برمجة الحاسوب C#" },
                new Subject { Id = 2, Name = "قواعد البيانات SQLite" },
                new Subject { Id = 3, Name = "تطوير واجهات MAUI" }
            );
            SaveChanges();

            // 4. Seed GroupSubjects (Assigning subjects to groups)
            GroupSubjects.AddRange(
                new GroupSubject { Id = 1, GroupId = 1, SubjectId = 1 }, // Group 1 studies Subject 1
                new GroupSubject { Id = 2, GroupId = 1, SubjectId = 2 }, // Group 1 studies Subject 2
                new GroupSubject { Id = 3, GroupId = 2, SubjectId = 2 }, // Group 2 studies Subject 2
                new GroupSubject { Id = 4, GroupId = 3, SubjectId = 3 }  // Group 3 studies Subject 3
            );
            SaveChanges();

            // 5. Seed Fresh Students Data
            Students.AddRange(
                // مجموعة C# (GroupId = 1)
                new Student { FirstName = "أحمد", SecondName = "محمد", ThirdName = "علي", LastName = "الكبسي", AcademicId = "20260101", GroupId = 1, MajorId = 1, AcademicYear = "الأولى" },
                new Student { FirstName = "خالد", SecondName = "إبراهيم", ThirdName = "حسن", LastName = "المريسي", AcademicId = "20260102", GroupId = 1, MajorId = 1, AcademicYear = "الأولى" },
                new Student { FirstName = "سارة", SecondName = "عبدالله", ThirdName = "ناصر", LastName = "العنسي", AcademicId = "20260103", GroupId = 1, MajorId = 1, AcademicYear = "الأولى" },
                new Student { FirstName = "فاطمة", SecondName = "علي", ThirdName = "محمد", LastName = "العماد", AcademicId = "20260104", GroupId = 1, MajorId = 1, AcademicYear = "الأولى" },
                new Student { FirstName = "محمد", SecondName = "قاسم", ThirdName = "أحمد", LastName = "المؤيد", AcademicId = "20260105", GroupId = 1, MajorId = 1, AcademicYear = "الأولى" },

                // مجموعة قواعد البيانات (GroupId = 2)
                new Student { FirstName = "عبدالرحمن", SecondName = "صالح", ThirdName = "مهدي", LastName = "الخطيب", AcademicId = "20260201", GroupId = 2, MajorId = 1, AcademicYear = "الثانية" },
                new Student { FirstName = "يوسف", SecondName = "أمين", ThirdName = "عبدالكريم", LastName = "الرميمة", AcademicId = "20260202", GroupId = 2, MajorId = 1, AcademicYear = "الثانية" },
                new Student { FirstName = "مريم", SecondName = "حسن", ThirdName = "قاسم", LastName = "الشرعي", AcademicId = "20260203", GroupId = 2, MajorId = 1, AcademicYear = "الثانية" },
                new Student { FirstName = "بلال", SecondName = "فؤاد", ThirdName = "محمد", LastName = "الشامي", AcademicId = "20260204", GroupId = 2, MajorId = 1, AcademicYear = "الثانية" },

                // مجموعة MAUI (GroupId = 3)
                new Student { FirstName = "عمر", SecondName = "فاروق", ThirdName = "جميل", LastName = "القفري", AcademicId = "20260301", GroupId = 3, MajorId = 2, AcademicYear = "الثالثة" },
                new Student { FirstName = "زياد", SecondName = "طارق", ThirdName = "عبدالرحمن", LastName = "الزيادي", AcademicId = "20260302", GroupId = 3, MajorId = 2, AcademicYear = "الثالثة" },
                new Student { FirstName = "منى", SecondName = "عبدالكريم", ThirdName = "حسن", LastName = "الحداء", AcademicId = "20260303", GroupId = 3, MajorId = 2, AcademicYear = "الثالثة" }
            );
            SaveChanges();
        }
        catch (Exception)
        {
            // Handle or log exception if needed
        }
    }
}
