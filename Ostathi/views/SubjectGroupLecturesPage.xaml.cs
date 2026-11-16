using Microsoft.Maui.Controls;
using Ostathi.ViewModels;

namespace Ostathi;

public partial class SubjectGroupLecturesPage : ContentPage
{
    public SubjectGroupLecturesPage(SubjectGroupLecturesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
