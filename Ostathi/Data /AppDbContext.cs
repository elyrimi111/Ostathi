using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public class AppDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Major> Majors => Set<Major>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<GroupSubject> GroupSubjects => Set<GroupSubject>();
    public DbSet<MajorSubject> MajorSubjects => Set<MajorSubject>();
    public DbSet<Lecture> Lectures => Set<Lecture>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    
    // جدول درجات مجموعة المادة للطلاب
    public DbSet<SubjectGroupStudentGrade> SubjectGroupStudentGrades => Set<SubjectGroupStudentGrade>();

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

        // (تمت إزالة إعداد RepresentativeStudent لعدم وجود خاصية تنقل)

        // Configure Many-to-Many relationship configuration for GroupSubject
        modelBuilder.Entity<GroupSubject>()
            .HasOne(gs => gs.Group)
            .WithMany()
            .HasForeignKey(gs => gs.GroupId);

        modelBuilder.Entity<GroupSubject>()
            .HasOne(gs => gs.Subject)
            .WithMany()
            .HasForeignKey(gs => gs.SubjectId);

        // Configure MajorSubject relationship and unique index (Foreign Keys only)
        modelBuilder.Entity<MajorSubject>()
            .HasIndex(ms => new { ms.MajorId, ms.SubjectId })
            .IsUnique();

        modelBuilder.Entity<MajorSubject>()
            .HasOne<Major>()
            .WithMany()
            .HasForeignKey(ms => ms.MajorId);

        modelBuilder.Entity<MajorSubject>()
            .HasOne<Subject>()
            .WithMany()
            .HasForeignKey(ms => ms.SubjectId);
    }


    // Method to insert fresh initial data on every startup
    private void SeedDatabase()
    {
        try
        {
            // 1. Seed Comprehensive University Majors (Computer Science, IT, Engineering, Admin)
            Majors.AddRange(
                new Major { Id = 1, Name = "هندسة البرمجيات" },
                new Major { Id = 2, Name = "علوم الحاسوب" },
                new Major { Id = 3, Name = "نظم المعلومات الحاسوبية (CIS)" },
                new Major { Id = 4, Name = "تكنولوجيا المعلومات (IT)" },
                new Major { Id = 5, Name = "الأمن السيبراني والشبكات" },
                new Major { Id = 6, Name = "الذكاء الاصطناعي وعلم البيانات" },
                new Major { Id = 7, Name = "هندسة الحاسوب والتحكم" },
                new Major { Id = 8, Name = "نظم المعلومات الإدارية (MIS)" },
                new Major { Id = 9, Name = "إدارة الأعمال" },
                new Major { Id = 10, Name = "المحاسبة والتمويل" }
            );
            SaveChanges();

            // 2. Seed Fresh Students Data (قبل المجموعات لكي نتمكن من ربط المندوب بالطلاب مباشرة)
            Students.AddRange(
                // Group A (GroupId = 1)
                new Student { Id = 1, FirstName = "أحمد", SecondName = "محمد", ThirdName = "علي", LastName = "الكبسي", AcademicId = "777123456", GroupId = 1, MajorId = 1, AcademicYear = "الأولى" },
                new Student { Id = 2, FirstName = "خالد", SecondName = "إبراهيم", ThirdName = "حسن", LastName = "المريسي", AcademicId = "777234567", GroupId = 1, MajorId = 1, AcademicYear = "الأولى" },
                new Student { Id = 3, FirstName = "سارة", SecondName = "عبدالله", ThirdName = "ناصر", LastName = "العنسي", AcademicId = "777345678", GroupId = 1, MajorId = 1, AcademicYear = "الأولى" },
                new Student { Id = 4, FirstName = "فاطمة", SecondName = "علي", ThirdName = "محمد", LastName = "العماد", AcademicId = "777456789", GroupId = 1, MajorId = 1, AcademicYear = "الأولى" },
                new Student { Id = 5, FirstName = "محمد", SecondName = "قاسم", ThirdName = "أحمد", LastName = "المؤيد", AcademicId = "777567890", GroupId = 1, MajorId = 1, AcademicYear = "الأولى" },

                // Group B (GroupId = 2)
                new Student { Id = 6, FirstName = "عبدالرحمن", SecondName = "صالح", ThirdName = "مهدي", LastName = "الخطيب", AcademicId = "778111222", GroupId = 2, MajorId = 1, AcademicYear = "الثانية" },
                new Student { Id = 7, FirstName = "يوسف", SecondName = "أمين", ThirdName = "عبدالكريم", LastName = "الرميمة", AcademicId = "778222333", GroupId = 2, MajorId = 1, AcademicYear = "الثانية" },
                new Student { Id = 8, FirstName = "مريم", SecondName = "حسن", ThirdName = "قاسم", LastName = "الشرعي", AcademicId = "778333444", GroupId = 2, MajorId = 1, AcademicYear = "الثانية" },
                new Student { Id = 9, FirstName = "بلال", SecondName = "فؤاد", ThirdName = "محمد", LastName = "الشامي", AcademicId = "778444555", GroupId = 2, MajorId = 1, AcademicYear = "الثانية" },

                // Group C (GroupId = 3)
                new Student { Id = 10, FirstName = "عمر", SecondName = "فاروق", ThirdName = "جميل", LastName = "القفري", AcademicId = "779111000", GroupId = 3, MajorId = 2, AcademicYear = "الثالثة" },
                new Student { Id = 11, FirstName = "زياد", SecondName = "طارق", ThirdName = "عبدالرحمن", LastName = "الزيادي", AcademicId = "779222000", GroupId = 3, MajorId = 2, AcademicYear = "الثالثة" },
                new Student { Id = 12, FirstName = "منى", SecondName = "عبدالكريم", ThirdName = "حسن", LastName = "الحداء", AcademicId = "779333000", GroupId = 3, MajorId = 2, AcademicYear = "الثالثة" }
            );
            SaveChanges();

            // 3. Seed Groups (مع تعيين طالب كـ مندوب لكل مجموعة افتراضياً للاختبار)
            Groups.AddRange(
                new Group { Id = 1, Name = "مجموعة أ (هندسة برمجيات)", MajorId = 1, AcademicYear = "المستوى الأول", RepresentativeStudentId = 1 },
                new Group { Id = 2, Name = "مجموعة ب (هندسة برمجيات)", MajorId = 1, AcademicYear = "المستوى الثاني", RepresentativeStudentId = 6 },
                new Group { Id = 3, Name = "مجموعة ج (علوم حاسوب)", MajorId = 2, AcademicYear = "المستوى الثالث", RepresentativeStudentId = 10 },
                new Group { Id = 4, Name = "مجموعة د (نظم معلومات حاسوبية)", MajorId = 3, AcademicYear = "المستوى الرابع", RepresentativeStudentId = null }
            );
            SaveChanges();

            // 4. Seed Subjects
            Subjects.AddRange(
                new Subject { Id = 1, Name = "برمجة الحاسوب C#" },
                new Subject { Id = 2, Name = "قواعد البيانات SQLite" },
                new Subject { Id = 3, Name = "تطوير واجهات MAUI" },
                new Subject { Id = 4, Name = "هياكل البيانات والخوارزميات" },
                new Subject { Id = 5, Name = "تحليل وتصميم النظم" },
                new Subject { Id = 6, Name = "شبكات الحاسوب والأمن" }
            );
            SaveChanges();

            // 5. Seed GroupSubjects (Assigning subjects to groups)
            GroupSubjects.AddRange(
                new GroupSubject { Id = 1, GroupId = 1, SubjectId = 1 }, 
                new GroupSubject { Id = 2, GroupId = 1, SubjectId = 2 }, 
                new GroupSubject { Id = 3, GroupId = 2, SubjectId = 4 }, 
                new GroupSubject { Id = 4, GroupId = 3, SubjectId = 5 }  
            );
            SaveChanges();

            // 6. Seed MajorSubjects (Assigning subjects to academic majors / study plans)
            MajorSubjects.AddRange(
                new MajorSubject { Id = 1, MajorId = 1, SubjectId = 1, Level = 1, Semester = 1, IsMandatory = true },
                new MajorSubject { Id = 2, MajorId = 1, SubjectId = 2, Level = 1, Semester = 2, IsMandatory = true },
                new MajorSubject { Id = 3, MajorId = 1, SubjectId = 3, Level = 2, Semester = 1, IsMandatory = true },
                new MajorSubject { Id = 4, MajorId = 2, SubjectId = 4, Level = 1, Semester = 1, IsMandatory = true },
                new MajorSubject { Id = 5, MajorId = 3, SubjectId = 5, Level = 2, Semester = 2, IsMandatory = true }
            );
            SaveChanges();

            // 7. Seed Sample Lectures and Attendance Records for testing attendance matrix
            var sampleLectures = new List<Lecture>
            {
                new Lecture { Id = 1, GroupId = 1, SubjectName = "برمجة الحاسوب C#", HallName = "قاعة 1", Day = "السبت", LectureDate = DateTime.Now.AddDays(-7) },
                new Lecture { Id = 2, GroupId = 1, SubjectName = "برمجة الحاسوب C#", HallName = "قاعة 1", Day = "الإثنين", LectureDate = DateTime.Now.AddDays(-5) },
                new Lecture { Id = 3, GroupId = 1, SubjectName = "برمجة الحاسوب C#", HallName = "قاعة 2", Day = "الأربعاء", LectureDate = DateTime.Now.AddDays(-2) }
            };
            Lectures.AddRange(sampleLectures);
            SaveChanges();

            // Seed sample attendance data linking students to lectures with status
            Attendances.AddRange(
                new Attendance { Id = 1, LectureId = 1, StudentId = 1, Status = AttendanceStatus.Present, Date = DateTime.Now.AddDays(-7) },
                new Attendance { Id = 2, LectureId = 1, StudentId = 2, Status = AttendanceStatus.Absent, Date = DateTime.Now.AddDays(-7) },
                new Attendance { Id = 3, LectureId = 1, StudentId = 3, Status = AttendanceStatus.Excused, Date = DateTime.Now.AddDays(-7) },
                
                new Attendance { Id = 4, LectureId = 2, StudentId = 1, Status = AttendanceStatus.Present, Date = DateTime.Now.AddDays(-5) },
                new Attendance { Id = 5, LectureId = 2, StudentId = 2, Status = AttendanceStatus.Present, Date = DateTime.Now.AddDays(-5) },

                new Attendance { Id = 6, LectureId = 3, StudentId = 1, Status = AttendanceStatus.Excused, Date = DateTime.Now.AddDays(-2) }
            );
            SaveChanges();

            // 8. إضافة تكاليف تجريبية أولية للاختبار
            Assignments.AddRange(
                new Assignment { Id = 1, GroupId = 1, SubjectName = "برمجة الحاسوب C#", Title = "تطبيق شاشة تسجيل الدخول", Description = "تصميم واجهة تسجيل دخول باستخدام MAUI", DueDate = DateTime.Now.AddDays(5) },
                new Assignment { Id = 2, GroupId = 1, SubjectName = "برمجة الحاسوب C#", Title = "مشروع إدارة الطلاب", Description = "ربط التطبيق بقاعدة بيانات SQLite وإجراء عمليات CRUD", DueDate = DateTime.Now.AddDays(12) }
            );
            SaveChanges();

            // 9. إضافة درجات طلاب تجريبية للاختبار
            SubjectGroupStudentGrades.AddRange(
                new SubjectGroupStudentGrade { Id = 1, StudentId = 1, GroupSubjectId = 1, Attendance = 9.5, Assignments = 14, Midterm = 22, Final = 42 },
                new SubjectGroupStudentGrade { Id = 2, StudentId = 2, GroupSubjectId = 1, Attendance = 8, Assignments = 10, Midterm = 19, Final = 38 },
                new SubjectGroupStudentGrade { Id = 3, StudentId = 3, GroupSubjectId = 1, Attendance = 10, Assignments = 15, Midterm = 24, Final = 46 }
            );
            SaveChanges();
        }
        catch (Exception)
        {
            // Handle or log exception if needed
        }
    }
}
