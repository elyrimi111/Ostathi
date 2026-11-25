using System.ComponentModel.DataAnnotations;

namespace Ostathi;

public class Group
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? AcademicYear { get; set; }

    // Foreign key for the related Major.
    public int MajorId { get; set; }

    // Navigation property for the related Major.
    public Major? Major { get; set; }

    // Foreign key for the group representative.
    public int? RepresentativeStudentId { get; set; }

    // Navigation property for the representative Student.
    public Student? RepresentativeStudent { get; set; }
}