using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi.ViewModels
{
    // Receive the target GroupId parameter automatically via Shell navigation
    [QueryProperty(nameof(GroupId), "GroupId")]
    public class GroupDashboardViewModel : BaseViewModel
    {
        private readonly AppDbContext _dbContext;

        // Auto-bound GroupId property received via QueryProperty
        private int _groupId;
        public int GroupId
        {
            get => _groupId;
            set
            {
                if (_groupId != value)
                {
                    _groupId = value;
                    OnPropertyChanged();

                    if (_groupId > 0)
                    {
                        _ = LoadGroupAndDashboardDataAsync(_groupId);
                    }
                }
            }
        }

        // Active Group entity model
        private Group? _currentGroup;
        public Group? CurrentGroup
        {
            get => _currentGroup;
            set => SetProperty(ref _currentGroup, value);
        }

        // Display text for total student count bound to UI
        private string _studentCountText = "0 Students";
        public string StudentCountText
        {
            get => _studentCountText;
            set => SetProperty(ref _studentCountText, value);
        }

        #region Commands for Interactive Cards and Buttons

        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand OpenAttendanceCommand { get; }
        public ICommand OpenYearWorkCommand { get; }
        public ICommand OpenAssignmentsCommand { get; }
        public ICommand OpenNotificationsCommand { get; }
        public ICommand OpenLecturesCommand { get; }
        public ICommand OpenMidtermCommand { get; }
        public ICommand OpenFinalExamCommand { get; }
        public ICommand DeleteGroupCommand { get; }

        #endregion

        public GroupDashboardViewModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;

            // Initialize navigation commands
            ImportCommand = new Command(async () => await GoToAsync(nameof(ImportRosterPage)));
            ExportCommand = new Command(async () => await GoToAsync(nameof(ExportMasterPage)));
            
            // Dedicated method binding for OpenAttendanceCommand passing current GroupId
            OpenAttendanceCommand = new Command(OnOpenAttendanceExecuted);

            OpenYearWorkCommand = new Command(async () => await GoToAsync(nameof(YearWorkPage)));
            OpenAssignmentsCommand = new Command(async () => await GoToAsync(nameof(AssignmentsPage)));
            OpenNotificationsCommand = new Command(async () => await GoToAsync(nameof(NotificationsPage)));
            OpenLecturesCommand = new Command(async () => await GoToAsync(nameof(LecturesPage)));
            OpenMidtermCommand = new Command(async () => await GoToAsync(nameof(MidtermExamPage)));
            OpenFinalExamCommand = new Command(async () => await GoToAsync(nameof(FinalExamPage)));
            
            // Initialize Delete Group Command
            DeleteGroupCommand = new Command(async () => await OnDeleteGroupAsync());
        }

        #region Attendance Navigation Handlers

        // Synchronous command execution handler passing the active GroupId directly
        private async void OnOpenAttendanceExecuted()
        {
            await OpenAttendanceLogAsync(GroupId);
        }
        public async Task OpenAttendanceLogAsync(int targetGroupId)
        {
            if (targetGroupId <= 0)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Warning", "Please select a valid group first.", "OK");
                }
                return;
            }

            if (Shell.Current != null)
            {
                // الانتقال إلى شاشة إدارة الحضور والغياب للمجموعة مع تمرير الـ GroupId
                await Shell.Current.GoToAsync($"{nameof(GroupAttendanceManagementPage)}?GroupId={targetGroupId}");
            }
        }


        #endregion

        // Fetch group details and calculate student roster count based on the received GroupId
        private async Task LoadGroupAndDashboardDataAsync(int id)
        {
            try
            {
                IsBusy = true;

                // 1. Fetch group record from SQLite database by ID
                CurrentGroup = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == id);

                // 2. Count total students belonging strictly to this group
                int count = await _dbContext.Students.CountAsync(s => s.GroupId == id);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StudentCountText = $"{count} Students";
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StudentCountText = "0 Students";
                });

                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Error", $"Failed to load dashboard data: {ex.Message}", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Handles the delete group operation with user confirmation prompt
        private async Task OnDeleteGroupAsync()
        {
            if (_currentGroup == null || Shell.Current == null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select a target group to delete first.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Delete Confirmation",
                $"Are you sure you want to delete the group '{_currentGroup.Name}'? All associated records will be permanently removed.",
                "Yes, Delete",
                "Cancel"
            );

            if (!confirm) return;

            try
            {
                IsBusy = true;

                // Remove group entity from database asynchronously
                _dbContext.Groups.Remove(_currentGroup);
                await _dbContext.SaveChangesAsync();

                // Clear active group references immediately
                _currentGroup = null;
                _groupId = 0;

                // Broadcast event to notify other ViewModels of group deletion
                MessengerService.NotifyGroupDeleted();

                await Shell.Current.DisplayAlert("Success", "Group has been deleted successfully.", "OK");

                // Navigate back to previous page
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete group: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Helper method to navigate to sub-pages while passing the active GroupId parameter
        protected virtual async Task GoToAsync(string pageName)
        {
            if (GroupId <= 0)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Warning", "Please select a group first", "OK");
                }
                return;
            }

            if (Shell.Current != null)
            {
                // Forward the active GroupId to child pages via QueryProperty
                await Shell.Current.GoToAsync($"{pageName}?GroupId={GroupId}");
            }
        }
    }
}
