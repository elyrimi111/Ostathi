using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

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
        if (SetProperty(ref _groupId, value))
        {
            if (_groupId > 0)
                _ = LoadGroupAndStudentsAsync();
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
    set => SetProperty(ref _totalStudentsCount, value);
}

private int _presentCount;

public int PresentCount
{
    get => _presentCount;
    set => SetProperty(ref _presentCount, value);
}

private int _absentCount;

public int AbsentCount
{
    get => _absentCount;
    set => SetProperty(ref _absentCount, value);
}

private StudentAttendanceItem? _selectedStudent;

public StudentAttendanceItem? SelectedStudent
{
    get => _selectedStudent;
    set => SetProperty(ref _selectedStudent, value);
}

public ObservableCollection<StudentAttendanceItem> StudentsList { get; }
    = new();

public ICommand StudentTappedCommand { get; }

public ICommand SaveAllCommand { get; }

public ICommand ClosePageCommand { get; }

public GroupAttendanceManagementPageViewModel()
{
    _dbContext = new AppDbContext();

    StudentTappedCommand =
        new Command<StudentAttendanceItem>(
            async student => await OnStudentTapped(student));

    SaveAllCommand =
        new Command(async () => await OnSaveAllExecuted());

    ClosePageCommand =
        new Command(async () => await OnClosePageExecuted());
}

private async Task LoadGroupAndStudentsAsync()
{
    if (GroupId <= 0 || IsBusy)
        return;

    try
    {
        IsBusy = true;

        var group = await _dbContext.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == GroupId);

        GroupName = group != null
            ? $"إدارة حضور: {group.Name}"
            : "إدارة حضور المجموعة";

        var students = await _dbContext.Students
            .AsNoTracking()
            .Where(s => s.GroupId == GroupId)
            .ToListAsync();

        StudentsList.Clear();

        foreach (var student in students)
        {
            StudentsList.Add(new StudentAttendanceItem
            {
                StudentId = student.Id,
                FullName = student.FullName,
                PhoneNumber = student.AcademicId ?? string.Empty,
                Status = AttendanceStatus.Present
            });
        }

        UpdateStatistics();
    }
    catch (Exception ex)
    {
        await Shell.Current.DisplayAlert(
            "خطأ",
            $"تعذر تحميل طلاب المجموعة: {ex.Message}",
            "موافق");
    }
    finally
    {
        IsBusy = false;
    }
}

private void UpdateStatistics()
{
    TotalStudentsCount = StudentsList.Count;

    PresentCount = StudentsList.Count(
        s => s.Status == AttendanceStatus.Present);

    AbsentCount = StudentsList.Count(
        s => s.Status == AttendanceStatus.Absent);
}

private async Task OnStudentTapped(StudentAttendanceItem? student)
{
    if (student == null)
        return;

    await Shell.Current.GoToAsync(
        $"{nameof(StudentAttendanceLogPage)}?GroupId={GroupId}&StudentId={student.StudentId}");
}

private async Task OnSaveAllExecuted()
{
    try
    {
        await Shell.Current.DisplayAlert(
            "تنبيه",
            "يجب ربط أمر الحفظ بجدول سجلات الحضور قبل اعتماد الحفظ.",
            "موافق");
    }
    catch (Exception ex)
    {
        await Shell.Current.DisplayAlert(
            "خطأ",
            ex.Message,
            "موافق");
    }
}

private async Task OnClosePageExecuted()
{
    await Shell.Current.GoToAsync("..");
}

}