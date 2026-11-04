namespace Ostathi;

public partial class HomePage : ContentView
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
