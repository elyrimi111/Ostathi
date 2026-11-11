using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ostathi;

public class Group
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty; // Group name (e.g., مجموعة أ / مجموعة ب)

    [Required]
    [MaxLength(150)]
    public string SubjectName { get; set; } = string.Empty; // Course or Subject name (اسم المادة الدراسية)

    public string? Description { get; set; } // Description or notes

    [MaxLength(100)]
    public string? AcademicYear { get; set; } // Academic year level

    // Foreign key for the related Major entity
    public int MajorId { get; set; }

    
}
