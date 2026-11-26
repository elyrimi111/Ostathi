using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Ostathi;

public class GroupStudentRowItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string AcademicId { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public int GroupId { get; set; }
}

[QueryProperty(nameof(GroupId), "GroupId")]
[QueryProperty(nameof(SubjectId), "SubjectId")]
[QueryProperty(nameof(GroupSubjectId), "GroupSubjectId")]
public partial class GroupSubjectStudentsViewModel : ObservableObject
{
    private readonly AppDbContext _dbContext = new();
    private List<GroupStudentRowItem> _allStudents = new();

    [ObservableProperty]
    private int _groupId;

    [ObservableProperty]
    private int _subjectId;

    [ObservableProperty]
    private int _groupSubjectId;

    [ObservableProperty]
    private string _groupName = "غير محدد";

    [ObservableProperty]
    private string _subjectName = "غير محدد";

    [ObservableProperty]
    private int _totalStudentsCount;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isRefreshing;

    public ObservableCollection<GroupStudentRowItem> FilteredStudents { get; } = new();

    public bool IsNotBusy => !IsBusy;
    public bool IsEmpty => !IsBusy && FilteredStudents.Count == 0;

    public ICommand OpenStudentDetailsCommand { get; }
    public ICommand EditStudentCommand { get; }
    public ICommand DeleteStudentCommand { get; }
    public ICommand StudentOptionsCommand { get; }
    public ICommand RefreshCommand { get; }

    public GroupSubjectStudentsViewModel()
    {
        OpenStudentDetailsCommand = new AsyncRelayCommand<GroupStudentRowItem>(OnOpenStudentDetailsAsync);
        EditStudentCommand = new AsyncRelayCommand<GroupStudentRowItem>(OnEditStudentAsync);
        DeleteStudentCommand = new AsyncRelayCommand<GroupStudentRowItem>(OnDeleteStudentAsync);
        StudentOptionsCommand = new AsyncRelayCommand<GroupStudentRowItem>(OnStudentOptionsAsync);
        RefreshCommand = new AsyncRelayCommand(OnRefreshAsync);
    }

    partial void OnGroupIdChanged(int value)
    {
        if (value > 0)
        {
            _ = LoadDataAsync();
        }
    }

    partial void OnSubjectIdChanged(int value)
    {
        if (value > 0)
        {
            _ = LoadHeaderInfoAsync();
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilter();
    }

    private async Task LoadHeaderInfoAsync()
    {
        try
        {
            if (GroupId > 0)
            {
                var group = await _dbContext.Groups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == GroupId);

                GroupName = group?.Name ?? $"مجموعة #{GroupId}";
            }

            if (SubjectId > 0)
            {
                var subject = await _dbContext.Subjects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == SubjectId);

                SubjectName = subject?.Name ?? $"مادة #{SubjectId}";
            }
        }
        catch
        {
            // Keep existing or fallback values if lookup fails
        }
    }

    public async Task LoadDataAsync()
    {
        if (IsBusy || GroupId <= 0) return;

        try
        {
            IsBusy = true;

            await LoadHeaderInfoAsync();

            var students = await _dbContext.Students
                .AsNoTracking()
                .Where(s => s.GroupId == GroupId)
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.SecondName)
                .ToListAsync();

            _allStudents = students.Select(s => new GroupStudentRowItem
            {
                Id = s.Id,
                FullName = string.Join(" ", new[] { s.FirstName, s.SecondName, s.ThirdName, s.LastName }
                    .Where(n => !string.IsNullOrWhiteSpace(n))),
                AcademicId = s.AcademicId ?? string.Empty,
                AcademicYear = s.AcademicYear ?? string.Empty,
                GroupId = s.GroupId
            }).ToList();

            TotalStudentsCount = _allStudents.Count;
            ApplyFilter();
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"تعذر تحميل طلاب المجموعة: {ex.Message}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    private async Task OnRefreshAsync()
    {
        try
        {
            IsRefreshing = true;
            await LoadDataAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void ApplyFilter()
    {
        string query = SearchQuery?.Trim() ?? string.Empty;

        var results = string.IsNullOrWhiteSpace(query)
            ? _allStudents
            : _allStudents.Where(s =>
                (!string.IsNullOrWhiteSpace(s.FullName) && s.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(s.AcademicId) && s.AcademicId.Contains(query, StringComparison.OrdinalIgnoreCase)));

        MainThread.BeginInvokeOnMainThread(() =>
        {
            FilteredStudents.Clear();
            foreach (var item in results)
            {
                FilteredStudents.Add(item);
            }
            OnPropertyChanged(nameof(IsEmpty));
        });
    }

    private async Task OnStudentOptionsAsync(GroupStudentRowItem? student)
    {
        if (student == null || Shell.Current == null) return;

        string action = await Shell.Current.DisplayActionSheet(
            student.FullName,
            "إلغاء",
            "حذف الطالب",
            "عرض التفاصيل",
            "تعديل بيانات الطالب");

        if (action == "عرض التفاصيل")
        {
            await OnOpenStudentDetailsAsync(student);
        }
        else if (action == "تعديل بيانات الطالب")
        {
            await OnEditStudentAsync(student);
        }
        else if (action == "حذف الطالب")
        {
            await OnDeleteStudentAsync(student);
        }
    }

    private async Task OnOpenStudentDetailsAsync(GroupStudentRowItem? student)
    {
        if (student == null || student.Id <= 0 || Shell.Current == null) return;
        await Shell.Current.GoToAsync($"{nameof(StudentDetailsPage)}?StudentId={student.Id}&GroupId={GroupId}&SubjectId={SubjectId}");
    }

    private async Task OnEditStudentAsync(GroupStudentRowItem? student)
    {
        if (student == null || student.Id <= 0 || Shell.Current == null) return;
        await Shell.Current.GoToAsync($"{nameof(AddUpdateStudent)}?StudentId={student.Id}&GroupId={GroupId}");
    }

    private async Task OnDeleteStudentAsync(GroupStudentRowItem? student)
    {
        if (student == null || Shell.Current == null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "تأكيد الحذف",
            $"هل تريد حذف الطالب \"{student.FullName}\" من المجموعة؟",
            "حذف",
            "إلغاء");

        if (!confirm) return;

        try
        {
            var entity = await _dbContext.Students.FindAsync(student.Id);
            if (entity != null)
            {
                _dbContext.Students.Remove(entity);
                await _dbContext.SaveChangesAsync();

                _allStudents.RemoveAll(x => x.Id == student.Id);
                TotalStudentsCount = _allStudents.Count;
                ApplyFilter();

                await Shell.Current.DisplayAlert("نجاح", "تم حذف الطالب بنجاح", "موافق");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("خطأ", $"تعذر حذف الطالب: {ex.Message}", "موافق");
        }
    }
}
