using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;

namespace Ostathi;

public class GroupImportedStudentItem : BaseViewModel
{
    public int StudentId { get; set; }

    private int _index;
    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }

    public string DisplayName { get; set; } = string.Empty;
    public string AcademicId { get; set; } = string.Empty;

    private bool _isRepresentative;
    public bool IsRepresentative
    {
        get => _isRepresentative;
        set => SetProperty(ref _isRepresentative, value);
    }
}

[QueryProperty(nameof(GroupId), nameof(GroupId))]
public class AddUpdateGroupViewModel : BaseViewModel, IQueryAttributable
{
    public enum GroupMode
    {
        Add,
        Update
    }

    private readonly GroupService _groupService;
    private readonly MajorService _majorService;

    private Group? _createdGroup;
    private int? _representativeStudentId;

    private int? _groupId;
    public int? GroupId
    {
        get => _groupId;
        set
        {
            if (SetProperty(ref _groupId, value))
            {
                if (_groupId.HasValue && _groupId.Value > 0)
                {
                    CurrentMode = GroupMode.Update;
                    _ = InitializeEditModeAsync(_groupId.Value);
                }
                else
                {
                    CurrentMode = GroupMode.Add;
                    _createdGroup = null;
                    _representativeStudentId = null;
                    OnPropertyChanged(nameof(RepresentativeSummaryText));
                }
            }
        }
    }

    private GroupMode _currentMode = GroupMode.Add;
    public GroupMode CurrentMode
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

    public bool IsUpdateMode => CurrentMode == GroupMode.Update;
    public string PageTitleText => IsUpdateMode ? "تعديل المجموعة" : "إضافة مجموعة جديدة";
    public string SaveButtonText => IsUpdateMode ? "تحديث المجموعة" : "حفظ المجموعة";

    public string RepresentativeSummaryText
    {
        get
        {
            var rep = ImportedStudents.FirstOrDefault(s => s.StudentId == _representativeStudentId);
            return rep != null ? $"المندوب: {rep.DisplayName}" : "المندوب: غير محدد";
        }
    }

    private string _groupNumber = string.Empty;
    public string GroupNumber
    {
        get => _groupNumber;
        set
        {
            if (SetProperty(ref _groupNumber, value))
            {
                OnPropertyChanged(nameof(GroupName));
            }
        }
    }

    // Enabled only after an academic year is selected
    public bool IsGroupNumberEnabled => !string.IsNullOrWhiteSpace(SelectedAcademicYear);

    // Calculated internally from SelectedMajor, SelectedAcademicYear, and GroupNumber
    public string GroupName => BuildGroupName();

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
        set
        {
            if (SetProperty(ref _selectedMajor, value))
            {
                OnPropertyChanged(nameof(IsAcademicYearEnabled));
                UpdateAcademicYearsForSelectedMajor();
                OnPropertyChanged(nameof(GroupName));
            }
        }
    }

    // Enabled only after a major is selected
    public bool IsAcademicYearEnabled => SelectedMajor != null;

    private ObservableCollection<string> _academicYearsList = new();
    public ObservableCollection<string> AcademicYearsList
    {
        get => _academicYearsList;
        set => SetProperty(ref _academicYearsList, value);
    }

    private string? _selectedAcademicYear;
    public string? SelectedAcademicYear
    {
        get => _selectedAcademicYear;
        set
        {
            if (SetProperty(ref _selectedAcademicYear, value))
            {
                if (string.IsNullOrWhiteSpace(_selectedAcademicYear))
                {
                    GroupNumber = string.Empty;
                }

                OnPropertyChanged(nameof(IsGroupNumberEnabled));
                OnPropertyChanged(nameof(GroupName));
            }
        }
    }

    public ObservableCollection<GroupImportedStudentItem> ImportedStudents { get; } = new();

    public bool HasImportedStudents => ImportedStudents.Count > 0;
    public string StudentsCountText => $"الطلاب المستوردون ({ImportedStudents.Count})";

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
    public ICommand DeleteImportedStudentCommand { get; }
    public ICommand SelectImportedStudentCommand { get; }
    public ICommand SetRepresentativeCommand { get; }
    public ICommand StudentOptionsCommand { get; }

    public AddUpdateGroupViewModel(GroupService groupService, MajorService majorService, int? groupId = null)
    {
        _groupService = groupService;
        _majorService = majorService;

        ImportedStudents.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasImportedStudents));
            OnPropertyChanged(nameof(StudentsCountText));
            OnPropertyChanged(nameof(RepresentativeSummaryText));
        };

        SaveGroupCommand = new Command(async () => await OnCreateGroupAsync(navigateBack: true));
        CancelCommand = new Command(async () => await OnCancelAsync());
        ImportStudentsCommand = new Command(async () => await OnImportStudentsAsync());
        RefreshMajorsCommand = new Command(async () => await LoadMajorsAsync());
        DeleteImportedStudentCommand = new Command<GroupImportedStudentItem>(async (item) => await OnDeleteImportedStudentAsync(item));
        SelectImportedStudentCommand = new Command<GroupImportedStudentItem>(async (item) => await OnSelectImportedStudentAsync(item));
        SetRepresentativeCommand = new Command<GroupImportedStudentItem>(async (item) => await OnSetRepresentativeAsync(item));
        StudentOptionsCommand = new Command<GroupImportedStudentItem>(async (item) => await OnStudentOptionsAsync(item));

        _ = LoadMajorsAsync();

        if (groupId.HasValue && groupId.Value > 0)
        {
            GroupId = groupId.Value;
        }
        else
        {
            CurrentMode = GroupMode.Add;
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query == null) return;

        if (query.TryGetValue(nameof(GroupId), out var rawGroupId) && rawGroupId != null)
        {
            if (rawGroupId is int intId)
            {
                GroupId = intId;
            }
            else if (int.TryParse(rawGroupId.ToString(), out int parsedId))
            {
                GroupId = parsedId;
            }
        }
    }

    private async Task OnStudentOptionsAsync(GroupImportedStudentItem? item)
    {
        if (item == null || item.StudentId <= 0 || Shell.Current == null)
            return;

        string repAction = item.IsRepresentative ? "إلغاء تعيين المندوب" : "تعيين كمندوب للمجموعة";
        const string openDetailsAction = "فتح ملف الطالب";
        const string deleteAction = "حذف الطالب";

        string? choice = await Shell.Current.DisplayActionSheet(
            item.DisplayName,
            "إلغاء",
            deleteAction,
            openDetailsAction,
            repAction);

        if (string.IsNullOrWhiteSpace(choice) || choice == "إلغاء")
            return;

        if (choice == openDetailsAction)
        {
            await OnSelectImportedStudentAsync(item);
        }
        else if (choice == repAction)
        {
            await OnSetRepresentativeAsync(item);
        }
        else if (choice == deleteAction)
        {
            await OnDeleteImportedStudentAsync(item);
        }
    }

    private async Task OnSetRepresentativeAsync(GroupImportedStudentItem? item)
    {
        if (item == null || item.StudentId <= 0 || IsBusy)
            return;

        int targetGroupId = _createdGroup?.Id > 0 ? _createdGroup.Id : (GroupId ?? 0);
        if (targetGroupId <= 0)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تنبيه", "يرجى حفظ المجموعة أولاً قبل تحديد المندوب.", "موافق");
            }
            return;
        }

        try
        {
            IsBusy = true;

            int? newRepId = (_representativeStudentId == item.StudentId) ? null : item.StudentId;
            await _groupService.SetGroupRepresentativeAsync(targetGroupId, newRepId);

            _representativeStudentId = newRepId;
            if (_createdGroup != null)
            {
                _createdGroup.RepresentativeStudentId = newRepId;
            }

            foreach (var student in ImportedStudents)
            {
                student.IsRepresentative = _representativeStudentId.HasValue &&
                                         student.StudentId == _representativeStudentId.Value;
            }

            OnPropertyChanged(nameof(RepresentativeSummaryText));
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"تعذر تحديد المندوب: {ex.Message}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnSelectImportedStudentAsync(GroupImportedStudentItem? item)
    {
        if (item == null || item.StudentId <= 0 || Shell.Current == null)
            return;

        await Shell.Current.GoToAsync($"{nameof(StudentDetailsPage)}?StudentId={item.StudentId}");
    }

    private async Task InitializeEditModeAsync(int groupId)
    {
        if (MajorsList.Count == 0)
        {
            await LoadMajorsAsync();
        }

        await LoadGroupForEditAsync(groupId);
    }

    // Load existing group data when opening the page in Update mode
    public async Task LoadGroupForEditAsync(int groupId)
    {
        if (groupId <= 0)
        {
            CurrentMode = GroupMode.Add;
            return;
        }

        try
        {
            CurrentMode = GroupMode.Update;

            using var dbContext = new AppDbContext();
            var group = await dbContext.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return;

            _createdGroup = group;
            _representativeStudentId = group.RepresentativeStudentId;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SelectedMajor = MajorsList.FirstOrDefault(m => m.Id == group.MajorId);
                SelectedAcademicYear = group.AcademicYear;
                GroupDescription = group.Description ?? string.Empty;

                var lastSpace = group.Name?.LastIndexOf(' ');
                GroupNumber = (lastSpace.HasValue && lastSpace.Value >= 0)
                    ? group.Name![(lastSpace.Value + 1)..].Trim()
                    : string.Empty;

                OnPropertyChanged(nameof(RepresentativeSummaryText));
            });

            await LoadImportedStudentsAsync();
        }
        catch
        {
            // Keep UI stable if loading group fails
        }
    }

    // Load imported students belonging to the created/edited group when returning via OnAppearing
    public async Task LoadImportedStudentsAsync()
    {
        int targetGroupId = _createdGroup?.Id > 0 ? _createdGroup.Id : (GroupId ?? 0);
        if (targetGroupId <= 0)
            return;

        try
        {
            using var dbContext = new AppDbContext();

            _representativeStudentId = await dbContext.Groups
                .Where(g => g.Id == targetGroupId)
                .Select(g => g.RepresentativeStudentId)
                .FirstOrDefaultAsync();

            var students = await dbContext.Students
                .Where(s => s.GroupId == targetGroupId)
                .OrderBy(s => s.Id)
                .ToListAsync();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                ImportedStudents.Clear();
                int idx = 1;
                foreach (var s in students)
                {
                    var parts = new[] { s.FirstName, s.SecondName, s.ThirdName, s.LastName }
                        .Where(p => !string.IsNullOrWhiteSpace(p) && p.Trim() != "-")
                        .Select(p => p.Trim());

                    ImportedStudents.Add(new GroupImportedStudentItem
                    {
                        StudentId = s.Id,
                        Index = idx++,
                        DisplayName = string.Join(" ", parts),
                        AcademicId = s.AcademicId,
                        IsRepresentative = _representativeStudentId.HasValue && s.Id == _representativeStudentId.Value
                    });
                }

                OnPropertyChanged(nameof(RepresentativeSummaryText));
            });
        }
        catch
        {
            // Keep UI stable if loading students fails
        }
    }

    private async Task OnDeleteImportedStudentAsync(GroupImportedStudentItem? item)
    {
        if (item == null || item.StudentId <= 0) return;

        try
        {
            using var dbContext = new AppDbContext();
            var student = await dbContext.Students.FirstOrDefaultAsync(s => s.Id == item.StudentId);
            if (student != null)
            {
                dbContext.Students.Remove(student);
                await dbContext.SaveChangesAsync();
            }

            if (_representativeStudentId == item.StudentId)
            {
                int targetGroupId = _createdGroup?.Id > 0 ? _createdGroup.Id : (GroupId ?? 0);
                if (targetGroupId > 0)
                {
                    await _groupService.SetGroupRepresentativeAsync(targetGroupId, null);
                }
                _representativeStudentId = null;
            }

            ImportedStudents.Remove(item);
            for (int i = 0; i < ImportedStudents.Count; i++)
            {
                ImportedStudents[i].Index = i + 1;
            }

            OnPropertyChanged(nameof(RepresentativeSummaryText));
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"تعذر حذف الطالب: {ex.Message}", "موافق");
            }
        }
    }

    // Build formatted group name: e.g. "نظم معلومات سنة ثانية مجموعة 1"
    private string BuildGroupName()
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(SelectedMajor?.Name))
        {
            sb.Append(SelectedMajor.Name.Trim());
        }

        string formattedYear = FormatAcademicYearForName(SelectedAcademicYear);
        if (!string.IsNullOrWhiteSpace(formattedYear))
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(formattedYear);
        }

        if (!string.IsNullOrWhiteSpace(GroupNumber))
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append($"مجموعة {GroupNumber.Trim()}");
        }

        return sb.ToString();
    }

    private static string FormatAcademicYearForName(string? selectedYear)
    {
        if (string.IsNullOrWhiteSpace(selectedYear))
            return string.Empty;

        string clean = selectedYear.Trim();

        // Remove trailing "(سنة التخرج)" if present
        int parenIndex = clean.IndexOf('(');
        if (parenIndex >= 0)
        {
            clean = clean[..parenIndex].Trim();
        }

        return clean switch
        {
            "السنة الأولى" => "سنة أولى",
            "السنة الثانية" => "سنة ثانية",
            "السنة الثالثة" => "سنة ثالثة",
            "السنة الرابعة" => "سنة رابعة",
            "السنة الخامسة" => "سنة خامسة",
            _ when clean.StartsWith("السنة ") => "سنة " + clean["السنة ".Length..].Trim(),
            _ => clean
        };
    }

    // Build academic years list based on SelectedMajor.DurationYears
    private void UpdateAcademicYearsForSelectedMajor()
    {
        SelectedAcademicYear = null;
        AcademicYearsList.Clear();

        if (SelectedMajor == null || SelectedMajor.DurationYears <= 0)
            return;

        for (int i = 1; i <= SelectedMajor.DurationYears; i++)
        {
            if (i == SelectedMajor.DurationYears && SelectedMajor.DurationYears > 1)
                AcademicYearsList.Add($"السنة {i} (سنة التخرج)");
            else
                AcademicYearsList.Add(FormatYearLabel(i));
        }
    }

    private static string FormatYearLabel(int yearNumber) => yearNumber switch
    {
        1 => "السنة الأولى",
        2 => "السنة الثانية",
        3 => "السنة الثالثة",
        4 => "السنة الرابعة",
        5 => "السنة الخامسة",
        _ => $"السنة {yearNumber}"
    };

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

    // Validate inputs against GroupConfiguration constraints and persist a Group in Add or Update mode
    private async Task<Group?> CreateGroupInternalAsync()
    {
        if (SelectedMajor == null)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select a target major", "OK");
            }
            return null;
        }

        if (string.IsNullOrWhiteSpace(SelectedAcademicYear))
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select an academic year", "OK");
            }
            return null;
        }

        if (string.IsNullOrWhiteSpace(GroupNumber))
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please enter the group number", "OK");
            }
            return null;
        }

        string generatedName = GroupName.Trim();
        string? trimmedDescription = string.IsNullOrWhiteSpace(GroupDescription) ? null : GroupDescription.Trim();
        string academicYear = SelectedAcademicYear.Trim();

        // Enforce schema MaxLength limits defined in GroupConfiguration
        if (generatedName.Length > 150)
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

        if (academicYear.Length > 100)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Warning", "Academic year must not exceed 100 characters", "OK");
            }
            return null;
        }

        try
        {
            if (_createdGroup != null && _createdGroup.Id > 0)
            {
                _createdGroup.Name = generatedName;
                _createdGroup.Description = trimmedDescription;
                _createdGroup.AcademicYear = academicYear;
                _createdGroup.MajorId = SelectedMajor.Id;
                _createdGroup.RepresentativeStudentId = _representativeStudentId;

                await _groupService.SaveGroupAsync(_createdGroup);
                CurrentMode = GroupMode.Update;
                return _createdGroup;
            }

            // Keep Id at 0 so SQLite assigns the primary key automatically on insert
            var newGroup = new Group
            {
                Id = 0,
                Name = generatedName,
                Description = trimmedDescription,
                AcademicYear = academicYear,
                MajorId = SelectedMajor.Id,
                RepresentativeStudentId = _representativeStudentId
            };

            await _groupService.SaveGroupAsync(newGroup);
            _createdGroup = newGroup;
            _groupId = newGroup.Id;
            OnPropertyChanged(nameof(GroupId));
            CurrentMode = GroupMode.Update;

            return _createdGroup;
        }
        catch (Exception ex)
        {
            // Extract inner exception details to pinpoint the exact SQLite constraint error
            string details = ex.InnerException?.Message ?? ex.Message;

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save group: {details}", "OK");
            }
            return null;
        }
    }

    // Create or update a group and navigate back to the previous page
    private async Task OnCreateGroupAsync(bool navigateBack)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var savedGroup = await CreateGroupInternalAsync();
            if (savedGroup == null) return;

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

    // Ensure a valid GroupId exists before opening ImportGroupPage
    private async Task OnImportStudentsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var group = await CreateGroupInternalAsync();
            if (group == null || group.Id <= 0) return;

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync($"{nameof(ImportGroupPage)}?GroupId={group.Id}");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Navigate back without leaving unsaved changes
    private async Task OnCancelAsync()
    {
        if (IsBusy || Shell.Current == null) return;
        await Shell.Current.GoToAsync("..");
    }
}
