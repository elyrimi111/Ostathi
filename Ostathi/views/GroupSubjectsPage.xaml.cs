using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class GroupSubjectsPage : ContentPage
{
    // Constructor injecting GroupSubjectsPageViewModel via Dependency Injection
    public GroupSubjectsPage(GroupSubjectsPageViewModel viewModel)
    {
        InitializeComponent();

        // ربط الصفحة بالـ ViewModel الخاص بها
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is GroupSubjectsPageViewModel vm)
        {
            await vm.LoadDataAsync();
        }
    }
}
