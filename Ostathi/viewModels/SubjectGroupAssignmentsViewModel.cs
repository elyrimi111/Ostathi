using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Ostathi.Models;
using Ostathi.Services;

namespace Ostathi.ViewModels;

[QueryProperty(nameof(GroupId), nameof(GroupId))]
[QueryProperty(nameof(SubjectId), nameof(SubjectId))]
public class SubjectGroupAssignmentsViewModel : BaseViewModel
{
    private readonly AppDbContext _dbContext;
    private readonly AssignmentService _assignmentService;

    private int _groupId;
    public int GroupId
    {
        get => _groupId;
        set
        {
            if (SetProperty(ref _groupId, value))
            {
                TryLoadData();
            }
        }
    }

    private int _subjectId;
    public int SubjectId
    {
        get => _subjectId;
        set
        {
            if (SetProperty(ref _subjectId, value))
            {
                TryLoadData();
            }
        }
    }

    private string _groupName = "تحميل...";
    public string GroupName
    {
        get => _groupName;
        set => SetProperty(ref _groupName, value);
    }

    private string _subjectName = "تحميل...";
    public string SubjectName
    {
        get => _subjectName;
        set => SetProperty(ref _subjectName, value);
    }

    public ObservableCollection<Assignment> AssignmentsList { get; } = new();

    public ICommand AddAssignmentCommand { get; }
    public ICommand DeleteAssignmentCommand { get; }

    // مُنشئ افتراضي بدون معاملات (بدون حقن تبعيات)
    public SubjectGroupAssignmentsViewModel()
    {
        // إنشاء الكائنات يدوياً هنا
        _dbContext = new AppDbContext();
        _assignmentService = new AssignmentService(_dbContext);

        AddAssignmentCommand = new Command(async () => await NavigateToAddAssignmentAsync());
        DeleteAssignmentCommand = new Command<Assignment>(async (a) => await DeleteAssignmentAsync(a));
    }

    // دالة مساعدة لضمان تحميل البيانات فقط عندما يتوفر المعرفان معا
    private void TryLoadData()
    {
        if (GroupId > 0 && SubjectId > 0)
        {
            _ = LoadDataAsync();
        }
    }

    public async Task LoadDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // جلب اسم المجموعة
            var group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == GroupId);
            if (group != null) GroupName = group.Name;

            // جلب اسم المقرر/المادة
            var subject = await _dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == SubjectId);
            string targetSubject = subject?.Name ?? string.Empty;
            SubjectName = targetSubject;

            // جلب قائمة التكاليف والواجبات الخاصة بالمادة والمجموعة
            var items = await _assignmentService.GetAssignmentsAsync(GroupId, targetSubject);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AssignmentsList.Clear();
                foreach (var item in items)
                {
                    AssignmentsList.Add(item);
                }
            });
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"فشل في تحميل التكاليف: {ex.Message}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task NavigateToAddAssignmentAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync($"AddUpdateAssignmentPage?GroupId={GroupId}&SubjectId={SubjectId}");
        }
    }

    private async Task DeleteAssignmentAsync(Assignment assignment)
    {
        if (assignment == null) return;

        bool confirm = false;
        if (Shell.Current != null)
        {
            confirm = await Shell.Current.DisplayAlert("تأكيد الحذف", "هل أنت متأكد من حذف هذا التكليف؟", "نعم", "إلغاء");
        }

        if (confirm)
        {
            await _assignmentService.DeleteAssignmentAsync(assignment);
            AssignmentsList.Remove(assignment);
        }
    }
}
