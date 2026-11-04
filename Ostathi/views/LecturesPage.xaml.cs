using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class LecturesPage : ContentPage
{
    public LecturesPage(LecturesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
