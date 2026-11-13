using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

// Receive GroupId, StudentId, and SubjectId parameters automatically via Shell navigation
[QueryProperty(nameof(GroupId), "GroupId")]
[QueryProperty(nameof(StudentId), "StudentId")]
[QueryProperty(nameof(SubjectId), "SubjectId")]
public class StudentAttendanceLogPageViewModel : BaseViewModel
{
    private readonly AppDbContext _dbContext;

    private int _groupId;
    public int GroupId
    {
        get => _groupId;
        set { _groupId = value; OnPropertyChanged(); CheckAndLoad(); }
    }

    private int _studentId;
    public int StudentId
    {
        get => _studentId;
        set { _studentId = value; OnPropertyChanged(); CheckAndLoad(); }
    }

    private int _subjectId;
    public int SubjectId
    {
        get => _subjectId;
        set { _subjectId = value; OnPropertyChanged(); CheckAndLoad(); }
    }

    private string _studentFullName = string.Empty;
    public string StudentFullName
    {
        get => _studentFullName;
        set => SetProperty(ref _studentFullName, value);
    }

    // Lecture Card Contextual Properties
    private string _hallName = "قاعة 1";
    public string HallName { get => _hallName; set => SetProperty(ref _hallName, value); }

    private string _subjectName = "مادة غير محددة";
    public string SubjectName { get => _subjectName; set => SetProperty(ref _subjectName, value); }

    private string _groupName = "مجموعة غير محددة";
    public string GroupName { get => _groupName; set => SetProperty(ref _groupName, value); }

    private string _academicLevel = "المستوى الأول";
    public string AcademicLevel { get => _academicLevel; set => SetProperty(ref _academicLevel, value); }

    private string _day = "السبت";
    public string Day { get => _day; set => SetProperty(ref _day, value); }

    private DateTime _lectureDate = DateTime.Now;
    public DateTime LectureDate { get => _lectureDate; set => SetProperty(ref _lectureDate, value); }

    private string _majorName = "هندسة البرمجيات";
    public string MajorName { get => _majorName; set => SetProperty(ref _majorName, value); }

    // Collection strictly holding the 12 sequential lectures
    public ObservableCollection<StudentLectureAttendanceItem> LecturesList { get; }

    public ICommand SaveAttendanceCommand { get; }
    public ICommand ClosePageCommand { get; }

    // Commands for direct 1-click status selection
    public ICommand SetStatusPresentCommand { get; }
    public ICommand SetStatusAbsentCommand { get; }
    public ICommand SetStatusExcusedCommand { get; }

    public StudentAttendanceLogPageViewModel()
    {
        _dbContext = new AppDbContext();
        LecturesList = new ObservableCollection<StudentLectureAttendanceItem>();

        SaveAttendanceCommand = new Command(async () => await OnSaveExecuted());
        ClosePageCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

        // Initialize direct 1-click status commands
        SetStatusPresentCommand = new Command<StudentLectureAttendanceItem>(item => { if (item != null) item.Status = AttendanceStatus.Present; });
        SetStatusAbsentCommand = new Command<StudentLectureAttendanceItem>(item => { if (item != null) item.Status = AttendanceStatus.Absent; });
        SetStatusExcusedCommand = new Command<StudentLectureAttendanceItem>(item => { if (item != null) item.Status = AttendanceStatus.Excused; });
    }

    private void CheckAndLoad()
    {
        if (GroupId > 0 && StudentId > 0 && SubjectId > 0)
        {
            _ = LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;

            // 1. Fetch Group details
            var group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == GroupId);
            if (group != null)
            {
                GroupName = group.Name;
                AcademicLevel = string.IsNullOrEmpty(group.AcademicYear) ? "المستوى الأول" : group.AcademicYear;
                MajorName = $"تخصص #{group.MajorId}";
            }

            // 2. Fetch Subject Name strictly by SubjectId
            var subject = await _dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == SubjectId);
            string targetSubjectName = subject?.Name ?? "المقرر الدراسي";
            SubjectName = targetSubjectName;

            // 3. Fetch target Student name
            var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.Id == StudentId);
            if (student != null)
            {
                StudentFullName = student.FullName;
            }

            // 4. Fetch existing lectures for this group and exact subject name from database
            var lecturesFromDb = await _dbContext.Lectures
                .Where(l => l.GroupId == GroupId && l.SubjectName == targetSubjectName)
                .OrderBy(l => l.LectureDate)
                .ToListAsync();

            // Fetch existing attendance records for this student
            var existingAttendances = await _dbContext.Attendances
                .Where(a => a.StudentId == StudentId)
                .ToListAsync();

            LecturesList.Clear();

            // 5. Generate exactly 12 sequential lecture cards displaying "محاضرة ١", "محاضرة ٢", etc.
            for (int i = 1; i <= 12; i++)
            {
                var dbLecture = lecturesFromDb.ElementAtOrDefault(i - 1);
                int lectureIdToUse = dbLecture?.Id ?? 0;

                var existingRecord = lectureIdToUse > 0 
                    ? existingAttendances.FirstOrDefault(a => a.LectureId == lectureIdToUse) 
                    : null;

                // تحويل الرقم إلى الأرقام العربية المشرقية بطريقة سليمة وآمنة
                string arabicIndex = GetArabicNumber(i);

                LecturesList.Add(new StudentLectureAttendanceItem
                {
                    LectureId = lectureIdToUse,
                    LectureIndex = i,
                    SubjectName = targetSubjectName,
                    CardTitle = $"محاضرة {arabicIndex}", // عرض "محاضرة ١"، "محاضرة ٢" بدون تكرار اسم المادة
                    HallName = !string.IsNullOrEmpty(dbLecture?.HallName) ? dbLecture.HallName : HallName,
                    Day = !string.IsNullOrEmpty(dbLecture?.Day) ? dbLecture.Day : Day,
                    LectureDate = dbLecture?.LectureDate ?? DateTime.Now.AddDays((i - 1) * 7),
                    Status = existingRecord != null ? existingRecord.Status : AttendanceStatus.Present
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("خطأ", ex.Message, "موافق");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// دالة مساعدة آمنة لتحويل الأرقام إلى أرقام عربية مشرقية بدون أخطاء استنتاج أنواع
    /// </summary>
    private string GetArabicNumber(int number)
    {
        string[] arabicDigits = { "٠", "١", "٢", "٣", "٤", "٥", "٦", "٧", "٨", "٩" };
        string str = number.ToString();
        var sb = new StringBuilder();
        foreach (char c in str)
        {
            if (char.IsDigit(c))
            {
                sb.Append(arabicDigits[c - '0']);
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private async Task OnSaveExecuted()
    {
        try
        {
            foreach (var item in LecturesList)
            {
                // If the lecture doesn't exist in DB yet, create it first to get a valid LectureId
                if (item.LectureId <= 0)
                {
                    var newLecture = new Lecture
                    {
                        GroupId = GroupId,
                        SubjectName = SubjectName,
                        HallName = item.HallName,
                        Day = item.Day,
                        LectureDate = item.LectureDate
                    };
                    await _dbContext.Lectures.AddAsync(newLecture);
                    await _dbContext.SaveChangesAsync();
                    item.LectureId = newLecture.Id;
                }

                // Check if attendance record already exists for this student and lecture
                var attendanceRecord = await _dbContext.Attendances
                    .FirstOrDefaultAsync(a => a.StudentId == StudentId && a.LectureId == item.LectureId);

                if (attendanceRecord != null)
                {
                    attendanceRecord.Status = item.Status;
                    _dbContext.Attendances.Update(attendanceRecord);
                }
                else
                {
                    var newRecord = new Attendance
                    {
                        LectureId = item.LectureId,
                        StudentId = StudentId,
                        Status = item.Status,
                        Date = item.LectureDate
                    };
                    await _dbContext.Attendances.AddAsync(newRecord);
                }
            }

            await _dbContext.SaveChangesAsync();
            await Shell.Current.DisplayAlert("نجاح", "تم حفظ سجل حضور الـ 12 محاضرة للطالب بنجاح.", "موافق");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("خطأ", $"فشل الحفظ: {ex.Message}", "موافق");
        }
    }
}

// Inner view model item representing individual lecture attendance with 3 direct states support
public class StudentLectureAttendanceItem : BaseViewModel
{
    public int LectureId { get; set; }
    public int LectureIndex { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string CardTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public string Day { get; set; } = string.Empty;
    public DateTime LectureDate { get; set; }

    private AttendanceStatus _status = AttendanceStatus.Present;
    public AttendanceStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
}
