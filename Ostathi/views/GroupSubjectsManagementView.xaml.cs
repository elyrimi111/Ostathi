using System;
using Microsoft.Maui.Controls;
using Ostathi.ViewModels;

namespace Ostathi;

public partial class GroupSubjectsManagementView : ContentView
{
    public GroupSubjectsManagementView(GroupSubjectsManagementViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
