using System;
using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class AcademicSettingsPage : ContentPage
{
    public AcademicSettingsPage(AcademicSettingsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
