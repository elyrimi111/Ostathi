using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi;

[QueryProperty(nameof(LectureId), nameof(LectureId))]
public class LectureDetailsViewModel : BaseViewModel
{
    private readonly LucterService _lecturesService;
    private Lecture? _currentLecture;

    private int _lectureId;
    public int LectureId
    {
        get => _lectureId;
        set
        {
            _lectureId = value;
            OnPropertyChanged();
            _ = LoadLectureDetailsAsync();
        }
    }

    private string _subjectName = string.Empty;
    public string SubjectName
    {
        get => _subjectName;
        set => SetProperty(ref _subjectName, value);
    }

    private string _hallName = string.Empty;
    public string HallName
    {
        get => _hallName;
        set => SetProperty(ref _hallName, value);
    }

    private string _day = "السبت";
    public string Day
    {
        get => _day;
        set => SetProperty(ref _day, value);
    }

    private DateTime _lectureDate = DateTime.Now;
    public DateTime LectureDate
    {
        get => _lectureDate;
        set => SetProperty(ref _lectureDate, value);
    }

    // Days list for the Picker
    public List<string> DaysList { get; } = new List<string>
    {
        "السبت", "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة"
    };

    public ICommand SaveCommand { get; }

    public LectureDetailsViewModel(LucterService lecturesService)
    {
        _lecturesService = lecturesService;
        SaveCommand = new Command(async () => await SaveChangesAsync());
    }

    /// <summary>
    /// Load lecture details from database for editing
    /// </summary>
    private async Task LoadLectureDetailsAsync()
    {
        if (LectureId <= 0) return;

        IsBusy = true;
        try
        {
            _currentLecture = await _lecturesService.GetLectureByIdAsync(LectureId);
            if (_currentLecture != null)
            {
                SubjectName = _currentLecture.SubjectName;
                HallName = _currentLecture.HallName;
                Day = _currentLecture.Day;
                LectureDate = _currentLecture.LectureDate;
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"فشل في تحميل بيانات المحاضرة: {ex.Message}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Save modifications to the existing lecture
    /// </summary>
    private async Task SaveChangesAsync()
    {
        if (_currentLecture == null) return;

        try
        {
            _currentLecture.HallName = HallName;
            _currentLecture.Day = Day;
            _currentLecture.LectureDate = LectureDate;

            await _lecturesService.UpdateLectureAsync(_currentLecture);

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("نجاح", "تم تحديث بيانات المحاضرة بنجاح", "موافق");
                await Shell.Current.GoToAsync(".."); // العودة للشاشة السابقة
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"فشل في حفظ التعديلات: {ex.Message}", "موافق");
            }
        }
    }
}
