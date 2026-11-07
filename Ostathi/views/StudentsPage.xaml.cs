namespace Ostathi;

public partial class StudentsPage : ContentView
{
    // Receive the ViewModel via constructor injection
    public StudentsPage(StudentsViewModel viewModel)
    {
        InitializeComponent();
        
        // Set the ViewModel as the BindingContext for the view
        BindingContext = viewModel;
    }
}
