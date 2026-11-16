using Ostathi.ViewModels;

namespace Ostathi;

public partial class LectureDetailsPage : ContentPage
{
    public LectureDetailsPage(LectureDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
