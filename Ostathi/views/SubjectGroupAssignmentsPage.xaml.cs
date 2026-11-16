using Ostathi.ViewModels;

namespace Ostathi;

public partial class SubjectGroupAssignmentsPage : ContentPage
{
    public SubjectGroupAssignmentsPage(SubjectGroupAssignmentsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
