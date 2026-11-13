using Ostathi.ViewModels;

namespace Ostathi;

public partial class SubjectGroupStudentsAttendanceView : ContentPage
{
    public SubjectGroupStudentsAttendanceView(SubjectGroupStudentsAttendanceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
