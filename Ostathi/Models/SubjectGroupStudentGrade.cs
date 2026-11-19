using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ostathi;

public class SubjectGroupStudentGrade
{
    [Key]
    public int Id { get; set; }

    // Student ID (Foreign Key)
    [Required]
    public int StudentId { get; set; }

    // Group Subject ID (Foreign Key)
    [Required]
    public int GroupSubjectId { get; set; }

    // Grade breakdown
    public double Attendance { get; set; } = 0;   // Attendance grade
    public double Assignments { get; set; } = 0;  // Assignments grades
    public double Midterm { get; set; } = 0;      // Midterm exam grade
    public double Final { get; set; } = 0;        // Final exam grade

    // Total score (calculated automatically and not stored as a database column)
    [NotMapped]
    public double Total => Attendance + Assignments + Midterm + Final;
}
