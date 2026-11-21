using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi.ViewModels
{
    public class SubjectGroupLectureDisplayItem : BaseViewModel
    {
        public Lecture Lecture { get; set; } = null!;
        public int Id => Lecture.Id;
        public int LectureNumber => Lecture.LectureNumber;
        public string HallName => Lecture.HallName;
        public DateTime LectureDate => Lecture.LectureDate;
        public TimeOnly StartTime => Lecture.StartTime;
        public TimeOnly EndTime => Lecture.EndTime;
        public string ArabicDayName { get; set; } = string.Empty;

        private bool _isTaken;
        public bool IsTaken
        {
            get => _isTaken;
            set
            {
                if (SetProperty(ref _isTaken, value))
                {
                    OnPropertyChanged(nameof(TakenStatusText));
                }
            }
        }

        public string TakenStatusText => IsTaken ? "تم أخذها" : "لم تُؤخذ";
    }

    [QueryProperty(nameof(GroupId), nameof(GroupId))]
    [QueryProperty(nameof(SubjectId), nameof(SubjectId))]
    public class SubjectGroupLecturesViewModel : BaseViewModel
    {
        private readonly AppDbContext _dbContext;
        private readonly LucterService _lecturesService;

        private int _currentGroupSubjectId;

        private int _groupId;
        public int GroupId
        {
            get => _groupId;
            set
            {
                if (_groupId != value)
                {
                    _groupId = value;
                    OnPropertyChanged();
                    _ = LoadDataAsync();
                }
            }
        }

        private int _subjectId;
        public int SubjectId
        {
            get => _subjectId;
            set
            {
                if (_subjectId != value)
                {
                    _subjectId = value;
                    OnPropertyChanged();
                    _ = LoadDataAsync();
                }
            }
        }

        private string _groupName = "Loading...";
        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        private string _subjectName = "Loading...";
        public string SubjectName
        {
            get => _subjectName;
            set => SetProperty(ref _subjectName, value);
        }

        public ObservableCollection<SubjectGroupLectureDisplayItem> LecturesList { get; } = new();

        private SubjectGroupLectureDisplayItem? _selectedLecture;
        public SubjectGroupLectureDisplayItem? SelectedLecture
        {
            get => _selectedLecture;
            set
            {
                SetProperty(ref _selectedLecture, value);
                if (value != null)
                {
                    _ = NavigateToEditLectureAsync(value);
                    SelectedLecture = null;
                }
            }
        }

        public ICommand EditLectureCommand { get; }
        public ICommand DeleteLectureCommand { get; }
        public ICommand ToggleTakenCommand { get; }
        public ICommand RefreshCommand { get; }

        public SubjectGroupLecturesViewModel(AppDbContext dbContext, LucterService lecturesService)
        {
            _dbContext = dbContext;
            _lecturesService = lecturesService;

            EditLectureCommand = new Command<SubjectGroupLectureDisplayItem>(async (item) => await NavigateToEditLectureAsync(item));
            DeleteLectureCommand = new Command<SubjectGroupLectureDisplayItem>(async (item) => await DeleteLectureAsync(item));
            ToggleTakenCommand = new Command<SubjectGroupLectureDisplayItem>(async (item) => await ToggleTakenAsync(item));
            RefreshCommand = new Command(async () => await LoadDataAsync());
        }

        public async Task LoadDataAsync()
        {
            try
            {
                if (GroupId <= 0 || SubjectId <= 0) return;

                IsBusy = true;

                var group = await _dbContext.Groups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == GroupId);
                if (group != null)
                {
                    GroupName = group.Name;
                }

                var subject = await _dbContext.Subjects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == SubjectId);
                SubjectName = subject?.Name ?? string.Empty;

                var groupSubject = await _dbContext.GroupSubjects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(gs => gs.GroupId == GroupId && gs.SubjectId == SubjectId);

                if (groupSubject == null)
                {
                    _currentGroupSubjectId = 0;
                    MainThread.BeginInvokeOnMainThread(() => LecturesList.Clear());
                    return;
                }

                _currentGroupSubjectId = groupSubject.Id;

                await _lecturesService.GenerateDefaultLecturesAsync(groupSubject.Id);

                var filteredLectures = await _lecturesService.GetLecturesByGroupSubjectIdAsync(groupSubject.Id);

                var links = await _dbContext.GroupSubjectLucters
                    .AsNoTracking()
                    .Where(x => x.GroupSubjectId == groupSubject.Id)
                    .ToListAsync();

                var linksByLectureId = links.ToDictionary(x => x.LectureId, x => x);

                var displayItems = filteredLectures
                    .Where(l => !l.IsCancelled)
                    .OrderBy(l => l.LectureNumber)
                    .ThenBy(l => l.LectureDate)
                    .Select(lecture =>
                    {
                        linksByLectureId.TryGetValue(lecture.Id, out var link);
                        return new SubjectGroupLectureDisplayItem
                        {
                            Lecture = lecture,
                            ArabicDayName = ScheduleManagementViewModel.ToArabicDayName(lecture.DayOfWeek),
                            IsTaken = link?.IsTaken ?? false
                        };
                    })
                    .ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LecturesList.Clear();
                    foreach (var item in displayItems)
                    {
                        LecturesList.Add(item);
                    }
                });
            }
            catch (Exception ex)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Error", $"Failed to load lectures: {ex.Message}", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ToggleTakenAsync(SubjectGroupLectureDisplayItem? item)
        {
            if (item == null || _currentGroupSubjectId <= 0) return;

            try
            {
                bool nextState = !item.IsTaken;

                var link = await _dbContext.GroupSubjectLucters
                    .FirstOrDefaultAsync(x => x.GroupSubjectId == _currentGroupSubjectId && x.LectureId == item.Id);

                if (link != null)
                {
                    link.IsTaken = nextState;
                    link.LectureNumber = item.LectureNumber;
                }
                else
                {
                    _dbContext.GroupSubjectLucters.Add(new GroupSubjectLucters
                    {
                        GroupSubjectId = _currentGroupSubjectId,
                        LectureId = item.Id,
                        LectureNumber = item.LectureNumber,
                        IsTaken = nextState
                    });
                }

                await _dbContext.SaveChangesAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    item.IsTaken = nextState;
                });
            }
            catch (Exception ex)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("خطأ", $"تعذر تحديث حالة المحاضرة: {ex.Message}", "موافق");
                }
            }
        }

        private async Task NavigateToEditLectureAsync(SubjectGroupLectureDisplayItem? item)
        {
            if (item == null) return;
            await Shell.Current.GoToAsync($"{nameof(LectureDetailsPage)}?LectureId={item.Id}");
        }

        private async Task DeleteLectureAsync(SubjectGroupLectureDisplayItem? item)
        {
            if (item == null || Shell.Current == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirm Delete",
                "Are you sure you want to delete this lecture?",
                "Yes",
                "Cancel");

            if (confirm)
            {
                try
                {
                    await _lecturesService.DeleteLectureAsync(item.Lecture);
                    MainThread.BeginInvokeOnMainThread(() => LecturesList.Remove(item));
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
                }
            }
        }
    }
}
