using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi;

[QueryProperty(nameof(GradeId), "GradeId")]
[QueryProperty(nameof(StudentId), "StudentId")]
[QueryProperty(nameof(GroupSubjectId), "GroupSubjectId")]
public partial class SubjectGroupStudentGradeViewModel : ObservableObject
{
    private readonly SubjectGroupStudentGradeService _gradeService;
    private readonly AppDbContext _context;

    [ObservableProperty]
    private int _gradeId;

    [ObservableProperty]
    private int _studentId;

    [ObservableProperty]
    private int _groupSubjectId;

    [ObservableProperty]
    private string _studentFullName = string.Empty;

    [ObservableProperty]
    private string _academicId = string.Empty;

    [ObservableProperty]
    private string _subjectName = string.Empty;

    [ObservableProperty]
    private double _attendance;

    [ObservableProperty]
    private double _assignments;

    [ObservableProperty]
    private double _midterm;

    [ObservableProperty]
    private double _final;

    // حساب المجموع الكلي تلقائياً وتحديث واجهة المستخدم عند تغير أي درجة
    public double Total => Attendance + Assignments + Midterm + Final;

    public ICommand LoadDataCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    public SubjectGroupStudentGradeViewModel(SubjectGroupStudentGradeService gradeService, AppDbContext context)
    {
        _gradeService = gradeService;
        _context = context;

        LoadDataCommand = new AsyncRelayCommand(InitializeDataAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
    }

    // تحديث البيانات تلقائياً بمجرد وصول أو تغير StudentId عبر التنقل
    partial void OnStudentIdChanged(int value)
    {
        if (value > 0)
        {
            _ = InitializeDataAsync();
        }
    }

    // تحديث البيانات تلقائياً بمجرد وصول أو تغير GradeId عبر التنقل
    partial void OnGradeIdChanged(int value)
    {
        if (value > 0)
        {
            _ = InitializeDataAsync();
        }
    }

    // تحديث اسم المادة تلقائياً بمجرد وصول أو تغير GroupSubjectId عبر التنقل
    partial void OnGroupSubjectIdChanged(int value)
    {
        if (value > 0)
        {
            _ = LoadSubjectNameAsync();
        }
    }

    // جلب اسم المادة من خلال GroupSubjectId -> SubjectId
    private async Task LoadSubjectNameAsync()
    {
        if (GroupSubjectId <= 0)
        {
            SubjectName = string.Empty;
            return;
        }

        var groupSubject = await _context.GroupSubjects
            .AsNoTracking()
            .FirstOrDefaultAsync(gs => gs.Id == GroupSubjectId);

        if (groupSubject != null && groupSubject.SubjectId > 0)
        {
            var subject = await _context.Subjects
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == groupSubject.SubjectId);

            SubjectName = subject?.Name ?? $"مادة #{groupSubject.SubjectId}";
        }
    }

    // جلب بيانات الطالب والمادة والسجلات المسجلة مسبقاً
    public async Task InitializeDataAsync()
    {
        if (StudentId > 0)
        {
            var student = await _context.Students.FindAsync(StudentId);
            if (student != null)
            {
                StudentFullName = $"{student.FirstName} {student.SecondName} {student.ThirdName} {student.LastName}";
                AcademicId = student.AcademicId;
            }
        }

        if (GroupSubjectId > 0)
        {
            await LoadSubjectNameAsync();
        }

        if (GradeId > 0)
        {
            var grade = await _gradeService.GetByIdAsync(GradeId);
            if (grade != null)
            {
                Attendance = grade.Attendance;
                Assignments = grade.Assignments;
                Midterm = grade.Midterm;
                Final = grade.Final;

                if (GroupSubjectId == 0 && grade.GroupSubjectId > 0)
                {
                    GroupSubjectId = grade.GroupSubjectId;
                    await LoadSubjectNameAsync();
                }
            }
        }
    }

    partial void OnAttendanceChanged(double value) => OnPropertyChanged(nameof(Total));
    partial void OnAssignmentsChanged(double value) => OnPropertyChanged(nameof(Total));
    partial void OnMidtermChanged(double value) => OnPropertyChanged(nameof(Total));
    partial void OnFinalChanged(double value) => OnPropertyChanged(nameof(Total));

    // حفظ درجات الطالب
    private async Task SaveAsync()
    {
        var gradeRecord = new SubjectGroupStudentGrade
        {
            Id = GradeId,
            StudentId = StudentId,
            GroupSubjectId = GroupSubjectId,
            Attendance = Attendance,
            Assignments = Assignments,
            Midterm = Midterm,
            Final = Final
        };

        bool success = await _gradeService.SaveAsync(gradeRecord);
        if (success)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("نجاح", "تم حفظ درجات الطالب بنجاح", "موافق");
                await Shell.Current.GoToAsync("..");
            }
        }
        else
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", "فشل في حفظ البيانات", "موافق");
            }
        }
    }

    // حذف السجل
    private async Task DeleteAsync()
    {
        if (GradeId == 0)
        {
            await Shell.Current!.GoToAsync("..");
            return;
        }

        bool confirm = await Shell.Current!.DisplayAlert("تأكيد الحذف", "هل أنت متأكد من حذف سجل درجات هذا الطالب؟", "نعم", "إلغاء");
        if (confirm)
        {
            bool success = await _gradeService.DeleteAsync(GradeId);
            if (success)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}
