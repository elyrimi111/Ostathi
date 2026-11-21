using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;

namespace Ostathi;

// Receive GroupId, StudentId, and SubjectId parameters automatically via Shell navigation
[QueryProperty(nameof(GroupId), "GroupId")]
[QueryProperty(nameof(StudentId), "StudentId")]
[QueryProperty(nameof(SubjectId), "SubjectId")]
public class StudentAttendanceLogPageViewModel : BaseViewModel
{
    private readonly AppDbContext _dbContext;
    private int _currentGroupSubjectId;

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

        SetStatusPresentCommand = new Command<StudentLectureAttendanceItem>(item =>
        {
            if (item != null)
            {
                item.Status = AttendanceStatus.Present;
                item.IsTaken = true;
            }
        });

        SetStatusAbsentCommand = new Command<StudentLectureAttendanceItem>(item =>
        {
            if (item != null)
            {
                item.Status = AttendanceStatus.Absent;
            }
        });

        SetStatusExcusedCommand = new Command<StudentLectureAttendanceItem>(item =>
        {
            if (item != null)
            {
                item.Status = AttendanceStatus.Excused;
            }
        });
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

            // 1. Fetch Group and Major details
            var group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == GroupId);
            if (group != null)
            {
                GroupName = group.Name;
                AcademicLevel = string.IsNullOrEmpty(group.AcademicYear) ? "المستوى الأول" : group.AcademicYear;

                var major = await _dbContext.Majors.FirstOrDefaultAsync(m => m.Id == group.MajorId);
                MajorName = major != null ? major.Name : $"تخصص #{group.MajorId}";
            }

            // 2. Fetch Subject Name strictly by SubjectId
            var subject = await _dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == SubjectId);
            string targetSubjectName = subject?.Name ?? "المقرر الدراسي";
            SubjectName = targetSubjectName;

            // 3. Resolve GroupSubjectId for this GroupId and SubjectId
            var groupSubject = await _dbContext.GroupSubjects
                .FirstOrDefaultAsync(gs => gs.GroupId == GroupId && gs.SubjectId == SubjectId);
            _currentGroupSubjectId = groupSubject?.Id ?? 0;

            // 4. Fetch target Student name
            var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.Id == StudentId);
            if (student != null)
            {
                StudentFullName = student.FullName;
            }

            // 5. Fetch existing lectures for this GroupSubjectId from database
            var lecturesFromDb = _currentGroupSubjectId > 0
                ? await _dbContext.Lectures
                    .Where(l => l.GroupSubjectId.HasValue && l.GroupSubjectId.Value == _currentGroupSubjectId)
                    .OrderBy(l => l.LectureDate)
                    .ThenBy(l => l.StartTime)
                    .ToListAsync()
                : new List<Lecture>();

            var lectureLinks = _currentGroupSubjectId > 0
                ? await _dbContext.GroupSubjectLucters
                    .Where(x => x.GroupSubjectId == _currentGroupSubjectId)
                    .ToListAsync()
                : new List<GroupSubjectLucters>();

            var linksByLectureId = lectureLinks.ToDictionary(x => x.LectureId, x => x);

            // Fetch existing attendance records for this student
            var existingAttendances = await _dbContext.Attendances
                .Where(a => a.StudentId == StudentId)
                .ToListAsync();

            LecturesList.Clear();

            // 6. Generate exactly 12 sequential lecture cards displaying "محاضرة ١", "محاضرة ٢", etc.
            for (int i = 1; i <= 12; i++)
            {
                var dbLecture = lecturesFromDb.ElementAtOrDefault(i - 1);
                int lectureIdToUse = dbLecture?.Id ?? 0;

                var existingRecord = lectureIdToUse > 0
                    ? existingAttendances.FirstOrDefault(a => a.LectureId == lectureIdToUse)
                    : null;

                linksByLectureId.TryGetValue(lectureIdToUse, out var linkRecord);

                var lectureDate = dbLecture?.LectureDate ?? DateTime.Now.AddDays((i - 1) * 7);
                var dayEnum = dbLecture != null
                    ? dbLecture.DayOfWeek
                    : ScheduleService.ToOstathiWeekDay(lectureDate.DayOfWeek);

                string arabicIndex = GetArabicNumber(i);

                LecturesList.Add(new StudentLectureAttendanceItem
                {
                    LectureId = lectureIdToUse,
                    LectureIndex = i,
                    SubjectName = targetSubjectName,
                    CardTitle = $"محاضرة {arabicIndex}",
                    HallName = !string.IsNullOrEmpty(dbLecture?.HallName) ? dbLecture.HallName : HallName,
                    Day = ScheduleManagementViewModel.ToArabicDayName(dayEnum),
                    LectureDate = lectureDate,
                    IsTaken = linkRecord?.IsTaken ?? false,
                    Status = existingRecord != null ? existingRecord.Status : AttendanceStatus.Present
                });
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", ex.Message, "موافق");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

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
            if (_currentGroupSubjectId <= 0)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("تنبيه", "لا توجد مجموعة مادة مرتبطة بهذه المجموعة والمادة.", "موافق");
                }
                return;
            }

            foreach (var item in LecturesList)
            {
                var parsedDay = ScheduleManagementViewModel.ParseArabicDayName(item.Day)
                    ?? ScheduleService.ToOstathiWeekDay(item.LectureDate.DayOfWeek);

                if (item.LectureId <= 0)
                {
                    var newLecture = new Lecture
                    {
                        LectureNumber = item.LectureIndex,
                        GroupSubjectId = _currentGroupSubjectId,
                        HallName = item.HallName,
                        DayOfWeek = parsedDay,
                        LectureDate = item.LectureDate,
                        StartTime = new TimeOnly(8, 0),
                        EndTime = new TimeOnly(9, 30),
                        IsCancelled = false
                    };
                    await _dbContext.Lectures.AddAsync(newLecture);
                    await _dbContext.SaveChangesAsync();
                    item.LectureId = newLecture.Id;
                }
                else
                {
                    var existingLecture = await _dbContext.Lectures.FirstOrDefaultAsync(l => l.Id == item.LectureId);
                    if (existingLecture != null)
                    {
                        existingLecture.DayOfWeek = parsedDay;
                        existingLecture.LectureDate = item.LectureDate;
                        if (!string.IsNullOrWhiteSpace(item.HallName))
                        {
                            existingLecture.HallName = item.HallName;
                        }
                    }
                }

                var linkRecord = await _dbContext.GroupSubjectLucters
                    .FirstOrDefaultAsync(x => x.LectureId == item.LectureId && x.GroupSubjectId == _currentGroupSubjectId);

                if (linkRecord != null)
                {
                    linkRecord.IsTaken = item.IsTaken;
                    linkRecord.LectureNumber = item.LectureIndex;
                }
                else
                {
                    await _dbContext.GroupSubjectLucters.AddAsync(new GroupSubjectLucters
                    {
                        GroupSubjectId = _currentGroupSubjectId,
                        LectureId = item.LectureId,
                        LectureNumber = item.LectureIndex,
                        IsTaken = item.IsTaken
                    });
                }

                var attendanceRecord = await _dbContext.Attendances
                    .FirstOrDefaultAsync(a => a.StudentId == StudentId && a.LectureId == item.LectureId);

                if (attendanceRecord != null)
                {
                    attendanceRecord.Status = item.Status;
                    attendanceRecord.Date = item.LectureDate;
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

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("نجاح", "تم حفظ سجل حضور الـ 12 محاضرة للطالب بنجاح.", "موافق");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"فشل الحفظ: {ex.Message}", "موافق");
            }
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

    private bool _isTaken;
    public bool IsTaken
    {
        get => _isTaken;
        set => SetProperty(ref _isTaken, value);
    }

    private AttendanceStatus _status = AttendanceStatus.Present;
    public AttendanceStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
}
