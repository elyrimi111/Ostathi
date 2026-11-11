using System;
using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class GroupAttendanceManagementPage : ContentPage
{
    public GroupAttendanceManagementPage(GroupAttendanceManagementPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
