using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi;

[QueryProperty(nameof(GroupId), "GroupId")]
public class AddUpdateGroupViewModel : BaseViewModel
{
    private readonly GroupService _groupService;
    private readonly MajorService _majorService;

    private string _groupId = string.Empty;
    public string GroupId
    {
        get => _groupId;
        set
        {
            if (SetProperty(ref _groupId, value))
            {
                // Load group details for editing if GroupId is provided
                _ = LoadGroupDetailsAsync();
            }
        }
    }

    private int _editingGroupId = 0;
    private int? _pendingMajorId = null;

    // Subject / Course Name Property
    private string _subjectName = string.Empty;
    public string SubjectName
    {
        get => _subjectName;
        set => SetProperty(ref _subjectName, value);
    }

    // Group Name Property
    private string _groupName = string.Empty;
    public string GroupName
    {
        get => _groupName;
        set => SetProperty(ref _groupName, value);
    }

    // Group Description or Notes Property
    private string _groupDescription = string.Empty;
    public string GroupDescription
    {
        get => _groupDescription;
        set => SetProperty(ref _groupDescription, value);
    }

    private ObservableCollection<Major> _majorsList = new();
    public ObservableCollection<Major> MajorsList
    {
        get => _majorsList;
        set => SetProperty(ref _majorsList, value);
    }

    private Major? _selectedMajor;
    public Major? SelectedMajor
    {
        get => _selectedMajor;
        set => SetProperty(ref _selectedMajor, value);
    }

    private ObservableCollection<string> _academicYearsList = new()
    {
        "السنة الأولى",
        "السنة الثانية",
        "السنة الثالثة",
        "سنة التخرج"
    };
    public ObservableCollection<string> AcademicYearsList
    {
        get => _academicYearsList;
        set => SetProperty(ref _academicYearsList, value);
    }

    private string? _selectedAcademicYear;
    public string? SelectedAcademicYear
    {
        get => _selectedAcademicYear;
        set => SetProperty(ref _selectedAcademicYear, value);
    }

    #region Commands

    public ICommand SaveGroupCommand { get; }
    public ICommand CancelCommand { get; }

    #endregion

    // Constructor injecting services via Dependency Injection
    public AddUpdateGroupViewModel(GroupService groupService, MajorService majorService)
    {
        _groupService = groupService;
        _majorService = majorService;

        SaveGroupCommand = new Command(async () => await OnSaveGroupAsync());
        CancelCommand = new Command(async () => await OnCancelAsync());

        // Load available majors from database on initialization
        _ = LoadMajorsAsync();
    }

    // Fetch majors list from SQLite database
    private async Task LoadMajorsAsync()
    {
        try
        {
            var majors = await _majorService.GetMajorsAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MajorsList.Clear();
                if (majors != null)
                {
                    foreach (var major in majors)
                    {
                        MajorsList.Add(major);
                    }
                }

                // Assign selected major after populating list if in edit mode
                if (_pendingMajorId.HasValue && MajorsList.Any())
                {
                    SelectedMajor = MajorsList.FirstOrDefault(m => m.Id == _pendingMajorId.Value);
                    _pendingMajorId = null;
                }
            });
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load majors: {ex.Message}", "OK");
            }
        }
    }

    // Populate fields when editing an existing group
    private async Task LoadGroupDetailsAsync()
    {
        if (int.TryParse(GroupId, out int id) && id > 0)
        {
            _editingGroupId = id;
            try
            {
                var group = await _groupService.GetGroupByIdAsync(id);
                if (group != null)
                {
                    SubjectName = group.SubjectName;
                    GroupName = group.Name;
                    GroupDescription = group.Description ?? string.Empty;
                    SelectedAcademicYear = group.AcademicYear;
                    
                    if (MajorsList.Any())
                    {
                        SelectedMajor = MajorsList.FirstOrDefault(m => m.Id == group.MajorId);
                    }
                    else
                    {
                        _pendingMajorId = group.MajorId;
                    }
                }
            }
            catch (Exception ex)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Error", $"Failed to load group details: {ex.Message}", "OK");
                }
            }
        }
    }

    // Create or update a group record
    private async Task OnSaveGroupAsync()
    {
        if (string.IsNullOrWhiteSpace(SubjectName))
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please enter the subject name", "OK");
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(GroupName))
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please enter the group name", "OK");
            }
            return;
        }

        if (SelectedMajor == null)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select a target major", "OK");
            }
            return;
        }

        try
        {
            var groupToSave = new Group
            {
                Id = _editingGroupId,
                SubjectName = SubjectName.Trim(),
                Name = GroupName.Trim(),
                Description = GroupDescription?.Trim(),
                AcademicYear = SelectedAcademicYear,
                MajorId = SelectedMajor.Id
            };

            await _groupService.SaveGroupAsync(groupToSave);

            // Notify subscriber ViewModels to refresh list on MainThread
            MessengerService.NotifyGroupAdded();

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save group: {ex.Message}", "OK");
            }
        }
    }

    private async Task OnCancelAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
