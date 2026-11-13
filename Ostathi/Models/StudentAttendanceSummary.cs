namespace Ostathi.Models
{
    public class StudentAttendanceSummary
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string AcademicNumber { get; set; } = string.Empty;
        public int AttendedCount { get; set; }
        public int TotalRequiredLectures { get; set; } = 12;
        public int ConductedLecturesCount { get; set; }

        public string AttendanceRatioText => $"{AttendedCount} / {TotalRequiredLectures}";
        public double ProgressValue => TotalRequiredLectures > 0 ? (double)AttendedCount / TotalRequiredLectures : 0.0;
        public bool IsWarning => ConductedLecturesCount > 0 && ((double)AttendedCount / ConductedLecturesCount) < 0.75;
    }
}
