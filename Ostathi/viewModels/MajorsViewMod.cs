using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ostathi.Models;

namespace Ostathi;

public class MajorsViewMod : INotifyPropertyChanged
{
    private readonly MajorService _majorService;
    
    // Holds the ID of the major currently being edited (0 means adding a new major)
    private int _editingMajorId = 0;

    // Binds to the entry field for major name input
    private string _newMajorName = string.Empty;
    public string NewMajorName
    {
        get => _newMajorName;
        set => SetProperty(ref _newMajorName, value);
    }

    // Observable collection of majors bound to the UI CollectionView
    private ObservableCollection<Major> _majorsList = new();
    public ObservableCollection<Major> MajorsList
    {
        get => _majorsList;
        set => SetProperty(ref _majorsList, value);
    }

    #region Commands

    public ICommand SaveMajorCommand { get; }
    public ICommand EditMajorCommand { get; }
    public ICommand DeleteMajorCommand { get; }
    public ICommand LoadMajorsCommand { get; }

    #endregion

    // Constructor injecting MajorService via Dependency Injection
    public MajorsViewMod(MajorService majorService)
    {
        _majorService = majorService;

        SaveMajorCommand = new Command(async () => await OnSaveMajorAsync());
        EditMajorCommand = new Command<Major>(OnEditMajor);
        DeleteMajorCommand = new Command<Major>(async (major) => await OnDeleteMajorAsync(major));
        LoadMajorsCommand = new Command(async () => await LoadMajorsAsync());

        // Automatically load majors on initialization
        _ = LoadMajorsAsync();
    }

    // Load all majors using MajorService
    public async Task LoadMajorsAsync()
    {
        try
        {
            var majorsFromService = await _majorService.GetMajorsAsync();
            
            MainThread.BeginInvokeOnMainThread(() =>
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
    }

    // Save a new major or update an existing one using MajorService
    private async Task OnSaveMajorAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMajorName))
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تحذير", "الرجاء إدخال اسم التخصص", "موافق");
            }
            return;
        }

        try
        {
            Major majorToSave;

            if (_editingMajorId == 0)
            {
                // Create a new major entity
                majorToSave = new Major
                {
                    Name = NewMajorName.Trim()
                };
            }
            else
            {
                // Update an existing major entity
                majorToSave = new Major
                {
                    Id = _editingMajorId,
                    Name = NewMajorName.Trim()
                };
            }

            // Save via MajorService
            await _majorService.SaveMajorAsync(majorToSave);

            // Reset input and state after saving
            NewMajorName = string.Empty;
            _editingMajorId = 0;

            // Refresh the list view
            await LoadMajorsAsync();
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save major: {ex.Message}", "OK");
            }
        }
    }

    // Populate fields when the user clicks 'Edit' on a major item
    private void OnEditMajor(Major? major)
    {
        if (major == null) return;

        _editingMajorId = major.Id;
        NewMajorName = major.Name;
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

                // Clear editing state if the currently edited item was deleted
                if (_editingMajorId == major.Id)
                {
                    _editingMajorId = 0;
                    NewMajorName = string.Empty;
                }

                await LoadMajorsAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete major: {ex.Message}", "OK");
            }
        }
    }

    #region INotifyPropertyChanged Implementation

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
