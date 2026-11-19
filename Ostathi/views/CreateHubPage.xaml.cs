using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class CreateHubPage : ContentPage
{
    public CreateHubPage()
    {
        InitializeComponent();
        BindingContext = new CreateHubViewModel();
    }
}
