namespace Ostathi;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage(NotificationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
