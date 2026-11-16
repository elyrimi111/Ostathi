using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ostathi.Models;
using Ostathi.Services;

namespace Ostathi;

[QueryProperty(nameof(GroupId), nameof(GroupId))]
[QueryProperty(nameof(SubjectName), nameof(SubjectName))]
[QueryProperty(nameof(AssignmentId), nameof(AssignmentId))]
public class AddUpdateAssignmentViewModel : BaseViewModel
{
    private readonly AssignmentService _assignmentService;

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
        set => SetProperty(ref _assignmentId, value);
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

    public AddUpdateAssignmentViewModel(AssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
        SaveCommand = new Command(async () => await SaveAssignmentAsync());
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
