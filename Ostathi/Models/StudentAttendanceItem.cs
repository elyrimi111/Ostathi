using Microsoft.Maui.Controls;

namespace Ostathi;

public class StudentAttendanceItem : BindableObject
{
    // Unique identifier for the student
    public int StudentId { get; set; }

    // Student's full name
    public string FullName { get; set; } = string.Empty;

    // Student's contact phone number
    public string PhoneNumber { get; set; } = string.Empty;

    // Attendance status (Present, Absent, Excused)
    private AttendanceStatus _status = AttendanceStatus.Present; // Default to Present
    public AttendanceStatus Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged();
            }
        }
    }
}
