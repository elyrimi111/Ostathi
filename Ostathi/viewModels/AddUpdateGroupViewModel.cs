using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public class AddUpdateGroupViewModel : BaseViewModel
{
    private readonly GroupService _groupService;
    private readonly MajorService _majorService;

    private string _groupName = string.Empty;
    public string GroupName
    {
        get => _groupName;
        set => SetProperty(ref _groupName, value);
    }

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

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private bool _isLoadingMajors;
    public bool IsLoadingMajors
    {
        get => _isLoadingMajors;
        set => SetProperty(ref _isLoadingMajors, value);
    }

    public ICommand SaveGroupCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ImportStudentsCommand { get; }
    public ICommand RefreshMajorsCommand { get; }

    public AddUpdateGroupViewModel(GroupService groupService, MajorService majorService)
    {
        _groupService = groupService;
        _majorService = majorService;

        SaveGroupCommand = new Command(async () => await OnCreateGroupAsync(navigateBack: true));
        CancelCommand = new Command(async () => await OnCancelAsync());
        ImportStudentsCommand = new Command(async () => await OnImportStudentsAsync());
        RefreshMajorsCommand = new Command(async () => await LoadMajorsAsync());

        // Load available academic majors from SQLite on initialization
        _ = LoadMajorsAsync();
    }

    // Fetch majors list from the database without blocking group save actions
    public async Task LoadMajorsAsync()
    {
        if (IsLoadingMajors) return;

        try
        {
            IsLoadingMajors = true;
            var majors = await _majorService.GetMajorsAsync();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MajorsList.Clear();
                if (majors != null)
                {
                    foreach (var major in majors)
                    {
                        MajorsList.Add(major);
                    }
                }

                // Keep or restore selection if still present in MajorsList
                if (SelectedMajor != null)
                {
                    SelectedMajor = MajorsList.FirstOrDefault(m => m.Id == SelectedMajor.Id);
                }
            });
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayAlert("Error", $"Failed to load majors: {ex.Message}", "OK"));
            }
        }
        finally
        {
            IsLoadingMajors = false;
        }
    }

    // Validate inputs against GroupConfiguration constraints and persist a new Group
    private async Task<Group?> CreateGroupInternalAsync()
    {
        if (string.IsNullOrWhiteSpace(GroupName))
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please enter the group name", "OK");
            }
            return null;
        }

        if (SelectedMajor == null)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select a target major", "OK");
            }
            return null;
        }

        string trimmedName = GroupName.Trim();
        string? trimmedDescription = string.IsNullOrWhiteSpace(GroupDescription) ? null : GroupDescription.Trim();
        string? academicYear = string.IsNullOrWhiteSpace(SelectedAcademicYear) ? null : SelectedAcademicYear.Trim();

        // Enforce schema MaxLength limits defined in GroupConfiguration
        if (trimmedName.Length > 150)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Warning", "Group name must not exceed 150 characters", "OK");
            }
            return null;
        }

        if (trimmedDescription != null && trimmedDescription.Length > 500)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Warning", "Description must not exceed 500 characters", "OK");
            }
            return null;
        }

        if (academicYear != null && academicYear.Length > 100)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Warning", "Academic year must not exceed 100 characters", "OK");
            }
            return null;
        }

        try
        {
            // Keep Id at 0 so SQLite assigns the primary key automatically on insert
            var newGroup = new Group
            {
                Id = 0,
                Name = trimmedName,
                Description = trimmedDescription,
                AcademicYear = academicYear,
                MajorId = SelectedMajor.Id,
                RepresentativeStudentId = null
            };

            await _groupService.SaveGroupAsync(newGroup);

            // TODO: Re-enable group-added notification once MessengerService namespace/signature is confirmed
            return newGroup;
        }
        catch (Exception ex)
        {
            // Extract inner exception details to pinpoint the exact SQLite constraint error
            string details = ex.InnerException?.Message ?? ex.Message;

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to create group: {details}", "OK");
            }
            return null;
        }
    }

    // Create a new group and navigate back to the previous page
    private async Task OnCreateGroupAsync(bool navigateBack)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var createdGroup = await CreateGroupInternalAsync();
            if (createdGroup == null) return;

            if (navigateBack && Shell.Current != null)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Create the group first so a valid GroupId exists before opening ImportGroupPage
    private async Task OnImportStudentsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var createdGroup = await CreateGroupInternalAsync();
            if (createdGroup == null || createdGroup.Id <= 0) return;

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync($"{nameof(ImportGroupPage)}?GroupId={createdGroup.Id}");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Navigate back without creating a group
    private async Task OnCancelAsync()
    {
        if (IsBusy || Shell.Current == null) return;
        await Shell.Current.GoToAsync("..");
    }
}
