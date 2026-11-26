using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class GroupSubjectStudentsPage : ContentPage
{
    public GroupSubjectStudentsPage(GroupSubjectStudentsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is GroupSubjectStudentsViewModel vm)
        {
            await vm.LoadDataAsync();
        }
    }
}
