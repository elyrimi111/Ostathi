using System;
using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class StudentAttendanceLogPage : ContentPage
{
    public StudentAttendanceLogPage(StudentAttendanceLogPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
