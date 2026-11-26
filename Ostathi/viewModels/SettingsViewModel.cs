using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class SettingsViewModel : BaseViewModel
{
    private bool _isNavigating;

    // Command for navigating to the Academic Settings page
    public ICommand OpenAcademicSettingsCommand { get; }

    // Command for navigating to the Schedule Management page
    public ICommand OpenScheduleManagementCommand { get; }

    // Command for navigating to the Majors Management page
    public ICommand OpenMajorsPageCommand { get; }

    // Command for navigating to the Groups Management page
    public ICommand OpenGroupsPageCommand { get; }

    // Command for navigating to the Subjects Management page
    public ICommand OpenSubjectsPageCommand { get; }

    // Command for navigating to the About page
    public ICommand OpenAboutPageCommand { get; }

    // Command for handling user logout
    public ICommand LogoutCommand { get; }

    public SettingsViewModel()
    {
        // Initialize navigation & action commands
        OpenAcademicSettingsCommand = new Command(async () => await NavigateToAsync(nameof(AcademicSettingsPage)));
        OpenScheduleManagementCommand = new Command(async () => await NavigateToAsync(nameof(ScheduleManagementPage)));
        OpenMajorsPageCommand = new Command(async () => await NavigateToAsync(nameof(MajorsPage)));
        OpenGroupsPageCommand = new Command(async () => await NavigateToAsync(nameof(GroupsPage)));
        OpenSubjectsPageCommand = new Command(async () => await NavigateToAsync(nameof(SubjectsPage)));
        OpenAboutPageCommand = new Command(async () => await NavigateToAsync(nameof(AboutPage)));
        LogoutCommand = new Command(async () => await OnLogoutExecuted());
    }

    // Safely navigate to a registered Shell route while preventing duplicate taps
    private async Task NavigateToAsync(string route)
    {
        if (_isNavigating || IsBusy || Shell.Current == null) return;

        try
        {
            _isNavigating = true;
            IsBusy = true;
            await Shell.Current.GoToAsync(route);
        }
        finally
        {
            IsBusy = false;
            _isNavigating = false;
        }
    }

    // Handles Logout action
    private async Task OnLogoutExecuted()
    {
        if (_isNavigating || IsBusy || Shell.Current == null) return;

        try
        {
            _isNavigating = true;
            IsBusy = true;
            bool confirm = await Shell.Current.DisplayAlert(
                "تسجيل الخروج",
                "هل أنت متأكد من رغبتك في تسجيل الخروج من النظام؟",
                "نعم",
                "إلغاء"
            );

            if (confirm)
            {
                await Shell.Current.DisplayAlert("تنبيه", "تم تسجيل الخروج بنجاح", "حسناً");
            }
        }
        finally
        {
            IsBusy = false;
            _isNavigating = false;
        }
    }
}
