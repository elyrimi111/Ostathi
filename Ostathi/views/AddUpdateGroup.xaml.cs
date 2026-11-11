using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class AddUpdateGroup : ContentPage
{
    // Constructor injecting AddUpdateGroupViewModel via Dependency Injection
    public AddUpdateGroup(AddUpdateGroupViewModel viewModel)
    {
        InitializeComponent();
        
        // ربط الصفحة بالـ ViewModel الخاص بها
        BindingContext = viewModel;
    }
}
