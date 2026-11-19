using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi;

[QueryProperty(nameof(GroupSubjectId), "GroupSubjectId")]
[QueryProperty(nameof(GroupId), "GroupId")]
public partial class SubjectGroupGradesOverviewViewModel : ObservableObject
{
    private readonly AppDbContext _context;
    private readonly SubjectGroupStudentGradeService _gradeService;

    [ObservableProperty]
    private int _groupSubjectId;

    [ObservableProperty]
    private int _groupId;

    // Observable collection containing all students of the group with their grades
    public ObservableCollection<StudentGradeDisplayModel> StudentGradesList { get; set; } = new();

    private StudentGradeDisplayModel? _selectedStudentGrade;
    public StudentGradeDisplayModel? SelectedStudentGrade
    {
        get => _selectedStudentGrade;
        set
        {
            SetProperty(ref _selectedStudentGrade, value);
            if (value != null)
            {
                NavigateToDetailsCommand.Execute(value);
                SelectedStudentGrade = null; // Reset selection
            }
        }
    }

    public ICommand LoadDataCommand { get; }
    public ICommand NavigateToDetailsCommand { get; }
    public ICommand InitializeMissingGradesCommand { get; }

    public SubjectGroupGradesOverviewViewModel(AppDbContext context, SubjectGroupStudentGradeService gradeService)
    {
        _context = context;
        _gradeService = gradeService;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        NavigateToDetailsCommand = new AsyncRelayCommand<StudentGradeDisplayModel>(NavigateToDetailsAsync);
        InitializeMissingGradesCommand = new AsyncRelayCommand(InitializeMissingGradesAsync);
    }

    // يتم استدعاؤها تلقائياً بمجرد تعيين أو تغير GroupId عن طريق الـ Shell Navigation
    partial void OnGroupIdChanged(int value)
    {
        if (value > 0)
        {
            _ = LoadDataAsync();
        }
    }

    // يتم استدعاؤها تلقائياً بمجرد تغير GroupSubjectId
    partial void OnGroupSubjectIdChanged(int value)
    {
        if (value > 0 && GroupId > 0)
        {
            _ = LoadDataAsync();
        }
    }

    // Load all students belonging to this group and map their grades for this subject group
    public async Task LoadDataAsync()
    {
        if (GroupId == 0) return;

        // Fetch all students in this specific group directly from AppDbContext
        var groupStudents = _context.Students
            .Where(s => s.GroupId == GroupId)
            .ToList();

        // Fetch existing grades recorded for this subject group
        var existingGrades = await _gradeService.GetByGroupSubjectIdAsync(GroupSubjectId);

        StudentGradesList.Clear();

        foreach (var student in groupStudents)
        {
            var gradeRecord = existingGrades.FirstOrDefault(g => g.StudentId == student.Id);

            StudentGradesList.Add(new StudentGradeDisplayModel
            {
                GradeId = gradeRecord?.Id ?? 0,
                StudentId = student.Id,
                GroupSubjectId = GroupSubjectId,
                StudentFullName = $"{student.FirstName} {student.SecondName} {student.ThirdName} {student.LastName}",
                AcademicId = student.AcademicId,
                Attendance = gradeRecord?.Attendance ?? 0,
                Assignments = gradeRecord?.Assignments ?? 0,
                Midterm = gradeRecord?.Midterm ?? 0,
                Final = gradeRecord?.Final ?? 0
            });
        }
    }

    // Automatically import/initialize grade records for all students in the group who don't have records yet
    private async Task InitializeMissingGradesAsync()
    {
        if (GroupSubjectId == 0 || GroupId == 0) return;

        var groupStudents = _context.Students
            .Where(s => s.GroupId == GroupId)
            .ToList();
            
        var existingGrades = await _gradeService.GetByGroupSubjectIdAsync(GroupSubjectId);

        bool addedAny = false;
        foreach (var student in groupStudents)
        {
            bool exists = existingGrades.Any(g => g.StudentId == student.Id);
            if (!exists)
            {
                var newGrade = new SubjectGroupStudentGrade
                {
                    StudentId = student.Id,
                    GroupSubjectId = GroupSubjectId,
                    Attendance = 0,
                    Assignments = 0,
                    Midterm = 0,
                    Final = 0
                };
                await _gradeService.SaveAsync(newGrade);
                addedAny = true;
            }
        }

        if (addedAny)
        {
            await LoadDataAsync();
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("نجاح", "تم استيراد وإنشاء سجلات درجات لجميع طلاب المجموعة بنجاح", "موافق");
            }
        }
        else
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تنبيه", "جميع طلاب المجموعة لديهم سجلات درجات مسبقة بالفعل", "موافق");
            }
        }
    }

    // Navigate to the student grade details/editing page
    private async Task NavigateToDetailsAsync(StudentGradeDisplayModel? model)
    {
        if (model == null) return;

        await Shell.Current.GoToAsync(
            $"{nameof(SubjectGroupStudentGradeDetailsPage)}?GradeId={model.GradeId}&StudentId={model.StudentId}&GroupSubjectId={model.GroupSubjectId}");
    }
}

// Display model to wrap student info with their grade breakdown
public class StudentGradeDisplayModel
{
    public int GradeId { get; set; }
    public int StudentId { get; set; }
    public int GroupSubjectId { get; set; }
    public string StudentFullName { get; set; } = string.Empty;
    public string AcademicId { get; set; } = string.Empty;
    public double Attendance { get; set; }
    public double Assignments { get; set; }
    public double Midterm { get; set; }
    public double Final { get; set; }
    public double Total => Attendance + Assignments + Midterm + Final;
}
