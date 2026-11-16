using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ostathi.Models;
using Ostathi.Services;

namespace Ostathi;

[QueryProperty(nameof(GroupId), nameof(GroupId))]
[QueryProperty(nameof(SubjectName), nameof(SubjectName))]
[QueryProperty(nameof(AssignmentId), nameof(AssignmentId))]
public class AddUpdateAssignmentViewModel : BaseViewModel, IQueryAttributable
{
    // Service 
    private readonly AssignmentService _assignmentService = new AssignmentService(new AppDbContext());

    private int _groupId;
    public int GroupId
    {
        get => _groupId;
        set => SetProperty(ref _groupId, value);
    }

    private string _subjectName = string.Empty;
    public string SubjectName
    {
        get => _subjectName;
        set => SetProperty(ref _subjectName, value);
    }

    private int _assignmentId;
    public int AssignmentId
    {
        get => _assignmentId;
        set
        {
            SetProperty(ref _assignmentId, value);
            if (_assignmentId > 0)
            {
                _ = LoadAssignmentAsync(_assignmentId);
            }
        }
    }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private DateTime _dueDate = DateTime.Now.AddDays(7);
    public DateTime DueDate
    {
        get => _dueDate;
        set => SetProperty(ref _dueDate, value);
    }

    private Assignment? _currentAssignment;

    public ICommand SaveCommand { get; }

    public AddUpdateAssignmentViewModel()
    {
        SaveCommand = new Command(async () => await SaveAssignmentAsync());
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("GroupId", out var groupObj) && int.TryParse(groupObj?.ToString(), out var groupId))
        {
            GroupId = groupId;
        }

        if (query.TryGetValue("SubjectName", out var subjectObj) && subjectObj is string subject)
        {
            SubjectName = subject;
        }

        if (query.TryGetValue("AssignmentId", out var assignmentObj) && int.TryParse(assignmentObj?.ToString(), out var assignmentId))
        {
            AssignmentId = assignmentId;
        }
    }

    private async Task LoadAssignmentAsync(int id)
    {
        try
        {
            IsBusy = true;
            _currentAssignment = await _assignmentService.GetAssignmentAsync(id);
            if (_currentAssignment != null)
            {
                Title = _currentAssignment.Title;
                Description = _currentAssignment.Description ?? string.Empty;
                DueDate = _currentAssignment.DueDate;
                GroupId = _currentAssignment.GroupId;
                SubjectName = _currentAssignment.SubjectName ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"فشل في تحميل بيانات التكليف: {ex.Message}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAssignmentAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تنبيه", "الرجاء إدخال عنوان التكليف على الأقل", "موافق");
            }
            return;
        }

        try
        {
            IsBusy = true;

            var assignment = _currentAssignment ?? new Assignment();
            assignment.GroupId = GroupId;
            assignment.SubjectName = SubjectName;
            assignment.Title = Title;
            assignment.Description = Description;
            assignment.DueDate = DueDate;

            await _assignmentService.SaveAssignmentAsync(assignment);

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("نجاح", "تم حفظ التكليف بنجاح", "موافق");
                await Shell.Current.GoToAsync(".."); // العودة للشاشة السابقة
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"فشل في حفظ التكليف: {ex.Message}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
