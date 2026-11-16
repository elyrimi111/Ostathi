namespace Ostathi;

public partial class AssignmentDetailManagementPage : ContentPage
{
    public AssignmentDetailManagementPage(AssignmentDetailManagementViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
