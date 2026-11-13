using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class StudentDetailsPage : ContentPage
{
    public StudentDetailsPage(StudentDetailsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
