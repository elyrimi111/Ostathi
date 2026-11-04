using Ostathi.ViewModels;

namespace Ostathi;

public partial class groupsPage : ContentView
{
    public groupsPage()
    {
        InitializeComponent();
        
        BindingContext = new GroupsViewModel();
    }
}
