using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

[QueryProperty(nameof(LectureId), "LectureId")]
public class AddUpdateLectureViewModel : BindableObject
{
    private readonly LucterService _lucterService;

    private int _lectureId;
    public int LectureId
    {
        get => _lectureId;
        set
        {
            _lectureId = value;
            if (_lectureId > 0)
            {
                _ = LoadLectureDataAsync(_lectureId);
            }
        }
    }

    private int _groupId;
    public int GroupId
    {
        get => _groupId;
        set
        {
            if (_groupId != value)
            {
                _groupId = value;
                OnPropertyChanged(nameof(GroupId));
            }
        }
    }

    private string _selectedDay = "السبت";
    public string SelectedDay
    {
        get => _selectedDay;
        set
        {
            if (_selectedDay != value)
            {
                _selectedDay = value;
                OnPropertyChanged(nameof(SelectedDay));
            }
        }
    }

    private string _hallName = string.Empty;
    public string HallName
    {
        get => _hallName;
        set
        {
            if (_hallName != value)
            {
                _hallName = value;
                OnPropertyChanged(nameof(HallName));
            }
        }
    }

    public ICommand SaveLectureCommand { get; }
    public ICommand DeleteLectureCommand { get; }
    public ICommand CancelCommand { get; }

    // Constructor with Dependency Injection for LucterService
    public AddUpdateLectureViewModel(LucterService lucterService)
    {
        _lucterService = lucterService;

        SaveLectureCommand = new Command(async () => await OnSaveLectureExecuted());
        DeleteLectureCommand = new Command(async () => await OnDeleteLectureExecuted());
        CancelCommand = new Command(async () => await OnCancelExecuted());
    }

    // Load existing lecture data for editing
    private async Task LoadLectureDataAsync(int id)
    {
        var lecture = await _lucterService.GetLectureByIdAsync(id);
        if (lecture != null)
        {
            GroupId = lecture.GroupId;
            SelectedDay = !string.IsNullOrEmpty(lecture.Day) ? lecture.Day : "السبت";
            HallName = lecture.HallName;
        }
    }

    // Save or update lecture logic with Success/Failure alerts
    private async Task OnSaveLectureExecuted()
    {
        try
        {
            var lecture = new Lecture
            {
                Id = LectureId,
                GroupId = GroupId,
                Day = SelectedDay ?? "السبت",
                HallName = HallName ?? string.Empty
            };

            await _lucterService.SaveLectureAsync(lecture);

            await Shell.Current.DisplayAlert("Success", "Lecture saved successfully.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            await Shell.Current.DisplayAlert("Error", $"Failed to save lecture: {errorMsg}", "OK");
        }
    }

    // Delete lecture logic with Confirmation Alert
    private async Task OnDeleteLectureExecuted()
    {
        if (LectureId == 0)
        {
            await Shell.Current.DisplayAlert("Notice", "No lecture to delete.", "OK");
            return;
        }

        bool isConfirmed = await Shell.Current.DisplayAlert(
            "Confirm Delete", 
            "Are you sure you want to delete this lecture?", 
            "Yes, Delete", 
            "Cancel");

        if (isConfirmed)
        {
            try
            {
                var lecture = await _lucterService.GetLectureByIdAsync(LectureId);
                if (lecture != null)
                {
                    await _lucterService.DeleteLectureAsync(lecture);
                    await Shell.Current.DisplayAlert("Success", "Lecture deleted successfully.", "OK");
                    
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                await Shell.Current.DisplayAlert("Error", $"Failed to delete lecture: {errorMsg}", "OK");
            }
        }
    }

    // Cancel and return back
    private async Task OnCancelExecuted()
    {
        await Shell.Current.GoToAsync("..");
    }
}
