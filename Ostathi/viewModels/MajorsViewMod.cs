using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public class MajorsViewMod : BaseViewModel
{
    private readonly MajorService _majorService;

    // Observable collection of majors bound to the UI CollectionView
    private ObservableCollection<Major> _majorsList = new();
    public ObservableCollection<Major> MajorsList
    {
        get => _majorsList;
        set => SetProperty(ref _majorsList, value);
    }

    #region Commands

    public ICommand AddMajorCommand { get; }
    public ICommand EditMajorCommand { get; }
    public ICommand DeleteMajorCommand { get; }
    public ICommand LoadMajorsCommand { get; }

    #endregion

    // Constructor injecting MajorService via Dependency Injection
    public MajorsViewMod(MajorService majorService)
    {
        _majorService = majorService;
        Title = "إدارة التخصصات الأكاديمية";

        AddMajorCommand = new Command(async () => await OnAddMajorAsync());
        EditMajorCommand = new Command<Major>(async (major) => await OnEditMajorAsync(major));
        DeleteMajorCommand = new Command<Major>(async (major) => await OnDeleteMajorAsync(major));
        LoadMajorsCommand = new Command(async () => await LoadMajorsAsync());

        // Automatically load majors on initialization
        _ = LoadMajorsAsync();
    }

    // Load all majors using MajorService
    public async Task LoadMajorsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var majorsFromService = await _majorService.GetMajorsAsync();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MajorsList = new ObservableCollection<Major>(majorsFromService);
            });
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load majors: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Navigate to AddUpdateMajorPage in add mode
    private async Task OnAddMajorAsync()
    {
        if (Shell.Current == null) return;
        await Shell.Current.GoToAsync(nameof(AddUpdateMajorPage));
    }

    // Navigate to AddUpdateMajorPage in edit mode with the selected major ID
    private async Task OnEditMajorAsync(Major? major)
    {
        if (major == null || Shell.Current == null) return;
        await Shell.Current.GoToAsync($"{nameof(AddUpdateMajorPage)}?MajorId={major.Id}");
    }

    // Delete a selected major using MajorService
    private async Task OnDeleteMajorAsync(Major? major)
    {
        if (major == null || Shell.Current == null) return;

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
                // Delete via MajorService
                await _majorService.DeleteMajorAsync(major);

                await LoadMajorsAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete major: {ex.Message}", "OK");
            }
        }
    }
}
