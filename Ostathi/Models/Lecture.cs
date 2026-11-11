using System;
using System.ComponentModel.DataAnnotations;

namespace Ostathi;

public class Lecture
{
    // Primary Key for the Lecture entity
    [Key]
    public int Id { get; set; }

    // Hall or Room Name (e.g., "قاعة 1", "مدرج أ")
    [MaxLength(100)]
    public string HallName { get; set; } = string.Empty;

    // Subject Name (e.g., "OOP", "Database Management")
    [MaxLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    // Major ID or Specialization identifier
    [MaxLength(100)]
    public string MajorId { get; set; } = string.Empty;

    // Foreign key referencing the related Group entity
    public int GroupId { get; set; }

    // Day of the week (e.g., "Saturday", "Sunday")
    [Required]
    [MaxLength(50)]
    public string Day { get; set; } = "السبت";

    // Lecture Date for tracking attendance history
    public DateTime LectureDate { get; set; } = DateTime.Now;
}
