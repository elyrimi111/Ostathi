using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public class SubjectsViewModel : BaseViewModel
{
    private readonly SubjectService _subjectService;
    private List<Subject> _allSubjects = new();

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                ApplyFilter();
            }
        }
    }

    public ObservableCollection<Subject> FilteredSubjects { get; } = new();

    public ICommand LoadSubjectsCommand { get; }
    public ICommand AddSubjectCommand { get; }
    public ICommand SubjectTappedCommand { get; }
    public ICommand SubjectOptionsCommand { get; }

    public SubjectsViewModel(SubjectService subjectService)
    {
        _subjectService = subjectService;

        LoadSubjectsCommand = new Command(async () => await LoadSubjectsAsync());
        AddSubjectCommand = new Command(async () => await OnAddSubjectAsync());
        SubjectTappedCommand = new Command<Subject>(async (subject) => await OnEditSubjectAsync(subject));
        SubjectOptionsCommand = new Command<Subject>(async (subject) => await OnSubjectOptionsAsync(subject));
    }

    // Load subjects from the service and apply the active search filter
    public async Task LoadSubjectsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var subjects = await _subjectService.GetSubjectsAsync();
            _allSubjects = subjects ?? new List<Subject>();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"تعذر تحميل المواد: {ex.Message}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Filter the cached subjects list by name or description
    private void ApplyFilter()
    {
        string query = SearchQuery?.Trim() ?? string.Empty;

        var results = string.IsNullOrWhiteSpace(query)
            ? _allSubjects
            : _allSubjects.Where(s =>
                (!string.IsNullOrWhiteSpace(s.Name) && s.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(s.Description) && s.Description.Contains(query, StringComparison.OrdinalIgnoreCase)));

        FilteredSubjects.Clear();
        foreach (var item in results)
        {
            FilteredSubjects.Add(item);
        }
    }

    // Navigate to AddUpdateSubject in Add mode
    private async Task OnAddSubjectAsync()
    {
        if (Shell.Current == null) return;
        await Shell.Current.GoToAsync($"{nameof(AddUpdateSubject)}?SubjectId=0");
    }

    // Navigate to AddUpdateSubject in Update mode with SubjectId
    private async Task OnEditSubjectAsync(Subject? subject)
    {
        if (subject == null || subject.Id <= 0 || Shell.Current == null)
            return;

        await Shell.Current.GoToAsync($"{nameof(AddUpdateSubject)}?SubjectId={subject.Id}");
    }

    // Show context action sheet for edit and delete options
    private async Task OnSubjectOptionsAsync(Subject? subject)
    {
        if (subject == null || subject.Id <= 0 || Shell.Current == null)
            return;

        const string editAction = "تعديل المادة";
        const string deleteAction = "حذف المادة";

        string? choice = await Shell.Current.DisplayActionSheet(
            subject.Name,
            "إلغاء",
            deleteAction,
            editAction);

        if (choice == editAction)
        {
            await OnEditSubjectAsync(subject);
        }
        else if (choice == deleteAction)
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "تأكيد الحذف",
                $"هل أنت متأكد من حذف مادة \"{subject.Name}\"؟",
                "حذف",
                "إلغاء");

            if (!confirm) return;

            try
            {
                await _subjectService.DeleteSubjectAsync(subject);
                _allSubjects.RemoveAll(s => s.Id == subject.Id);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("خطأ", $"تعذر حذف المادة: {ex.Message}", "موافق");
            }
        }
    }
}
