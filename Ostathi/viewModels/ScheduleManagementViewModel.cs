using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi;

public class ScheduleManagementViewModel : BindableObject
{
    private readonly LucterService _lucterService;

    private string _selectedDay = "السبت"; // Default to Saturday
    public string SelectedDay
    {
        get => _selectedDay;
        set
        {
            if (_selectedDay != value)
            {
                _selectedDay = value;
                OnPropertyChanged(nameof(SelectedDay));
                // Automatically reload and filter lectures when the selected day changes
                _ = LoadLecturesAsync();
            }
        }
    }

    // List of lectures bound to the UI
    public ObservableCollection<Lecture> LecturesList { get; }

    public ICommand AddLectureCommand { get; }
    public ICommand EditLectureCommand { get; }
    public ICommand DeleteLectureCommand { get; }
    public ICommand LoadLecturesCommand { get; }
    public ICommand SelectDayCommand { get; }

    // Constructor with Dependency Injection for LucterService
    public ScheduleManagementViewModel(LucterService lucterService)
    {
        _lucterService = lucterService;
        LecturesList = new ObservableCollection<Lecture>();

        // Commands definition
        AddLectureCommand = new Command(async () => await OnAddLectureExecuted());
        EditLectureCommand = new Command<Lecture>(async (lecture) => await OnEditLectureExecuted(lecture));
        DeleteLectureCommand = new Command<Lecture>(async (lecture) => await OnDeleteLectureExecuted(lecture));
        LoadLecturesCommand = new Command(async () => await LoadLecturesAsync());
        SelectDayCommand = new Command<string>((dayStr) => 
        {
            if (!string.IsNullOrEmpty(dayStr))
            {
                SelectedDay = dayStr;
            }
        });

        // Subscribe to the LectureAdded event from MessengerService
        MessengerService.LectureAdded += OnLectureAdded;

        // Automatically load lectures when ViewModel initializes
        _ = LoadLecturesAsync();
    }

    // Event handler to refresh lectures list when a lecture is added or updated
    private async void OnLectureAdded()
    {
        // Ensure UI updates run on the main thread if triggered from background tasks
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await LoadLecturesAsync();
        });
    }

    // Load and filter lectures from the database based on SelectedDay (string)
    public async Task LoadLecturesAsync()
    {
        var allLectures = await _lucterService.GetAllLecturesAsync();
        
        LecturesList.Clear();
        
        // Filter lectures by the selected day string
        var filteredLectures = allLectures
            .Where(l => l.Day == SelectedDay)
            .ToList();

        foreach (var lecture in filteredLectures)
        {
            LecturesList.Add(lecture);
        }
    }

    // Navigate to Add/Update page for creating a new lecture
    private async Task OnAddLectureExecuted()
    {
        await Shell.Current.GoToAsync(nameof(AddUpdateLecturePage));
    }

    // Navigate to Add/Update page passing the selected lecture's ID for editing
    private async Task OnEditLectureExecuted(Lecture lecture)
    {
        if (lecture == null) return;

        // Pass the lecture Id as a query parameter
        await Shell.Current.GoToAsync($"{nameof(AddUpdateLecturePage)}?LectureId={lecture.Id}");
    }

    // Delete a lecture using LucterService with Confirmation Alert
    private async Task OnDeleteLectureExecuted(Lecture lecture)
    {
        if (lecture == null) return;

        // Confirmation alert before deletion
        bool isConfirmed = await Shell.Current.DisplayAlert(
            "Confirm Delete", 
            $"Are you sure you want to delete this lecture in hall \"{lecture.HallName}\"?", 
            "Yes, Delete", 
            "Cancel");

        if (isConfirmed)
        {
            try
            {
                await _lucterService.DeleteLectureAsync(lecture);
                LecturesList.Remove(lecture);
                await Shell.Current.DisplayAlert("Success", "Lecture deleted successfully.", "OK");
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                await Shell.Current.DisplayAlert("Error", $"Failed to delete lecture: {errorMsg}", "OK");
            }
        }
    }
}
