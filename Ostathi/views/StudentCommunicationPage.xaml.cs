using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class StudentCommunicationPage : ContentPage
{
    public StudentCommunicationPage(StudentCommunicationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
