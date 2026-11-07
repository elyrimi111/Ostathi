using Microsoft.Maui.Controls;
using Ostathi.viewModels;

namespace Ostathi;

public partial class MainLayoutView : ContentPage
{
    // Receive MainLayoutViewModel via dependency injection
    public MainLayoutView(MainLayoutViewModel viewModel)
    {
        InitializeComponent();
        
        // Bind the page to the injected ViewModel
        BindingContext = viewModel;
    }
}
