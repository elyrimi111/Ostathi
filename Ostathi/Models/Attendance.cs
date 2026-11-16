using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ostathi;

public class Attendance
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Lecture")]
    public int LectureId { get; set; }

    [ForeignKey("Student")]
    public int StudentId { get; set; }

    public DateTime Date { get; set; } = DateTime.Now.Date;

    public TimeSpan Time { get; set; }

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    public int GroupSubjectId { get; set; }

}
