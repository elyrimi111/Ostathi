namespace Ostathi;

public partial class ExportMasterPage : ContentPage
{
    public ExportMasterPage(ExportMasterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
