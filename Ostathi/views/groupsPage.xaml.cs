
using Ostathi.ViewModels;

namespace Ostathi;

public partial class groupsPage : ContentView
{
    public groupsPage(GroupsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
