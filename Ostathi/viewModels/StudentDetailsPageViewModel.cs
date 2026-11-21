using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;

namespace Ostathi;

[QueryProperty(nameof(StudentId), "StudentId")]
public class StudentDetailsPageViewModel : BaseViewModel
{
    private readonly AppDbContext _dbContext;
    private readonly GroupSubjectService _groupSubjectService;

    private int _studentId;
    public int StudentId
    {
        get => _studentId;
        set
        {
            if (_studentId != value)
            {
                _studentId = value;
                OnPropertyChanged();
                if (_studentId > 0)
                {
                    _ = LoadStudentDataAsync();
                }
            }
        }
    }

    private string _fullName = string.Empty;
    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    private string _academicId = string.Empty;
    public string AcademicId
    {
        get => _academicId;
        set => SetProperty(ref _academicId, value);
    }

    private string _phoneNumber = string.Empty;
    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
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

    private string _majorName = string.Empty;
    public string MajorName
    {
        get => _majorName;
        set => SetProperty(ref _majorName, value);
    }

    private string _newNoteText = string.Empty;
    public string NewNoteText
    {
        get => _newNoteText;
        set => SetProperty(ref _newNoteText, value);
    }

    public ObservableCollection<StudentNoteItem> StudentNotesList { get; } = new();
    public ObservableCollection<StudentSubjectItem> StudentSubjectsList { get; } = new();

    public ICommand EditStudentCommand { get; }
    public ICommand ClosePageCommand { get; }
    public ICommand SendWhatsAppCommand { get; }
    public ICommand AddNoteCommand { get; }
    public ICommand SelectSubjectCommand { get; }

    public StudentDetailsPageViewModel()
    {
        _dbContext = new AppDbContext();
        _groupSubjectService = new GroupSubjectService();

        EditStudentCommand = new Command(async () => await OnEditStudentExecuted());
        ClosePageCommand = new Command(async () => await OnClosePageExecuted());
        SendWhatsAppCommand = new Command(async () => await OnSendWhatsAppExecuted());
        AddNoteCommand = new Command(async () => await OnAddNoteExecuted());
        SelectSubjectCommand = new Command<StudentSubjectItem>(async (subject) => await OnSelectSubjectExecuted(subject));
    }

    private async Task LoadStudentDataAsync()
    {
        try
        {
            IsBusy = true;
            StudentSubjectsList.Clear();

            var student = await _dbContext.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == StudentId);

            if (student == null)
                return;

            FullName = string.IsNullOrWhiteSpace(student.FullName) ? "اسم الطالب غير متوفر" : student.FullName;
            AcademicId = string.IsNullOrWhiteSpace(student.AcademicId) ? "غير محدد" : student.AcademicId;
            PhoneNumber = "777000000";
            AcademicLevel = string.IsNullOrWhiteSpace(student.AcademicYear) ? "المستوى الأول" : student.AcademicYear;
            MajorName = $"تخصص #{student.MajorId}";

            if (student.GroupId > 0)
            {
                var group = await _dbContext.Groups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == student.GroupId);

                GroupName = (group != null && !string.IsNullOrWhiteSpace(group.Name))
                    ? group.Name
                    : $"مجموعة #{student.GroupId}";

                await LoadStudentSubjectsAsync(student.Id, student.GroupId);
            }
            else
            {
                GroupName = "بدون مجموعة";
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"فشل تحميل بيانات الطالب: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadStudentSubjectsAsync(int studentId, int groupId)
    {
        List<GroupSubject> groupSubjects = await _groupSubjectService.GetSubjectsForGroupAsync(groupId);
        if (groupSubjects.Count == 0)
            return;

        var subjectIds = groupSubjects.Select(gs => gs.SubjectId).Distinct().ToList();
        var groupSubjectIds = groupSubjects.Select(gs => gs.Id).Distinct().ToList();

        var subjectsMap = await _dbContext.Subjects
            .AsNoTracking()
            .Where(s => subjectIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        var gradesMap = await _dbContext.SubjectGroupStudentGrades
            .AsNoTracking()
            .Where(g => g.StudentId == studentId && groupSubjectIds.Contains(g.GroupSubjectId))
            .ToDictionaryAsync(g => g.GroupSubjectId);

        foreach (var gs in groupSubjects)
        {
            subjectsMap.TryGetValue(gs.SubjectId, out var subjectEntity);
            gradesMap.TryGetValue(gs.Id, out var gradeRecord);

            double total = gradeRecord != null
                ? gradeRecord.Attendance + gradeRecord.Assignments + gradeRecord.Midterm + gradeRecord.Final
                : 0;

            StudentSubjectsList.Add(new StudentSubjectItem
            {
                SubjectId = gs.SubjectId,
                GroupSubjectId = gs.Id,
                GradeId = gradeRecord?.Id ?? 0,
                SubjectName = !string.IsNullOrWhiteSpace(subjectEntity?.Name)
                    ? subjectEntity.Name
                    : $"مادة #{gs.SubjectId}",
                TeacherOrDetails = "انقر لإدارة درجات المادة",
                Grade = $"{total:0.#}/100"
            });
        }
    }

    private async Task OnSelectSubjectExecuted(StudentSubjectItem subject)
    {
        if (subject == null || Shell.Current == null) return;

        try
        {
            int gradeId = subject.GradeId;

            if (gradeId == 0 && subject.GroupSubjectId > 0 && StudentId > 0)
            {
                var existingGrade = await _dbContext.SubjectGroupStudentGrades
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.StudentId == StudentId && g.GroupSubjectId == subject.GroupSubjectId);

                gradeId = existingGrade?.Id ?? 0;
            }

            await Shell.Current.GoToAsync(
                $"{nameof(SubjectGroupStudentGradeDetailsPage)}?GradeId={gradeId}&StudentId={StudentId}&GroupSubjectId={subject.GroupSubjectId}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("خطأ", $"تعذر فتح شاشة درجات المادة: {ex.Message}", "OK");
        }
    }

    private async Task OnSendWhatsAppExecuted()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(PhoneNumber) || PhoneNumber == "غير متوفر")
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("تنبيـه", "رقم الهاتف غير متوفر لهذا الطالب.", "OK");
                }
                return;
            }

            string cleanPhone = PhoneNumber.Replace("+", "").Replace(" ", "").Trim();
            await Launcher.Default.OpenAsync(new Uri($"https://wa.me/{cleanPhone}?text=مرحباً بك يا طالبنا العزيز {FullName}،"));
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"تعذر فتح تطبيق واتساب: {ex.Message}", "OK");
            }
        }
    }

    private async Task OnAddNoteExecuted()
    {
        if (string.IsNullOrWhiteSpace(NewNoteText))
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تنبيـه", "الرجاء كتابة نص الملاحظة أولاً.", "OK");
            }
            return;
        }

        StudentNotesList.Insert(0, new StudentNoteItem
        {
            NoteContent = NewNoteText.Trim(),
            DateString = DateTime.Now.ToString("yyyy/MM/dd")
        });

        NewNoteText = string.Empty;
    }

    private async Task OnEditStudentExecuted()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync($"{nameof(AddUpdateStudent)}?StudentId={StudentId}");
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

public class StudentSubjectItem
{
    public int SubjectId { get; set; }
    public int GroupSubjectId { get; set; }
    public int GradeId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string TeacherOrDetails { get; set; } = string.Empty;
    public string Grade { get; set; } = "0/100";
}

public class StudentNoteItem
{
    public string NoteContent { get; set; } = string.Empty;
    public string DateString { get; set; } = string.Empty;
}
