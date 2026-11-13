namespace Ostathi;

public partial class ManageSingleGroupSubjectView : ContentPage
{
    public ManageSingleGroupSubjectView(ManageSingleGroupSubjectViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
