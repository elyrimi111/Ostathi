using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class SettingsViewModel : BindableObject
{
    public ICommand OpenScheduleManagementCommand { get; }

    public SettingsViewModel()
    {
        OpenScheduleManagementCommand = new Command(async () => await OnOpenScheduleManagementExecuted());
    }

    private async Task OnOpenScheduleManagementExecuted()
    {
        await Shell.Current.GoToAsync(nameof(ScheduleManagementPage));
    }
}
