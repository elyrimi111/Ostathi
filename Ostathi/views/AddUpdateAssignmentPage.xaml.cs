namespace Ostathi;

public partial class AddUpdateAssignmentPage : ContentPage
{
    public AddUpdateAssignmentPage(AddUpdateAssignmentViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
