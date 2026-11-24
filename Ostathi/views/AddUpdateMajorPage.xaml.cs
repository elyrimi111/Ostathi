using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class AddUpdateMajorPage : ContentPage
{
    public AddUpdateMajorPage(AddUpdateMajorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
