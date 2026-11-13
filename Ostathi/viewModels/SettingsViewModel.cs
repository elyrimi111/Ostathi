using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class SettingsViewModel : BindableObject
{
    // Command for navigating to the Academic Settings page
    public ICommand OpenAcademicSettingsCommand { get; }

    // Command for navigating to the Schedule Management page
    public ICommand OpenScheduleManagementCommand { get; }

    // Command for navigating to the Majors Management page
    public ICommand OpenMajorsPageCommand { get; }

    // Command for navigating to the About page
    public ICommand OpenAboutPageCommand { get; }

    // Command for handling user logout
    public ICommand LogoutCommand { get; }

    public SettingsViewModel()
    {
        // Initialize navigation & action commands
        OpenAcademicSettingsCommand = new Command(async () => await OnOpenAcademicSettingsExecuted());
        OpenScheduleManagementCommand = new Command(async () => await OnOpenScheduleManagementExecuted());
        OpenMajorsPageCommand = new Command(async () => await OnOpenMajorsPageExecuted());
        OpenAboutPageCommand = new Command(async () => await OnOpenAboutPageExecuted());
        LogoutCommand = new Command(async () => await OnLogoutExecuted());
    }

    // Handles navigation to AcademicSettingsPage
    private async Task OnOpenAcademicSettingsExecuted()
    {
        await Shell.Current.GoToAsync(nameof(AcademicSettingsPage));
    }

    // Handles navigation to ScheduleManagementPage
    private async Task OnOpenScheduleManagementExecuted()
    {
        await Shell.Current.GoToAsync(nameof(ScheduleManagementPage));
    }

    // Handles navigation to MajorsPage
    private async Task OnOpenMajorsPageExecuted()
    {
        await Shell.Current.GoToAsync(nameof(MajorsPage));
    }

    // Handles navigation to AboutPage
    private async Task OnOpenAboutPageExecuted()
    {
        await Shell.Current.GoToAsync(nameof(AboutPage));
    }

    // Handles Logout action
    private async Task OnLogoutExecuted()
    {
        bool confirm = await Shell.Current.DisplayAlert("تسجيل الخروج", "هل أنت متأكد من رغبتك في تسجيل الخروج من النظام؟", "نعم", "إلغاء");
        if (confirm)
        {
            await Shell.Current.DisplayAlert("تنبيه", "تم تسجيل الخروج بنجاح", "حسناً");
        }
    }
}
