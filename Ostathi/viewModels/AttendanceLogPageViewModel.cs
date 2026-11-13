using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

// Receive GroupId parameter automatically via Shell navigation
[QueryProperty(nameof(GroupId), "GroupId")]
public class AttendanceLogPageViewModel : BaseViewModel
{
    private readonly AppDbContext _dbContext;

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
        SetStatusPresentCommand = new Command<GroupLectureAttendanceItem>(item => { if (item != null) item.Status = AttendanceStatus.Present; });
        SetStatusAbsentCommand = new Command<GroupLectureAttendanceItem>(item => { if (item != null) item.Status = AttendanceStatus.Absent; });
        SetStatusExcusedCommand = new Command<GroupLectureAttendanceItem>(item => { if (item != null) item.Status = AttendanceStatus.Excused; });
    }

    // Fetch Group details and generate or load the 12 lectures
    private async Task LoadGroupAndLecturesDataAsync()
    {
        if (GroupId <= 0) return;

        try
        {
            IsBusy = true;

            // 1. Fetch Group details
            var group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == GroupId);
            if (group != null)
            {
                GroupName = group.Name;
                SubjectName = !string.IsNullOrEmpty(group.SubjectName) ? group.SubjectName : "برمجة تطبيقات";
                AcademicLevel = group.AcademicYear ?? "المستوى الأول";
                MajorName = $"تخصص #{group.MajorId}";
            }
            else
            {
                GroupName = "مجموعة غير محددة";
                SubjectName = "المادة غير محددة";
                AcademicLevel = "غير محدد";
                MajorName = "عام";
            }

            // 2. Fetch existing lectures linked to this GroupId from database
            var lecturesFromDb = await _dbContext.Lectures
                .Where(l => l.GroupId == GroupId)
                .OrderBy(l => l.LectureDate)
                .ToListAsync();

            LecturesList.Clear();

            // 3. Ensure exactly 12 lectures are displayed (either from DB or generated as placeholders)
            for (int i = 1; i <= 12; i++)
            {
                var dbLecture = lecturesFromDb.ElementAtOrDefault(i - 1);
                int lectureIdToUse = dbLecture?.Id ?? (2000 + i); // Fallback ID if not yet saved in DB

                LecturesList.Add(new GroupLectureAttendanceItem
                {
                    LectureId = lectureIdToUse,
                    LectureTitle = $"{SubjectName} - المحاضرة {i}",
                    HallName = !string.IsNullOrEmpty(dbLecture?.HallName) ? dbLecture.HallName : "قاعة 1",
                    Day = !string.IsNullOrEmpty(dbLecture?.Day) ? dbLecture.Day : "السبت",
                    LectureDate = dbLecture?.LectureDate ?? DateTime.Now.AddDays((i - 1) * 7),
                    Status = AttendanceStatus.Present // الحالة الافتراضية
                });
            }

            TotalLecturesCount = LecturesList.Count;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("خطأ", $"فشل تحميل البيانات: {ex.Message}", "موافق");
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
            foreach (var item in LecturesList)
            {
                // Ensure lecture exists in DB
                var lectureInDb = await _dbContext.Lectures.FirstOrDefaultAsync(l => l.Id == item.LectureId);
                if (lectureInDb == null)
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
            }

            await _dbContext.SaveChangesAsync();
            await Shell.Current.DisplayAlert("نجاح", "تم حفظ سجل المحاضرات بنجاح.", "موافق");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("خطأ", $"فشل الحفظ: {ex.Message}", "موافق");
        }
    }

    private async Task OnClosePageExecuted()
    {
        await Shell.Current.GoToAsync("..");
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

    private AttendanceStatus _status = AttendanceStatus.Present;
    public AttendanceStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
}
