using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class MajorsPage : ContentPage
{
    // Constructor injecting MajorsViewMod via Dependency Injection
    public MajorsPage(MajorsViewMod viewModel)
    {
        InitializeComponent();
        
        // Bind the page to its corresponding ViewModel
        BindingContext = viewModel;
    }
}
