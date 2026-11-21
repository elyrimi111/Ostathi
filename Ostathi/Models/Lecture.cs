using System;

namespace Ostathi;

public class Lecture
{
    public int Id { get; set; }
    public int LectureNumber { get; set; }

    public int? GroupSubjectId { get; set; }
    public WeekDay DayOfWeek { get; set; }

    public string HallName { get; set; } = string.Empty;

    public DateTime LectureDate { get; set; } = DateTime.Today;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public bool IsCancelled { get; set; }
    public string Notes { get; set; } = string.Empty;

    public DateTime StartDateTime => LectureDate.Date.Add(StartTime.ToTimeSpan());
    public DateTime EndDateTime => LectureDate.Date.Add(EndTime.ToTimeSpan());
}
