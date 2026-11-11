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
                    _ = LoadGroupAndLectureDataAsync();
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

    private string _majorName = string.Empty; // Major identifier or name based on MajorId
    public string MajorName
    {
        get => _majorName;
        set => SetProperty(ref _majorName, value);
    }

    private string _groupName = string.Empty; // Group Name from Group model
    public string GroupName
    {
        get => _groupName;
        set => SetProperty(ref _groupName, value);
    }

    private string _academicLevel = string.Empty; // Academic Year level mapped to Level/Year display
    public string AcademicLevel
    {
        get => _academicLevel;
        set => SetProperty(ref _academicLevel, value);
    }

    private string _day = "السبت";
    public string Day
    {
        get => _day;
        set => SetProperty(ref _day, value);
    }

    private DateTime _lectureDate = DateTime.Now;
    public DateTime LectureDate
    {
        get => _lectureDate;
        set => SetProperty(ref _lectureDate, value);
    }

    private int _totalStudentsCount;
    public int TotalStudentsCount
    {
        get => _totalStudentsCount;
        set { _totalStudentsCount = value; OnPropertyChanged(); }
    }

    public ObservableCollection<StudentAttendanceItem> StudentsList { get; }

    public ICommand SaveAttendanceCommand { get; }
    public ICommand ClosePageCommand { get; }

    public AttendanceLogPageViewModel()
    {
        _dbContext = new AppDbContext();
        StudentsList = new ObservableCollection<StudentAttendanceItem>();

        SaveAttendanceCommand = new Command(async () => await OnSaveAttendanceExecuted());
        ClosePageCommand = new Command(async () => await OnClosePageExecuted());
    }

    // Fetch names and details dynamically using IDs from the database based on the Group model structure
    private async Task LoadGroupAndLectureDataAsync()
    {
        if (GroupId <= 0) return;

        try
        {
            IsBusy = true;

            // 1. Fetch Group details (Name, SubjectName, AcademicYear, MajorId) using GroupId
            var group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == GroupId);
            if (group != null)
            {
                GroupName = group.Name; // جلب اسم المجموعة الحقيقي
                SubjectName = group.SubjectName; // جلب اسم المادة من المجموعة
                AcademicLevel = group.AcademicYear ?? "المستوى غير محدد"; // جلب المستوى أو السنة الدراسية
                
                // يمكنك جلب اسم التخصص باستخدام MajorId إذا كان لديك جدول Majors، أو عرضه كقيمة افتراضية
                MajorName = $"تخصص #{group.MajorId}"; 
            }
            else
            {
                GroupName = "مجموعة غير محددة";
                SubjectName = "المادة غير محددة";
                AcademicLevel = "غير محدد";
                MajorName = "عام";
            }

            // 2. Fetch Lecture details linked to this GroupId (if specific lecture overrides exist)
            var lecture = await _dbContext.Lectures.FirstOrDefaultAsync(l => l.GroupId == GroupId);
            if (lecture != null)
            {
                HallName = lecture.HallName;
                if (!string.IsNullOrEmpty(lecture.SubjectName))
                    SubjectName = lecture.SubjectName; // الاعتماد على المادة الخاصة بالمحاضرة إن وجدت
                Day = lecture.Day;
                LectureDate = lecture.LectureDate;
            }
            else
            {
                HallName = "قاعة غير محددة";
                Day = "السبت";
            }

            // 3. Fetch students belonging strictly to this GroupId
            var studentsFromDb = await _dbContext.Students
                .Where(s => s.GroupId == GroupId)
                .ToListAsync();

            StudentsList.Clear();
            foreach (var student in studentsFromDb)
            {
                StudentsList.Add(new StudentAttendanceItem
                {
                    StudentId = student.Id,
                    FullName = student.FullName,
                    PhoneNumber = student.AcademicId ?? string.Empty,
                    Status = AttendanceStatus.Present
                });
            }

            TotalStudentsCount = StudentsList.Count;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
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
            await Shell.Current.DisplayAlert("Success", "Attendance records saved successfully.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to save records: {ex.Message}", "OK");
        }
    }

    private async Task OnClosePageExecuted()
    {
        await Shell.Current.GoToAsync("..");
    }
}
