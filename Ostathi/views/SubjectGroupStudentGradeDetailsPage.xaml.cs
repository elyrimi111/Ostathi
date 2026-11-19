using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class SubjectGroupStudentGradeDetailsPage : ContentPage
{
    public SubjectGroupStudentGradeDetailsPage(SubjectGroupStudentGradeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
