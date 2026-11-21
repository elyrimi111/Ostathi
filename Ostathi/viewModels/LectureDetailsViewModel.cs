using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi;

[QueryProperty(nameof(LectureId), nameof(LectureId))]
public class LectureDetailsViewModel : BaseViewModel
{
    private readonly LucterService _lecturesService;
    private readonly AppDbContext _context;
    private Lecture? _currentLecture;

    private int _lectureId;
    public int LectureId
    {
        get => _lectureId;
        set
        {
            _lectureId = value;
            OnPropertyChanged();
            _ = LoadLectureDetailsAsync();
        }
    }

    private int _groupSubjectId;
    public int GroupSubjectId
    {
        get => _groupSubjectId;
        set => SetProperty(ref _groupSubjectId, value);
    }

    private string _subjectName = string.Empty;
    public string SubjectName
    {
        get => _subjectName;
        set => SetProperty(ref _subjectName, value);
    }

    private string _hallName = string.Empty;
    public string HallName
    {
        get => _hallName;
        set => SetProperty(ref _hallName, value);
    }

    // يتم تحديد اليوم تلقائياً ولا يُعدّل بواسطة المستخدم
    private string _day = GetArabicDayName(DateTime.Today);
    public string Day
    {
        get => _day;
        private set => SetProperty(ref _day, value);
    }

    // يتم تحديد التاريخ تلقائياً ولا يُعدّل بواسطة المستخدم
    private DateTime _lectureDate = DateTime.Today;
    public DateTime LectureDate
    {
        get => _lectureDate;
        private set => SetProperty(ref _lectureDate, value);
    }

    private bool _isTaken;
    public bool IsTaken
    {
        get => _isTaken;
        set => SetProperty(ref _isTaken, value);
    }

    public ObservableCollection<StudentAttendanceItem> StudentsAttendanceList { get; } = new();

    public ICommand SaveCommand { get; }
    public ICommand SetStudentPresentCommand { get; }
    public ICommand SetStudentAbsentCommand { get; }
    public ICommand SetStudentExcusedCommand { get; }

    public LectureDetailsViewModel(LucterService lecturesService, AppDbContext context)
    {
        _lecturesService = lecturesService;
        _context = context;

        ApplyCurrentDayAndDate();

        SaveCommand = new Command(async () => await SaveChangesAsync());
        SetStudentPresentCommand = new Command<StudentAttendanceItem>(item => SetStudentStatus(item, AttendanceStatus.Present));
        SetStudentAbsentCommand = new Command<StudentAttendanceItem>(item => SetStudentStatus(item, AttendanceStatus.Absent));
        SetStudentExcusedCommand = new Command<StudentAttendanceItem>(item => SetStudentStatus(item, AttendanceStatus.Excused));
    }

    private static string GetArabicDayName(DateTime date) =>
        ScheduleManagementViewModel.ToArabicDayName(ScheduleService.ToOstathiWeekDay(date.DayOfWeek));

    private void ApplyCurrentDayAndDate()
    {
        DateTime today = DateTime.Today;
        LectureDate = today;
        Day = GetArabicDayName(today);
    }

    private void SetStudentStatus(StudentAttendanceItem? item, AttendanceStatus status)
    {
        if (item == null) return;
        item.Status = status;
    }

    private async Task LoadLectureDetailsAsync()
    {
        if (LectureId <= 0) return;

        IsBusy = true;
        try
        {
            ApplyCurrentDayAndDate();

            var requestedLecture = await _lecturesService.GetLectureByIdAsync(LectureId);
            if (requestedLecture == null) return;

            var groupSubjectLink = await _context.GroupSubjectLucters
                .FirstOrDefaultAsync(x => x.LectureId == requestedLecture.Id);

            // إذا كانت المحاضرة مسجلة كمأخوذة مسبقاً، انتقل تلقائياً لأقرب محاضرة غير مأخوذة لنفس مجموعة المادة
            if (groupSubjectLink?.IsTaken == true && requestedLecture.GroupSubjectId.HasValue)
            {
                var takenLectureIds = await _context.GroupSubjectLucters
                    .Where(x => x.GroupSubjectId == requestedLecture.GroupSubjectId.Value && x.IsTaken)
                    .Select(x => x.LectureId)
                    .ToListAsync();

                var nextUnTakenLecture = await _context.Lectures
                    .Where(l => l.GroupSubjectId == requestedLecture.GroupSubjectId.Value &&
                                !l.IsCancelled &&
                                !takenLectureIds.Contains(l.Id))
                    .OrderBy(l => l.LectureDate)
                    .ThenBy(l => l.StartTime)
                    .ThenBy(l => l.Id)
                    .FirstOrDefaultAsync();

                if (nextUnTakenLecture == null)
                {
                    if (Shell.Current != null)
                    {
                        await Shell.Current.DisplayAlert("تنبيه", "تم أخذ جميع محاضرات هذا المقرر مسبقاً.", "موافق");
                        await Shell.Current.GoToAsync("..");
                    }
                    return;
                }

                requestedLecture = nextUnTakenLecture;
                _lectureId = requestedLecture.Id;
                OnPropertyChanged(nameof(LectureId));

                groupSubjectLink = await _context.GroupSubjectLucters
                    .FirstOrDefaultAsync(x => x.LectureId == requestedLecture.Id);
            }

            _currentLecture = requestedLecture;
            GroupSubjectId = _currentLecture.GroupSubjectId ?? 0;
            HallName = _currentLecture.HallName;
            IsTaken = groupSubjectLink?.IsTaken ?? false;

            // إسناد اليوم والتاريخ الحاليين تلقائياً
            _currentLecture.DayOfWeek = ScheduleService.ToOstathiWeekDay(LectureDate.DayOfWeek);
            _currentLecture.LectureDate = LectureDate;

            if (_currentLecture.GroupSubjectId.HasValue)
            {
                var groupSubject = await _context.GroupSubjects
                    .FirstOrDefaultAsync(gs => gs.Id == _currentLecture.GroupSubjectId.Value);

                if (groupSubject != null)
                {
                    var subject = await _context.Subjects
                        .FirstOrDefaultAsync(s => s.Id == groupSubject.SubjectId);
                    SubjectName = subject?.Name ?? string.Empty;

                    await LoadGroupStudentsAttendanceAsync(groupSubject.GroupId, _currentLecture.Id);
                }
                else
                {
                    SubjectName = string.Empty;
                    MainThread.BeginInvokeOnMainThread(() => StudentsAttendanceList.Clear());
                }
            }
            else
            {
                SubjectName = string.Empty;
                MainThread.BeginInvokeOnMainThread(() => StudentsAttendanceList.Clear());
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"فشل في تحميل بيانات المحاضرة: {ex.Message}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadGroupStudentsAttendanceAsync(int groupId, int lectureId)
    {
        var students = await _context.Students
            .Where(s => s.GroupId == groupId)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.SecondName)
            .ToListAsync();

        var existingRecords = await _context.Attendances
            .Where(a => a.LectureId == lectureId)
            .ToListAsync();

        var recordsByStudentId = existingRecords.ToDictionary(a => a.StudentId, a => a);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StudentsAttendanceList.Clear();

            foreach (var student in students)
            {
                recordsByStudentId.TryGetValue(student.Id, out var attendanceRecord);

                string displayName = string.IsNullOrWhiteSpace(student.FullName)
                    ? $"طالب #{student.Id}"
                    : student.FullName;

                StudentsAttendanceList.Add(new StudentAttendanceItem
                {
                    StudentId = student.Id,
                    FullName = displayName,
                    PhoneNumber = student.AcademicId ?? string.Empty,
                    Status = attendanceRecord?.Status ?? AttendanceStatus.Present
                });
            }
        });
    }

    private async Task SaveChangesAsync()
    {
        if (_currentLecture == null) return;

        try
        {
            IsBusy = true;

            // تحديث اليوم والتاريخ تلقائياً لحظة الحفظ واعتماد المحاضرة كمأخوذة
            ApplyCurrentDayAndDate();
            IsTaken = true;

            _currentLecture.GroupSubjectId = GroupSubjectId > 0 ? GroupSubjectId : null;
            _currentLecture.HallName = HallName;
            _currentLecture.DayOfWeek = ScheduleService.ToOstathiWeekDay(LectureDate.DayOfWeek);
            _currentLecture.LectureDate = LectureDate;

            await _lecturesService.UpdateLectureAsync(_currentLecture);

            if (_currentLecture.GroupSubjectId.HasValue)
            {
                var link = await _context.GroupSubjectLucters
                    .FirstOrDefaultAsync(x => x.LectureId == _currentLecture.Id);

                if (link != null)
                {
                    link.IsTaken = true;
                }
                else
                {
                    await _context.GroupSubjectLucters.AddAsync(new GroupSubjectLucters
                    {
                        GroupSubjectId = _currentLecture.GroupSubjectId.Value,
                        LectureId = _currentLecture.Id,
                        LectureNumber = _currentLecture.LectureNumber > 0 ? _currentLecture.LectureNumber : 1,
                        IsTaken = true
                    });
                }
            }

            var existingRecords = await _context.Attendances
                .Where(a => a.LectureId == _currentLecture.Id)
                .ToListAsync();

            var existingByStudentId = existingRecords.ToDictionary(a => a.StudentId, a => a);

            foreach (var item in StudentsAttendanceList)
            {
                if (existingByStudentId.TryGetValue(item.StudentId, out var existingRecord))
                {
                    existingRecord.Status = item.Status;
                    existingRecord.Date = LectureDate;
                }
                else
                {
                    await _context.Attendances.AddAsync(new Attendance
                    {
                        StudentId = item.StudentId,
                        LectureId = _currentLecture.Id,
                        Status = item.Status,
                        Date = LectureDate
                    });
                }
            }

            await _context.SaveChangesAsync();
            MessengerService.NotifyLectureAdded();

            // البحث عن المحاضرة التالية غير المأخوذة لنفس مجموعة المادة والانتقال إليها
            if (GroupSubjectId > 0)
            {
                var takenLectureIds = await _context.GroupSubjectLucters
                    .Where(x => x.GroupSubjectId == GroupSubjectId && x.IsTaken)
                    .Select(x => x.LectureId)
                    .ToListAsync();

                var nextLecture = await _context.Lectures
                    .Where(l => l.GroupSubjectId == GroupSubjectId &&
                                !l.IsCancelled &&
                                l.Id != _currentLecture.Id &&
                                !takenLectureIds.Contains(l.Id))
                    .OrderBy(l => l.LectureDate)
                    .ThenBy(l => l.StartTime)
                    .ThenBy(l => l.Id)
                    .FirstOrDefaultAsync();

                if (nextLecture != null)
                {
                    if (Shell.Current != null)
                    {
                        await Shell.Current.DisplayAlert("تم الحفظ", "تم حفظ التحضير بنجاح والانتقال إلى المحاضرة القادمة.", "موافق");
                    }

                    IsBusy = false;
                    LectureId = nextLecture.Id;
                    return;
                }
            }

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("اكتمل التحضير", "تم حفظ التحضير بنجاح واكتملت جميع محاضرات هذا المقرر.", "موافق");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            var err = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"فشل في حفظ التعديلات: {err}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
