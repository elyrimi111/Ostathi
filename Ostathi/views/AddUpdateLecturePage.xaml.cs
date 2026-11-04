using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class AddUpdateLecturePage : ContentPage
{
    public AddUpdateLecturePage(AddUpdateLectureViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
