using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;

namespace Ostathi;

// Receive GroupId parameter automatically via Shell navigation
[QueryProperty(nameof(GroupId), "GroupId")]
public class AttendanceLogPageViewModel : BaseViewModel
{
    private readonly AppDbContext _dbContext;
    private int _currentGroupSubjectId;

    private int _groupId;
    public int GroupId
    {
        get => _groupId;
        set
        {
            if (_groupId != value)
            {
                _groupId = value;
                OnPropertyChanged();
                if (_groupId > 0)
                {
                    _ = LoadGroupAndLecturesDataAsync();
                }
            }
        }
    }

    // UI Bound Properties for Names instead of raw IDs
    private string _hallName = string.Empty;
    public string HallName
    {
        get => _hallName;
        set => SetProperty(ref _hallName, value);
    }

    private string _subjectName = string.Empty;
    public string SubjectName
    {
        get => _subjectName;
        set => SetProperty(ref _subjectName, value);
    }

    private string _majorName = string.Empty;
    public string MajorName
    {
        get => _majorName;
        set => SetProperty(ref _majorName, value);
    }

    private string _groupName = string.Empty;
    public string GroupName
    {
        get => _groupName;
        set => SetProperty(ref _groupName, value);
    }

    private string _academicLevel = string.Empty;
    public string AcademicLevel
    {
        get => _academicLevel;
        set => SetProperty(ref _academicLevel, value);
    }

    private int _totalLecturesCount;
    public int TotalLecturesCount
    {
        get => _totalLecturesCount;
        set { _totalLecturesCount = value; OnPropertyChanged(); }
    }

    // Collection of the 12 lectures with 3 status states
    public ObservableCollection<GroupLectureAttendanceItem> LecturesList { get; }

    public ICommand SaveAttendanceCommand { get; }
    public ICommand ClosePageCommand { get; }

    // Direct 1-click status selection commands for each lecture item
    public ICommand SetStatusPresentCommand { get; }
    public ICommand SetStatusAbsentCommand { get; }
    public ICommand SetStatusExcusedCommand { get; }

    public AttendanceLogPageViewModel()
    {
        _dbContext = new AppDbContext();
        LecturesList = new ObservableCollection<GroupLectureAttendanceItem>();

        SaveAttendanceCommand = new Command(async () => await OnSaveAttendanceExecuted());
        ClosePageCommand = new Command(async () => await OnClosePageExecuted());

        // Initialize status commands
        SetStatusPresentCommand = new Command<GroupLectureAttendanceItem>(item =>
        {
            if (item != null)
            {
                item.Status = AttendanceStatus.Present;
                item.IsTaken = true;
            }
        });

        SetStatusAbsentCommand = new Command<GroupLectureAttendanceItem>(item =>
        {
            if (item != null)
            {
                item.Status = AttendanceStatus.Absent;
            }
        });

        SetStatusExcusedCommand = new Command<GroupLectureAttendanceItem>(item =>
        {
            if (item != null)
            {
                item.Status = AttendanceStatus.Excused;
            }
        });
    }

    // Fetch Group details and generate or load the 12 lectures
    private async Task LoadGroupAndLecturesDataAsync()
    {
        if (GroupId <= 0) return;

        try
        {
            IsBusy = true;

            // 1. Fetch Group and Major details
            var group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == GroupId);
            if (group != null)
            {
                GroupName = group.Name;
                AcademicLevel = !string.IsNullOrEmpty(group.AcademicYear) ? group.AcademicYear : "المستوى الأول";

                var major = await _dbContext.Majors.FirstOrDefaultAsync(m => m.Id == group.MajorId);
                MajorName = major != null ? major.Name : $"تخصص #{group.MajorId}";
            }
            else
            {
                GroupName = "مجموعة غير محددة";
                AcademicLevel = "غير محدد";
                MajorName = "عام";
            }

            // 2. Resolve GroupSubjectId and SubjectName for this Group
            var groupSubjectInfo = await (
                from gs in _dbContext.GroupSubjects
                join s in _dbContext.Subjects on gs.SubjectId equals s.Id
                where gs.GroupId == GroupId
                select new { gs.Id, SubjectName = s.Name }
            ).FirstOrDefaultAsync();

            _currentGroupSubjectId = groupSubjectInfo?.Id ?? 0;
            SubjectName = groupSubjectInfo?.SubjectName ?? "المادة غير محددة";

            // 3. Fetch existing lectures linked to this GroupSubjectId from database
            var lecturesFromDb = _currentGroupSubjectId > 0
                ? await _dbContext.Lectures
                    .Where(l => l.GroupSubjectId == _currentGroupSubjectId)
                    .OrderBy(l => l.LectureDate)
                    .ToListAsync()
                : new List<Lecture>();

            LecturesList.Clear();

            // 4. Ensure exactly 12 lectures are displayed (either from DB or generated as placeholders)
            for (int i = 1; i <= 12; i++)
            {
                var dbLecture = lecturesFromDb.ElementAtOrDefault(i - 1);
                int lectureIdToUse = dbLecture?.Id ?? (2000 + i);
                var lectureDate = dbLecture?.LectureDate ?? DateTime.Today.AddDays((i - 1) * 7);
                var dayEnum = dbLecture != null
                    ? dbLecture.DayOfWeek
                    : ScheduleService.ToOstathiWeekDay(lectureDate.DayOfWeek);

                LecturesList.Add(new GroupLectureAttendanceItem
                {
                    LectureId = lectureIdToUse,
                    LectureTitle = $"{SubjectName} - المحاضرة {i}",
                    HallName = !string.IsNullOrEmpty(dbLecture?.HallName) ? dbLecture.HallName : "قاعة 1",
                    Day = ScheduleManagementViewModel.ToArabicDayName(dayEnum),
                    LectureDate = lectureDate,
                    IsTaken = dbLecture != null && !dbLecture.IsCancelled,
                    Status = AttendanceStatus.Present
                });
            }

            TotalLecturesCount = LecturesList.Count;
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"فشل تحميل البيانات: {ex.Message}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnSaveAttendanceExecuted()
    {
        try
        {
            if (_currentGroupSubjectId <= 0)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("تنبيه", "لا توجد مجموعة مادة مرتبطة بهذه المجموعة.", "موافق");
                }
                return;
            }

            int index = 1;
            foreach (var item in LecturesList)
            {
                var parsedDay = ScheduleManagementViewModel.ParseArabicDayName(item.Day)
                    ?? ScheduleService.ToOstathiWeekDay(item.LectureDate.DayOfWeek);

                var lectureInDb = await _dbContext.Lectures.FirstOrDefaultAsync(l => l.Id == item.LectureId);
                if (lectureInDb == null)
                {
                    var newLecture = new Lecture
                    {
                        LectureNumber = index,
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
                    lectureInDb.LectureNumber = index;
                    lectureInDb.GroupSubjectId = _currentGroupSubjectId;
                    lectureInDb.HallName = item.HallName;
                    lectureInDb.DayOfWeek = parsedDay;
                    lectureInDb.LectureDate = item.LectureDate;
                }

                index++;
            }

            await _dbContext.SaveChangesAsync();

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("نجاح", "تم حفظ سجل المحاضرات بنجاح.", "موافق");
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

    private async Task OnClosePageExecuted()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}

// Inner view model item representing each of the 12 lectures with 3 statuses support
public class GroupLectureAttendanceItem : BaseViewModel
{
    public int LectureId { get; set; }
    public string LectureTitle { get; set; } = string.Empty;
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
