using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class SettingsViewModel : BindableObject
{
    // Command for navigating to the Schedule Management page
    public ICommand OpenScheduleManagementCommand { get; }

    // Command for navigating to the Majors Management page
    public ICommand OpenMajorsPageCommand { get; }

    public SettingsViewModel()
    {
        // Initialize navigation commands
        OpenScheduleManagementCommand = new Command(async () => await OnOpenScheduleManagementExecuted());
        OpenMajorsPageCommand = new Command(async () => await OnOpenMajorsPageExecuted());
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
}
