using Microsoft.Maui.Controls;
using Ostathi.ViewModels;

namespace Ostathi;

public partial class GroupDashboardView : ContentView
{
    public GroupDashboardView(GroupDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
