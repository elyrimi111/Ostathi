using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi;

public class ScheduleManagementViewModel : BindableObject
{
    private readonly LucterService _lucterService;
    private readonly ScheduleService _scheduleService;

    private WeekDay _selectedWeekDay = WeekDay.Saturday;
    public WeekDay SelectedWeekDay
    {
        get => _selectedWeekDay;
        set
        {
            if (_selectedWeekDay != value)
            {
                _selectedWeekDay = value;
                OnPropertyChanged(nameof(SelectedWeekDay));
                OnPropertyChanged(nameof(SelectedDay));
                _ = LoadLecturesAsync();
            }
        }
    }

    // إبقاء SelectedDay لسهولة الربط مع الواجهة أو الأزرار النصية
    public string SelectedDay
    {
        get => ToArabicDayName(_selectedWeekDay);
        set
        {
            var parsedDay = ParseArabicDayName(value);
            if (parsedDay.HasValue)
            {
                SelectedWeekDay = parsedDay.Value;
            }
        }
    }

    private Lecture? _nextLecture;
    public Lecture? NextLecture
    {
        get => _nextLecture;
        private set
        {
            if (_nextLecture != value)
            {
                _nextLecture = value;
                OnPropertyChanged(nameof(NextLecture));
                OnPropertyChanged(nameof(HasNextLecture));
            }
        }
    }

    public bool HasNextLecture => NextLecture is not null;

    public ObservableCollection<Lecture> LecturesList { get; }

    public ICommand AddLectureCommand { get; }
    public ICommand EditLectureCommand { get; }
    public ICommand DeleteLectureCommand { get; }
    public ICommand LoadLecturesCommand { get; }
    public ICommand SelectDayCommand { get; }

    public ScheduleManagementViewModel(LucterService lucterService, ScheduleService? scheduleService = null)
    {
        _lucterService = lucterService;
        _scheduleService = scheduleService ?? new ScheduleService();
        LecturesList = new ObservableCollection<Lecture>();

        AddLectureCommand = new Command(async () => await OnAddLectureExecuted());
        EditLectureCommand = new Command<Lecture>(async (lecture) => await OnEditLectureExecuted(lecture));
        DeleteLectureCommand = new Command<Lecture>(async (lecture) => await OnDeleteLectureExecuted(lecture));
        LoadLecturesCommand = new Command(async () => await LoadLecturesAsync());
        SelectDayCommand = new Command<string>((dayStr) =>
        {
            if (!string.IsNullOrWhiteSpace(dayStr))
            {
                SelectedDay = dayStr;
            }
        });

        MessengerService.LectureAdded += OnLectureAdded;

        _ = LoadLecturesAsync();
    }

    private void OnLectureAdded()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _ = LoadLecturesAsync();
        });
    }

    public async Task LoadLecturesAsync()
    {
        var allLectures = await _lucterService.GetAllLecturesAsync();

        // تحديث المحاضرة القادمة تلقائياً
        NextLecture = _scheduleService.GetNextLecture(allLectures);

        // تصفية وترتيب محاضرات اليوم المختار
        var filteredLectures = allLectures
            .Where(l => !l.IsCancelled && l.DayOfWeek == SelectedWeekDay)
            .OrderBy(l => l.StartTime)
            .ToList();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            LecturesList.Clear();
            foreach (var lecture in filteredLectures)
            {
                LecturesList.Add(lecture);
            }
        });
    }

    private async Task OnAddLectureExecuted()
    {
        await Shell.Current.GoToAsync(nameof(AddUpdateLecturePage));
    }

    private async Task OnEditLectureExecuted(Lecture? lecture)
    {
        if (lecture is null) return;

        await Shell.Current.GoToAsync($"{nameof(AddUpdateLecturePage)}?LectureId={lecture.Id}");
    }

    private async Task OnDeleteLectureExecuted(Lecture? lecture)
    {
        if (lecture is null) return;

        bool isConfirmed = await Shell.Current.DisplayAlert(
            "تأكيد الحذف",
            $"هل أنت متأكد من حذف المحاضرة في قاعة \"{lecture.HallName}\"؟",
            "نعم، احذف",
            "إلغاء");

        if (isConfirmed)
        {
            try
            {
                await _lucterService.DeleteLectureAsync(lecture);
                await LoadLecturesAsync();
                await Shell.Current.DisplayAlert("نجاح", "تم حذف المحاضرة بنجاح.", "موافق");
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                await Shell.Current.DisplayAlert("خطأ", $"فشل حذف المحاضرة: {errorMsg}", "موافق");
            }
        }
    }

    public static string ToArabicDayName(WeekDay day) => day switch
    {
        WeekDay.Saturday => "السبت",
        WeekDay.Sunday => "الأحد",
        WeekDay.Monday => "الإثنين",
        WeekDay.Tuesday => "الثلاثاء",
        WeekDay.Wednesday => "الأربعاء",
        WeekDay.Thursday => "الخميس",
        WeekDay.Friday => "الجمعة",
        _ => "السبت"
    };

    public static WeekDay? ParseArabicDayName(string dayName) => dayName.Trim() switch
    {
        "السبت" => WeekDay.Saturday,
        "الأحد" or "الاحد" => WeekDay.Sunday,
        "الإثنين" or "الاثنين" => WeekDay.Monday,
        "الثلاثاء" => WeekDay.Tuesday,
        "الأربعاء" or "الاربعاء" => WeekDay.Wednesday,
        "الخميس" => WeekDay.Thursday,
        "الجمعة" => WeekDay.Friday,
        _ => null
    };

    public void Cleanup()
    {
        MessengerService.LectureAdded -= OnLectureAdded;
    }
}
