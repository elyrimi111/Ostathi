using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Major> Majors => Set<Major>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<GroupSubject> GroupSubjects => Set<GroupSubject>();
    public DbSet<MajorSubject> MajorSubjects => Set<MajorSubject>();
    public DbSet<Lecture> Lectures => Set<Lecture>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<SubjectGroupStudentGrade> SubjectGroupStudentGrades => Set<SubjectGroupStudentGrade>();
    public DbSet<GroupSubjectLucters> GroupSubjectLucters => Set<GroupSubjectLucters>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(DatabaseConfig.ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
