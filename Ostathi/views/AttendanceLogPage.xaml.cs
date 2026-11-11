using System;
using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class AttendanceLogPage : ContentPage
{
    public AttendanceLogPage(AttendanceLogPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
