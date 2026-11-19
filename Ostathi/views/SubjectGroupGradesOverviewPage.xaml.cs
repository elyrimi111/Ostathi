using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class SubjectGroupGradesOverviewPage : ContentPage
{
    public SubjectGroupGradesOverviewPage(SubjectGroupGradesOverviewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
