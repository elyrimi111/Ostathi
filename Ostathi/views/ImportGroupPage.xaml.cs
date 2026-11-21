using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class ImportGroupPage : ContentPage
{
    public ImportGroupPage(ImportGroupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
