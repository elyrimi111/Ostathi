using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class ScheduleManagementPage : ContentPage
{
    public ScheduleManagementPage(ScheduleManagementViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
