using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi;

[QueryProperty(nameof(GroupSubjectId), "GroupSubjectId")]
[QueryProperty(nameof(PreselectedGroupId), "GroupId")]
public partial class AddUpdateGroupSubjectViewModel : ObservableObject
{
    private readonly AppDbContext _dbContext = new();
    private readonly GroupSubjectService _groupSubjectService = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    [NotifyPropertyChangedFor(nameof(PageTitle))]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    private int _groupSubjectId;

    [ObservableProperty]
    private int _preselectedGroupId;

    [ObservableProperty]
    private ObservableCollection<Group> _groups = new();

    [ObservableProperty]
    private ObservableCollection<Subject> _subjects = new();

    [ObservableProperty]
    private ObservableCollection<string> _daysOfWeek = new()
    {
        "السبت",
        "الأحد",
        "الإثنين",
        "الثلاثاء",
        "الأربعاء",
        "الخميس",
        "الجمعة"
    };

    [ObservableProperty]
    private Group? _selectedGroup;

    [ObservableProperty]
    private Subject? _selectedSubject;

    [ObservableProperty]
    private string _selectedDay = "السبت";

    [ObservableProperty]
    private TimeSpan _startTime = new(8, 0, 0);

    [ObservableProperty]
    private TimeSpan _endTime = new(9, 30, 0);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsEditMode => GroupSubjectId > 0;
    public bool IsNotBusy => !IsBusy;
    public string PageTitle => IsEditMode ? "تعديل مادة المجموعة" : "إضافة مادة لمجموعة";
    public string SaveButtonText => IsEditMode ? "حفظ التعديلات" : "حفظ وربط المادة";

    public ICommand ClosePageCommand { get; }
    public ICommand SaveCommand { get; }

    public AddUpdateGroupSubjectViewModel()
    {
        ClosePageCommand = new AsyncRelayCommand(ClosePageAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);

        _ = LoadDataAsync();
    }

    partial void OnGroupSubjectIdChanged(int value)
    {
        if (value > 0 && Groups.Count > 0 && Subjects.Count > 0)
        {
            _ = ApplySelectionAsync();
        }
    }

    partial void OnPreselectedGroupIdChanged(int value)
    {
        if (value > 0 && Groups.Count > 0)
        {
            SelectedGroup = Groups.FirstOrDefault(g => g.Id == value);
        }
    }

    public async Task LoadDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var groupList = await _dbContext.Groups.AsNoTracking().ToListAsync();
            var subjectList = await _dbContext.Subjects.AsNoTracking().ToListAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Groups = new ObservableCollection<Group>(groupList);
                Subjects = new ObservableCollection<Subject>(subjectList);
            });

            await ApplySelectionAsync();
        }
        catch (Exception ex)
        {
            var err = ex.InnerException?.Message ?? ex.Message;
            if (Shell.Current != null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayAlert("خطأ في التحميل", err, "موافق"));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ApplySelectionAsync()
    {
        if (IsEditMode)
        {
            var currentRecord = await _groupSubjectService.GetByIdAsync(GroupSubjectId);
            if (currentRecord != null)
            {
                var existingLecture = await _dbContext.Lectures
                    .AsNoTracking()
                    .Where(l => l.GroupSubjectId == currentRecord.Id && !l.IsCancelled)
                    .OrderBy(l => l.LectureNumber)
                    .ThenBy(l => l.Id)
                    .FirstOrDefaultAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SelectedGroup = Groups.FirstOrDefault(g => g.Id == currentRecord.GroupId);
                    SelectedSubject = Subjects.FirstOrDefault(s => s.Id == currentRecord.SubjectId);

                    if (existingLecture != null)
                    {
                        SelectedDay = ScheduleManagementViewModel.ToArabicDayName(existingLecture.DayOfWeek);
                        StartTime = existingLecture.StartTime.ToTimeSpan();
                        EndTime = existingLecture.EndTime.ToTimeSpan();
                    }
                });
            }
        }
        else if (PreselectedGroupId > 0)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SelectedGroup = Groups.FirstOrDefault(g => g.Id == PreselectedGroupId);
            });
        }
    }

    private async Task SaveAsync()
    {
        if (SelectedGroup == null)
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("تنبيه", "يرجى اختيار المجموعة أولاً", "موافق");
            return;
        }

        if (SelectedSubject == null)
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("تنبيه", "يرجى اختيار المادة أولاً", "موافق");
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedDay))
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("تنبيه", "يرجى اختيار اليوم الدراسي للمحاضرة", "موافق");
            return;
        }

        if (EndTime <= StartTime)
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("تنبيه", "وقت النهاية يجب أن يكون بعد وقت البداية", "موافق");
            return;
        }

        try
        {
            IsBusy = true;

            if (IsEditMode)
            {
                bool updated = await _groupSubjectService.UpdateGroupSubjectAsync(
                    GroupSubjectId,
                    SelectedGroup.Id,
                    SelectedSubject.Id);

                if (!updated)
                {
                    if (Shell.Current != null)
                        await Shell.Current.DisplayAlert("تنبيه", "هذا الربط موجود مسبقاً أو تعذر التحديث", "موافق");
                    return;
                }
            }
            else
            {
                bool added = await _groupSubjectService.AssignSubjectToGroupAsync(
                    SelectedGroup.Id,
                    SelectedSubject.Id);

                if (!added)
                {
                    if (Shell.Current != null)
                        await Shell.Current.DisplayAlert("تنبيه", "هذه المادة مرتبطة بهذه المجموعة مسبقاً", "موافق");
                    return;
                }
            }

            var groupSubjectRecord = await _dbContext.GroupSubjects
                .AsNoTracking()
                .FirstOrDefaultAsync(gs => gs.GroupId == SelectedGroup.Id && gs.SubjectId == SelectedSubject.Id);

            if (groupSubjectRecord != null)
            {
                await SyncLectureScheduleAsync(groupSubjectRecord.Id);
            }

            if (Shell.Current != null)
            {
                string msg = IsEditMode ? "تم تحديث بيانات المادة وموعد المحاضرة بنجاح" : "تم ربط المادة وتحديد موعد المحاضرة بنجاح";
                await Shell.Current.DisplayAlert("نجاح", msg, "موافق");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            var err = ex.InnerException?.Message ?? ex.Message;
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("خطأ", err, "موافق");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SyncLectureScheduleAsync(int groupSubjectId)
    {
        var weekDay = ScheduleManagementViewModel.ParseArabicDayName(SelectedDay) ?? WeekDay.Saturday;
        var startOnly = TimeOnly.FromTimeSpan(StartTime);
        var endOnly = TimeOnly.FromTimeSpan(EndTime);

        var existingLectures = await _dbContext.Lectures
            .Where(l => l.GroupSubjectId == groupSubjectId)
            .OrderBy(l => l.LectureNumber)
            .ThenBy(l => l.Id)
            .ToListAsync();

        if (existingLectures.Count > 0)
        {
            foreach (var lecture in existingLectures)
            {
                lecture.DayOfWeek = weekDay;
                lecture.StartTime = startOnly;
                lecture.EndTime = endOnly;
            }
        }
        else
        {
            var newLecture = new Lecture
            {
                LectureNumber = 1,
                GroupSubjectId = groupSubjectId,
                DayOfWeek = weekDay,
                HallName = "قاعة 1",
                LectureDate = DateTime.Today,
                StartTime = startOnly,
                EndTime = endOnly,
                IsCancelled = false
            };

            _dbContext.Lectures.Add(newLecture);
            await _dbContext.SaveChangesAsync();

            bool linkExists = await _dbContext.GroupSubjectLucters
                .AnyAsync(x => x.GroupSubjectId == groupSubjectId && x.LectureId == newLecture.Id);

            if (!linkExists)
            {
                _dbContext.GroupSubjectLucters.Add(new GroupSubjectLucters
                {
                    GroupSubjectId = groupSubjectId,
                    LectureId = newLecture.Id,
                    LectureNumber = 1,
                    IsTaken = false
                });
            }
        }

        await _dbContext.SaveChangesAsync();
        MessengerService.NotifyLectureAdded();
    }

    private async Task ClosePageAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
