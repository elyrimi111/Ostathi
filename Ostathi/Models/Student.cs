using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ostathi;

public class Student
{
// Primary key.
[Key]
public int Id { get; set; }

// Student's first name.
public string FirstName { get; set; } = string.Empty;

// Student's second name.
public string SecondName { get; set; } = string.Empty;

// Student's third name.
public string ThirdName { get; set; } = string.Empty;

// Student's last name.
public string LastName { get; set; } = string.Empty;

// Unique academic identifier.
public string AcademicId { get; set; } = string.Empty;

// Foreign key referencing the student's Major.
public int MajorId { get; set; }

// Navigation property referencing the student's Major.
public Major Major { get; set; } = null!;

// Foreign key referencing the student's Group.
public int GroupId { get; set; }

// Navigation property referencing the student's Group.
public Group Group { get; set; } = null!;

// Student's academic year.
public string AcademicYear { get; set; } = string.Empty;

// Full name generated from the four name parts.
[NotMapped]
public string FullName =>
    $"{FirstName} {SecondName} {ThirdName} {LastName}"
    .Replace("  ", " ")
    .Trim();

}