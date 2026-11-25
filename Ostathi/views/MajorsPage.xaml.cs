using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class MajorsPage : ContentPage
{
    public MajorsPage(MajorsViewMod viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MajorsViewMod viewModel)
        {
            await viewModel.OnAppearingAsync();
        }
    }
}
