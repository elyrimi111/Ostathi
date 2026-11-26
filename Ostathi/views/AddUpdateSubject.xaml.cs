using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class AddUpdateSubject : ContentPage
{
    public AddUpdateSubject(AddUpdateSubjectViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
