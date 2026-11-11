using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

[QueryProperty(nameof(StudentId), "StudentId")]
public class StudentDetailsPageViewModel : BaseViewModel
{
    private readonly AppDbContext _dbContext;

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

        EditStudentCommand = new Command(async () => await OnEditStudentExecuted());
        ClosePageCommand = new Command(async () => await OnClosePageExecuted());
        SendWhatsAppCommand = new Command(async () => await OnSendWhatsAppExecuted());
        AddNoteCommand = new Command(async () => await OnAddNoteExecuted());
        SelectSubjectCommand = new Command<StudentSubjectItem>(async (subject) => await OnSelectSubjectExecuted(subject));

        LoadSampleSubjects();
    }

    private void LoadSampleSubjects()
    {
        StudentSubjectsList.Clear();
        StudentSubjectsList.Add(new StudentSubjectItem { SubjectId = 1, SubjectName = "برمجة الحاسوب C#", TeacherOrDetails = "انقر لإدارة درجات المادة", Grade = "88/100" });
        StudentSubjectsList.Add(new StudentSubjectItem { SubjectId = 2, SubjectName = "قواعد البيانات SQLite", TeacherOrDetails = "انقر لإدارة درجات المادة", Grade = "92/100" });
        StudentSubjectsList.Add(new StudentSubjectItem { SubjectId = 3, SubjectName = "هيكلة البيانات والوارزميات", TeacherOrDetails = "انقر لإدارة درجات المادة", Grade = "79/100" });
    }

    private async Task LoadStudentDataAsync()
    {
        try
        {
            IsBusy = true;
            var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.Id == StudentId);

            if (student != null)
            {
                FullName = string.IsNullOrWhiteSpace(student.FullName) ? "اسم الطالب غير متوفر" : student.FullName;
                AcademicId = string.IsNullOrWhiteSpace(student.AcademicId) ? "غير محدد" : student.AcademicId;
                PhoneNumber = "777000000";
                AcademicLevel = string.IsNullOrWhiteSpace(student.AcademicYear) ? "المستوى الأول" : student.AcademicYear;
                MajorName = $"تخصص #{student.MajorId}";

                if (student.GroupId > 0)
                {
                    var group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == student.GroupId);
                    GroupName = (group != null && !string.IsNullOrWhiteSpace(group.Name)) ? group.Name : $"مجموعة #{student.GroupId}";
                }
                else
                {
                    GroupName = "بدون مجموعة";
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"فشل تحميل بيانات الطالب: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnSelectSubjectExecuted(StudentSubjectItem subject)
    {
        if (subject == null) return;

        try
        {
            // فتح شاشة تعديل درجات هذه المادة لهذا الطالب
            await Shell.Current.GoToAsync($"{nameof(EditStudentGradesPage)}?StudentId={StudentId}&SubjectId={subject.SubjectId}");
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
                await Shell.Current.DisplayAlert("تنبيـه", "رقم الهاتف غير متوفر لهذا الطالب.", "OK");
                return;
            }
            string cleanPhone = PhoneNumber.Replace("+", "").Replace(" ", "").Trim();
            await Launcher.Default.OpenAsync(new Uri($"https://wa.me/{cleanPhone}?text=مرحباً بك يا طالبنا العزيز {FullName}،"));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("خطأ", $"تعذر فتح تطبيق واتساب: {ex.Message}", "OK");
        }
    }

    private async Task OnAddNoteExecuted()
    {
        if (string.IsNullOrWhiteSpace(NewNoteText))
        {
            await Shell.Current.DisplayAlert("تنبيـه", "الرجاء كتابة نص الملاحظة أولاً.", "OK");
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
        await Shell.Current.GoToAsync($"{nameof(AddUpdateStudent)}?StudentId={StudentId}");
    }

    private async Task OnClosePageExecuted()
    {
        await Shell.Current.GoToAsync("..");
    }
}

public class StudentSubjectItem
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string TeacherOrDetails { get; set; } = string.Empty;
    public string Grade { get; set; } = "0/100";
}

public class StudentNoteItem
{
    public string NoteContent { get; set; } = string.Empty;
    public string DateString { get; set; } = string.Empty;
}
