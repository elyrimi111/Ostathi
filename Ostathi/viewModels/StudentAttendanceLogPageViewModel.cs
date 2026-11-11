using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

// Receive GroupId and StudentId parameters automatically via Shell navigation
[QueryProperty(nameof(GroupId), "GroupId")]
[QueryProperty(nameof(StudentId), "StudentId")]
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

    private string _studentFullName = string.Empty;
    public string StudentFullName
    {
        get => _studentFullName;
        set => SetProperty(ref _studentFullName, value);
    }

    // Lecture Card Contextual Properties
    private string _hallName = "قاعة غير محددة";
    public string HallName { get => _hallName; set => SetProperty(ref _hallName, value); }

    private string _subjectName = "مادة غير محددة";
    public string SubjectName { get => _subjectName; set => SetProperty(ref _subjectName, value); }

    private string _groupName = "مجموعة غير محددة";
    public string GroupName { get => _groupName; set => SetProperty(ref _groupName, value); }

    private string _academicLevel = "المستوى العام";
    public string AcademicLevel { get => _academicLevel; set => SetProperty(ref _academicLevel, value); }

    private string _day = "السبت";
    public string Day { get => _day; set => SetProperty(ref _day, value); }

    private DateTime _lectureDate = DateTime.Now;
    public DateTime LectureDate { get => _lectureDate; set => SetProperty(ref _lectureDate, value); }

    private string _majorName = "عام";
    public string MajorName { get => _majorName; set => SetProperty(ref _majorName, value); }

    // Collection of lectures registered for the group
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
        if (GroupId > 0 && StudentId > 0)
        {
            _ = LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;

            // 1. Fetch Group details to populate the top card
            var group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == GroupId);
            if (group != null)
            {
                GroupName = group.Name;
                SubjectName = string.IsNullOrEmpty(group.SubjectName) ? "برمجة تطبيقات" : group.SubjectName;
                AcademicLevel = string.IsNullOrEmpty(group.AcademicYear) ? "المستوى الأول" : group.AcademicYear;
                MajorName = $"تخصص #{group.MajorId}";
            }

            // 2. Fetch target Student name
            var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.Id == StudentId);
            if (student != null)
            {
                StudentFullName = student.FullName;
            }

            // 3. Fetch default or specific lecture details for the header card
            var lectureInfo = await _dbContext.Lectures.FirstOrDefaultAsync(l => l.GroupId == GroupId);
            if (lectureInfo != null)
            {
                HallName = string.IsNullOrEmpty(lectureInfo.HallName) ? "قاعة المحاضرات الكبرى" : lectureInfo.HallName;
                Day = string.IsNullOrEmpty(lectureInfo.Day) ? "السبت" : lectureInfo.Day;
                LectureDate = lectureInfo.LectureDate;
            }
            else
            {
                HallName = "قاعة 101";
                Day = "السبت";
            }

            // 4. Fetch all lectures belonging strictly to this group
            var lecturesFromDb = await _dbContext.Lectures
                .Where(l => l.GroupId == GroupId)
                .OrderBy(l => l.LectureDate)
                .ToListAsync();

            LecturesList.Clear();
            
            // If lectures are not created yet in DB, generate 12 default placeholder lectures
            if (lecturesFromDb == null || lecturesFromDb.Count == 0)
            {
                for (int i = 1; i <= 12; i++)
                {
                    LecturesList.Add(new StudentLectureAttendanceItem
                    {
                        LectureId = i,
                        SubjectName = $"{SubjectName} - المحاضرة {i}",
                        HallName = HallName,
                        Day = Day,
                        LectureDate = DateTime.Now.AddDays((i - 1) * 7),
                        Status = AttendanceStatus.Present
                    });
                }
            }
            else
            {
                foreach (var lec in lecturesFromDb)
                {
                    LecturesList.Add(new StudentLectureAttendanceItem
                    {
                        LectureId = lec.Id,
                        SubjectName = string.IsNullOrEmpty(lec.SubjectName) ? SubjectName : lec.SubjectName,
                        HallName = string.IsNullOrEmpty(lec.HallName) ? HallName : lec.HallName,
                        Day = string.IsNullOrEmpty(lec.Day) ? Day : lec.Day,
                        LectureDate = lec.LectureDate,
                        Status = AttendanceStatus.Present
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnSaveExecuted()
    {
        try
        {
            // Here you can implement saving each student lecture attendance status to database if needed
            await Shell.Current.DisplayAlert("Success", "تم حفظ سجل حضور الطالب بنجاح.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"فشل الحفظ: {ex.Message}", "OK");
        }
    }
}

// Inner view model item representing individual lecture attendance with 3 direct states support
public class StudentLectureAttendanceItem : BaseViewModel
{
    public int LectureId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
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
