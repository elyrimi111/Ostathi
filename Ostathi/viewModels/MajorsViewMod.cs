using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public class MajorsViewMod : BaseViewModel
{
    // Service responsible for managing academic majors.
    private readonly MajorService _majorService;

    // Observable collection of majors bound to the UI CollectionView.
    private ObservableCollection<Major> _majorsList = new();

    // Gets or sets the list of majors and notifies the UI when it changes.
    public ObservableCollection<Major> MajorsList
    {
        get => _majorsList;
        set => SetProperty(ref _majorsList, value);
    }

    #region Commands

    // Command for adding a new major.
    public ICommand AddMajorCommand { get; }

    // Command for editing an existing major.
    public ICommand EditMajorCommand { get; }

    // Command for deleting a selected major.
    public ICommand DeleteMajorCommand { get; }

    // Command for loading all majors.
    public ICommand LoadMajorsCommand { get; }

    #endregion

    // Constructor that receives MajorService through Dependency Injection.
    public MajorsViewMod(MajorService majorService)
    {
        _majorService = majorService;

        // Set the page title.
        Title = "إدارة التخصصات الأكاديمية";

        // Initialize commands and connect them to their corresponding methods.
        AddMajorCommand = new Command(async () => await OnAddMajorAsync());

        EditMajorCommand = new Command<Major>(
            async (major) => await OnEditMajorAsync(major));

        DeleteMajorCommand = new Command<Major>(
            async (major) => await OnDeleteMajorAsync(major));

        LoadMajorsCommand = new Command(
            async () => await LoadMajorsAsync());
    }

    // Called when the associated page appears on screen.
    public async Task OnAppearingAsync()
    {
        await LoadMajorsAsync();
    }

    // Load all majors using MajorService.
    public async Task LoadMajorsAsync()
    {
        // Prevent multiple loading operations at the same time.
        if (IsBusy) return;

        try
        {
            // Indicate that the application is busy.
            IsBusy = true;

            // Retrieve all majors from the service.
            var majorsFromService = await _majorService.GetMajorsAsync();

            // Update the existing ObservableCollection on the main UI thread.
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MajorsList.Clear();
                foreach (var major in majorsFromService)
                {
                    MajorsList.Add(major);
                }
            });
        }
        catch (Exception ex)
        {
            // Display an error message if loading fails.
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    $"Failed to load majors: {ex.Message}",
                    "OK");
            }
        }
        finally
        {
            // Reset the busy state after the operation finishes.
            IsBusy = false;
        }
    }

    // Navigate to AddUpdateMajorPage in add mode.
    private async Task OnAddMajorAsync()
    {
        if (Shell.Current == null) return;

        await Shell.Current.GoToAsync(nameof(AddUpdateMajorPage));
    }

    // Navigate to AddUpdateMajorPage in edit mode with the selected major ID.
    private async Task OnEditMajorAsync(Major? major)
    {
        if (major == null || Shell.Current == null) return;

        await Shell.Current.GoToAsync(
            $"{nameof(AddUpdateMajorPage)}?MajorId={major.Id}");
    }

    // Delete a selected major using MajorService.
    private async Task OnDeleteMajorAsync(Major? major)
    {
        if (major == null || Shell.Current == null) return;

        // Ask the user to confirm before deleting the major.
        bool confirm = await Shell.Current.DisplayAlert(
            "تأكيد الحذف",
            $"هل أنت متأكد من رغبتك في حذف تخصص '{major.Name}'؟",
            "نعم",
            "إلغاء"
        );

        if (confirm)
        {
            try
            {
                // Delete the selected major through MajorService.
                await _majorService.DeleteMajorAsync(major);

                // Reload the majors list after successful deletion.
                await LoadMajorsAsync();
            }
            catch (Exception ex)
            {
                // Display an error message if deletion fails.
                await Shell.Current.DisplayAlert(
                    "Error",
                    $"Failed to delete major: {ex.Message}",
                    "OK");
            }
        }
    }
}
