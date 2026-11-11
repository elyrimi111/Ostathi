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
public class GroupAttendanceManagementPageViewModel : BaseViewModel
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
                    _ = LoadGroupAndStudentsAsync();
                }
            }
        }
    }

    private string _groupName = string.Empty;
    public string GroupName
    {
        get => _groupName;
        set => SetProperty(ref _groupName, value);
    }

    private int _totalStudentsCount;
    public int TotalStudentsCount
    {
        get => _totalStudentsCount;
        set { _totalStudentsCount = value; OnPropertyChanged(); }
    }

    private int _presentCount;
    public int PresentCount
    {
        get => _presentCount;
        set { _presentCount = value; OnPropertyChanged(); }
    }

    private int _absentCount;
    public int AbsentCount
    {
        get => _absentCount;
        set { _absentCount = value; OnPropertyChanged(); }
    }

    private StudentAttendanceItem? _selectedStudent;
    public StudentAttendanceItem? SelectedStudent
    {
        get => _selectedStudent;
        set => SetProperty(ref _selectedStudent, value);
    }

    public ObservableCollection<StudentAttendanceItem> StudentsList { get; }

    public ICommand StudentTappedCommand { get; }
    public ICommand SaveAllCommand { get; }
    public ICommand ClosePageCommand { get; }

    public GroupAttendanceManagementPageViewModel()
    {
        _dbContext = new AppDbContext();
        StudentsList = new ObservableCollection<StudentAttendanceItem>();

        StudentTappedCommand = new Command<StudentAttendanceItem>(async (student) => await OnStudentTapped(student));
        SaveAllCommand = new Command(async () => await OnSaveAllExecuted());
        ClosePageCommand = new Command(async () => await OnClosePageExecuted());
    }

    // Fetch Group details and import/load students belonging strictly to this GroupId
    private async Task LoadGroupAndStudentsAsync()
    {
        if (GroupId <= 0) return;

        try
        {
            IsBusy = true;

            // 1. Fetch Group Name
            var group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == GroupId);
            if (group != null)
            {
                GroupName = $"إدارة حضور: {group.Name}";
            }
            else
            {
                GroupName = "إدارة حضور المجموعة";
            }

            // 2. Fetch Students belonging strictly to this GroupId from database
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
                    Status = AttendanceStatus.Present // Default status as present on load
                });
            }

            // 3. Update Statistics Counters
            TotalStudentsCount = StudentsList.Count;
            PresentCount = StudentsList.Count;
            AbsentCount = 0;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load group students: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // When clicking a student, navigate to StudentAttendanceLogPage passing GroupId and StudentId
    private async Task OnStudentTapped(StudentAttendanceItem student)
    {
        if (student == null) return;

        // Navigate to the individual student attendance log page passing both GroupId and StudentId
        await Shell.Current.GoToAsync($"{nameof(StudentAttendanceLogPage)}?GroupId={GroupId}&StudentId={student.StudentId}");
    }

    private async Task OnSaveAllExecuted()
    {
        try
        {
            await Shell.Current.DisplayAlert("Success", "تم حفظ سجل الحضور والغياب للمجموعة بنجاح.", "OK");
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
