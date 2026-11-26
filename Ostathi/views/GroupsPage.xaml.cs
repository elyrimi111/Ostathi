using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class GroupsPage : ContentPage
{
    public GroupsPage(GroupsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is GroupsViewModel viewModel)
        {
            await viewModel.OnAppearingAsync();
        }
    }
}
