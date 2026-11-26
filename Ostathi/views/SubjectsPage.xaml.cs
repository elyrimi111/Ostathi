using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class SubjectsPage : ContentPage
{
    private readonly SubjectsViewModel _viewModel;

    public SubjectsPage(SubjectsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    // Reload subjects list whenever the page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSubjectsAsync();
    }
}
