using System;
using Microsoft.Maui.Controls;
using Ostathi.ViewModels;

namespace Ostathi;

public partial class GroupSubjectsManagementView : ContentView
{
    private readonly GroupSubjectsManagementViewModel _viewModel;

    public GroupSubjectsManagementView(GroupSubjectsManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        Loaded += OnViewLoaded;
    }

    // Refresh data whenever the view is loaded or shown
    private async void OnViewLoaded(object? sender, EventArgs e)
    {
        await _viewModel.LoadInitialDataAsync();
    }
}
