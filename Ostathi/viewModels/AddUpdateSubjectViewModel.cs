using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

[QueryProperty(nameof(SubjectId), nameof(SubjectId))]
public class AddUpdateSubjectViewModel : BaseViewModel, IQueryAttributable
{
    public enum SubjectMode
    {
        Add,
        Update
    }

    private readonly SubjectService _subjectService;
    private Subject? _editingSubject;

    private int? _subjectId;
    public int? SubjectId
    {
        get => _subjectId;
        set
        {
            if (SetProperty(ref _subjectId, value))
            {
                if (_subjectId.HasValue && _subjectId.Value > 0)
                {
                    CurrentMode = SubjectMode.Update;
                    _ = LoadSubjectForEditAsync(_subjectId.Value);
                }
                else
                {
                    CurrentMode = SubjectMode.Add;
                    _editingSubject = null;
                }
            }
        }
    }

    private SubjectMode _currentMode = SubjectMode.Add;
    public SubjectMode CurrentMode
    {
        get => _currentMode;
        set
        {
            if (SetProperty(ref _currentMode, value))
            {
                OnPropertyChanged(nameof(IsUpdateMode));
                OnPropertyChanged(nameof(PageTitleText));
                OnPropertyChanged(nameof(SaveButtonText));
            }
        }
    }

    public bool IsUpdateMode => CurrentMode == SubjectMode.Update;
    public string PageTitleText => IsUpdateMode ? "تعديل المادة" : "إضافة مادة جديدة";
    public string SaveButtonText => IsUpdateMode ? "تحديث المادة" : "حفظ المادة";

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public ICommand SaveSubjectCommand { get; }
    public ICommand CancelCommand { get; }

    public AddUpdateSubjectViewModel(SubjectService subjectService, int? subjectId = null)
    {
        _subjectService = subjectService;

        SaveSubjectCommand = new Command(async () => await OnSaveSubjectAsync());
        CancelCommand = new Command(async () => await OnCancelAsync());

        if (subjectId.HasValue && subjectId.Value > 0)
        {
            SubjectId = subjectId.Value;
        }
        else
        {
            CurrentMode = SubjectMode.Add;
        }
    }

    // Apply query parameters passed via Shell navigation
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query == null) return;

        if (query.TryGetValue(nameof(SubjectId), out var rawId) && rawId != null)
        {
            if (rawId is int intId)
            {
                SubjectId = intId;
            }
            else if (int.TryParse(rawId.ToString(), out int parsedId))
            {
                SubjectId = parsedId;
            }
        }
    }

    // Load existing subject details when editing
    public async Task LoadSubjectForEditAsync(int subjectId)
    {
        if (subjectId <= 0)
        {
            CurrentMode = SubjectMode.Add;
            return;
        }

        try
        {
            CurrentMode = SubjectMode.Update;
            var subject = await _subjectService.GetSubjectByIdAsync(subjectId);
            if (subject == null) return;

            _editingSubject = subject;
            Name = subject.Name ?? string.Empty;
            Description = subject.Description ?? string.Empty;
        }
        catch
        {
            // Keep UI stable on load error
        }
    }

    // Validate and save new or updated subject
    private async Task OnSaveSubjectAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Name))
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تنبيه", "يرجى إدخال اسم المادة.", "موافق");
            }
            return;
        }

        string trimmedName = Name.Trim();
        string trimmedDescription = Description?.Trim() ?? string.Empty;

        if (trimmedName.Length > 150)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تنبيه", "اسم المادة يجب ألا يتجاوز 150 حرفاً.", "موافق");
            }
            return;
        }

        if (trimmedDescription.Length > 500)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تنبيه", "الوصف يجب ألا يتجاوز 500 حرف.", "موافق");
            }
            return;
        }

        try
        {
            IsBusy = true;

            if (_editingSubject != null && _editingSubject.Id > 0)
            {
                _editingSubject.Name = trimmedName;
                _editingSubject.Description = trimmedDescription;
                await _subjectService.SaveSubjectAsync(_editingSubject);
            }
            else
            {
                var newSubject = new Subject
                {
                    Id = 0,
                    Name = trimmedName,
                    Description = trimmedDescription
                };
                await _subjectService.SaveSubjectAsync(newSubject);
            }

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"تعذر حفظ المادة: {ex.Message}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Navigate back without saving
    private async Task OnCancelAsync()
    {
        if (IsBusy || Shell.Current == null) return;
        await Shell.Current.GoToAsync("..");
    }
}
