namespace Ostathi;

public partial class AddUpdateStudent : ContentPage
{
    public AddUpdateStudent(AddaUpdateStudentsViewModel viewModel)
    {
        InitializeComponent();
        
        BindingContext = viewModel;
    }
}
