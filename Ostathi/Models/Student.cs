using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ostathi;

public class Student
{
    // Primary key for the student entity
    [Key]
    public int Id { get; set; }
    
    // Student's first name
    public string FirstName { get; set; } = string.Empty;
    
    // Student's second name
    public string SecondName { get; set; } = string.Empty;
    
    // Student's third name
    public string ThirdName { get; set; } = string.Empty;
    
    // Student's last name
    public string LastName { get; set; } = string.Empty;
    
    // Unique academic identifier or student ID number
    public string AcademicId { get; set; } = string.Empty;
    
    // Foreign key linking the student to the Major table (ID only)
    public int MajorId { get; set; }

    // Foreign key representing the group ID the student belongs to
    public int GroupId { get; set; }

    // The academic year or study level of the student
    public string AcademicYear { get; set; } = string.Empty;

    // متغير/خاصية لجمع وتنسيق الأسماء الأربعة لعرضها مباشرة في الواجهة
    [NotMapped]
    public string FullName => $"{FirstName} {SecondName} {ThirdName} {LastName}".Replace("  ", " ").Trim();
}
