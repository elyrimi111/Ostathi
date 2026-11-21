using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class AddGroupSubjectPage : ContentPage
{
    public AddGroupSubjectPage(AddUpdateGroupSubjectViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
