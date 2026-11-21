using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi;

[QueryProperty(nameof(LectureId), "LectureId")]
[QueryProperty(nameof(GroupSubjectId), "GroupSubjectId")]
[QueryProperty(nameof(GroupId), "GroupId")]
public class AddUpdateLectureViewModel : BindableObject
{
    private readonly LucterService _lucterService;
    private readonly AppDbContext _dbContext;

    private int _lectureId;
    public int LectureId
    {
        get => _lectureId;
        set
        {
            _lectureId = value;
            OnPropertyChanged(nameof(LectureId));
            if (_lectureId > 0)
            {
                _ = LoadLectureDataAsync(_lectureId);
            }
        }
    }

    private int _groupSubjectId;
    public int GroupSubjectId
    {
        get => _groupSubjectId;
        set
        {
            if (_groupSubjectId != value)
            {
                _groupSubjectId = value;
                OnPropertyChanged(nameof(GroupSubjectId));
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
                if (_groupId > 0 && GroupSubjectId <= 0)
                {
                    _ = ResolveGroupSubjectFromGroupAsync(_groupId);
                }
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

    private DateTime _lectureDate = DateTime.Today;
    public DateTime LectureDate
    {
        get => _lectureDate;
        set
        {
            if (_lectureDate != value)
            {
                _lectureDate = value;
                OnPropertyChanged(nameof(LectureDate));
            }
        }
    }

    private TimeSpan _startTime = new(8, 0, 0);
    public TimeSpan StartTime
    {
        get => _startTime;
        set
        {
            if (_startTime != value)
            {
                _startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }
    }

    private TimeSpan _endTime = new(9, 30, 0);
    public TimeSpan EndTime
    {
        get => _endTime;
        set
        {
            if (_endTime != value)
            {
                _endTime = value;
                OnPropertyChanged(nameof(EndTime));
            }
        }
    }

    private bool _isCancelled;
    public bool IsCancelled
    {
        get => _isCancelled;
        set
        {
            if (_isCancelled != value)
            {
                _isCancelled = value;
                OnPropertyChanged(nameof(IsCancelled));
            }
        }
    }

    public ICommand SaveLectureCommand { get; }
    public ICommand DeleteLectureCommand { get; }
    public ICommand CancelCommand { get; }

    public AddUpdateLectureViewModel(LucterService lucterService, AppDbContext dbContext)
    {
        _lucterService = lucterService;
        _dbContext = dbContext;

        SaveLectureCommand = new Command(async () => await OnSaveLectureExecuted());
        DeleteLectureCommand = new Command(async () => await OnDeleteLectureExecuted());
        CancelCommand = new Command(async () => await OnCancelExecuted());
    }

    private async Task ResolveGroupSubjectFromGroupAsync(int groupId)
    {
        var groupSubject = await _dbContext.GroupSubjects
            .FirstOrDefaultAsync(gs => gs.GroupId == groupId);
        if (groupSubject != null)
        {
            GroupSubjectId = groupSubject.Id;
        }
    }

    private async Task LoadLectureDataAsync(int id)
    {
        var lecture = await _lucterService.GetLectureByIdAsync(id);
        if (lecture != null)
        {
            GroupSubjectId = lecture.GroupSubjectId ?? 0;
            SelectedDay = ScheduleManagementViewModel.ToArabicDayName(lecture.DayOfWeek);
            HallName = lecture.HallName;
            LectureDate = lecture.LectureDate;
            StartTime = lecture.StartTime.ToTimeSpan();
            EndTime = lecture.EndTime.ToTimeSpan();
            IsCancelled = lecture.IsCancelled;
        }
    }

    private async Task OnSaveLectureExecuted()
    {
        try
        {
            if (GroupSubjectId <= 0 && GroupId > 0)
            {
                await ResolveGroupSubjectFromGroupAsync(GroupId);
            }

            if (GroupSubjectId <= 0)
            {
                await Shell.Current.DisplayAlert("تنبيه", "يرجى تحديد مجموعة المادة أولاً.", "موافق");
                return;
            }

            if (EndTime <= StartTime)
            {
                await Shell.Current.DisplayAlert("تنبيه", "وقت النهاية يجب أن يكون بعد وقت البداية.", "موافق");
                return;
            }

            var parsedDay = ScheduleManagementViewModel.ParseArabicDayName(SelectedDay) ?? WeekDay.Saturday;

            var lecture = new Lecture
            {
                Id = LectureId,
                GroupSubjectId = GroupSubjectId,
                DayOfWeek = parsedDay,
                HallName = HallName?.Trim() ?? string.Empty,
                LectureDate = LectureDate,
                StartTime = TimeOnly.FromTimeSpan(StartTime),
                EndTime = TimeOnly.FromTimeSpan(EndTime),
                IsCancelled = IsCancelled
            };

            await _lucterService.SaveLectureAsync(lecture);

            await Shell.Current.DisplayAlert("نجاح", "تم حفظ بيانات المحاضرة بنجاح.", "موافق");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            await Shell.Current.DisplayAlert("خطأ", $"فشل حفظ المحاضرة: {errorMsg}", "موافق");
        }
    }

    private async Task OnDeleteLectureExecuted()
    {
        if (LectureId == 0)
        {
            await Shell.Current.DisplayAlert("تنبيه", "لا توجد محاضرة لحذفها.", "موافق");
            return;
        }

        bool isConfirmed = await Shell.Current.DisplayAlert(
            "تأكيد الحذف",
            "هل أنت متأكد من حذف هذه المحاضرة؟",
            "نعم، احذف",
            "إلغاء");

        if (isConfirmed)
        {
            try
            {
                var lecture = await _lucterService.GetLectureByIdAsync(LectureId);
                if (lecture != null)
                {
                    await _lucterService.DeleteLectureAsync(lecture);
                    await Shell.Current.DisplayAlert("نجاح", "تم حذف المحاضرة بنجاح.", "موافق");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                await Shell.Current.DisplayAlert("خطأ", $"فشل حذف المحاضرة: {errorMsg}", "موافق");
            }
        }
    }

    private async Task OnCancelExecuted()
    {
        await Shell.Current.GoToAsync("..");
    }
}
