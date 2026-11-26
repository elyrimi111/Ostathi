using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public class GroupDisplayItem
{
    public Group Group { get; }
    public int Id => Group.Id;
    public string Name => Group.Name;
    public Major? Major => Group.Major;
    public string MajorName => Group.Major?.Name ?? "غير محدد";
    public string MajorDurationText { get; }

    public GroupDisplayItem(Group group)
    {
        Group = group;
        MajorDurationText = FormatDurationYears(group.Major?.DurationYears);
    }

    public static string FormatDurationYears(int? years)
    {
        if (!years.HasValue || years.Value <= 0)
            return "عدد السنوات: غير محدد";

        return years.Value switch
        {
            1 => "سنة دراسية واحدة",
            2 => "سنتان دراسيتان",
            >= 3 and <= 10 => $"{years.Value} سنوات دراسية",
            _ => $"{years.Value} سنة دراسية"
        };
    }
}

public class GroupsViewModel : BaseViewModel
{
    // Service responsible for managing groups in the database.
    private readonly GroupService _groupService;

    // Collection containing all groups displayed in the CollectionView.
    private ObservableCollection<GroupDisplayItem> _groupsList = new();

    public ObservableCollection<GroupDisplayItem> GroupsList
    {
        get => _groupsList;
        set => SetProperty(ref _groupsList, value);
    }

    #region Commands

    // Command for navigating to the Add Group page.
    public ICommand AddGroupCommand { get; }

    // Command for navigating to the Edit Group page.
    public ICommand EditGroupCommand { get; }

    // Command for loading all groups.
    public ICommand LoadGroupsCommand { get; }

    #endregion

    // Constructor receives GroupService through Dependency Injection.
    public GroupsViewModel(GroupService groupService)
    {
        _groupService = groupService;

        Title = "إدارة المجموعات";

        // Initialize commands.
        AddGroupCommand = new Command(async () => await OnAddGroupAsync());

        EditGroupCommand = new Command<GroupDisplayItem>(
            async (item) => await OnEditGroupAsync(item));

        LoadGroupsCommand = new Command(
            async () => await LoadGroupsAsync());
    }

    // Called when GroupsPage appears on screen.
    public async Task OnAppearingAsync()
    {
        await LoadGroupsAsync();
    }

    // Retrieve all groups from the database.
    public async Task LoadGroupsAsync()
    {
        // Prevent multiple loading operations simultaneously.
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            // Retrieve groups with their related Major and Representative Student.
            var groups = await _groupService.GetGroupsAsync();

            // Update the collection on the main UI thread.
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                GroupsList.Clear();
                foreach (var group in groups)
                {
                    GroupsList.Add(new GroupDisplayItem(group));
                }
            });
        }
        catch (Exception ex)
        {
            // Display an error message if loading fails.
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    $"Failed to load groups: {ex.Message}",
                    "OK");
            }
        }
        finally
        {
            // Reset the loading state.
            IsBusy = false;
        }
    }

    // Navigate to AddUpdateGroup in add mode.
    private async Task OnAddGroupAsync()
    {
        if (Shell.Current == null)
            return;

        await Shell.Current.GoToAsync(
            nameof(AddUpdateGroup));
    }

    // Navigate to AddUpdateGroup in edit mode.
    private async Task OnEditGroupAsync(GroupDisplayItem? item)
    {
        if (item == null || Shell.Current == null)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(AddUpdateGroup)}?GroupId={item.Id}");
    }
}
