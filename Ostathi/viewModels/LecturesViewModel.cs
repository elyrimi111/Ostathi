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

namespace Ostathi
{
    [QueryProperty(nameof(GroupId), nameof(GroupId))]
    public class LecturesViewModel : BaseViewModel
    {
        private readonly AppDbContext _dbContext;
        private readonly LucterService _lecturesService;

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
                    _ = LoadPageDataAsync();
                }
            }
        }

        private Group? _currentGroup;
        public Group? CurrentGroup
        {
            get => _currentGroup;
            set => SetProperty(ref _currentGroup, value);
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
                    OnPropertyChanged();
                    FilterLecturesByDay();
                }
            }
        }

        private int _totalLectures;
        public int TotalLectures
        {
            get => _totalLectures;
            set => SetProperty(ref _totalLectures, value);
        }

        private int _completedLectures;
        public int CompletedLectures
        {
            get => _completedLectures;
            set => SetProperty(ref _completedLectures, value);
        }

        private List<Lecture> _allGroupLectures = new();
        private HashSet<int> _takenLectureIds = new();

        public ObservableCollection<Lecture> LecturesList { get; } = new();

        public ICommand ClosePageCommand { get; }
        public ICommand SelectDayCommand { get; }
        public ICommand AddLectureCommand { get; }
        public ICommand RefreshCommand { get; }

        public LecturesViewModel(AppDbContext dbContext, LucterService lecturesService)
        {
            _dbContext = dbContext;
            _lecturesService = lecturesService;

            ClosePageCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            SelectDayCommand = new Command<string>(day =>
            {
                if (!string.IsNullOrWhiteSpace(day))
                {
                    SelectedDay = day;
                }
            });
            AddLectureCommand = new Command(async () => await NavigateToAddLectureAsync());
            RefreshCommand = new Command(async () => await LoadPageDataAsync());
        }

        /// <summary>
        /// Load group details and all related lectures from the database
        /// </summary>
        public async Task LoadPageDataAsync()
        {
            try
            {
                if (GroupId <= 0) return;

                // 1. Fetch Group Details (Foreign key MajorId only)
                CurrentGroup = await _dbContext.Groups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == GroupId);

                // 2. Get all GroupSubject IDs belonging to this Group
                var groupSubjectIds = await _dbContext.GroupSubjects
                    .AsNoTracking()
                    .Where(gs => gs.GroupId == GroupId)
                    .Select(gs => gs.Id)
                    .ToListAsync();

                // 3. Fetch all non-cancelled lectures associated with those GroupSubjects
                var allLectures = await _lecturesService.GetAllLecturesAsync();
                _allGroupLectures = allLectures
                    .Where(l => !l.IsCancelled &&
                                l.GroupSubjectId.HasValue &&
                                groupSubjectIds.Contains(l.GroupSubjectId.Value))
                    .OrderBy(l => l.StartTime)
                    .ToList();

                // 4. Fetch taken lectures from GroupSubjectLucters
                var takenIds = await _dbContext.GroupSubjectLucters
                    .AsNoTracking()
                    .Where(x => groupSubjectIds.Contains(x.GroupSubjectId) && x.IsTaken)
                    .Select(x => x.LectureId)
                    .ToListAsync();

                _takenLectureIds = takenIds.ToHashSet();

                // 5. Update Statistics
                TotalLectures = _allGroupLectures.Count;
                CompletedLectures = _allGroupLectures.Count(l => _takenLectureIds.Contains(l.Id));

                // 6. Apply filter for the initially selected day
                FilterLecturesByDay();
            }
            catch (Exception ex)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Error", $"Failed to load lectures data: {ex.Message}", "OK");
                }
            }
        }

        /// <summary>
        /// Filter lectures based on the currently selected day of the week
        /// </summary>
        private void FilterLecturesByDay()
        {
            var targetWeekDay = ScheduleManagementViewModel.ParseArabicDayName(SelectedDay);

            var filtered = targetWeekDay.HasValue
                ? _allGroupLectures.Where(l => l.DayOfWeek == targetWeekDay.Value).ToList()
                : new List<Lecture>();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                LecturesList.Clear();
                foreach (var lecture in filtered)
                {
                    LecturesList.Add(lecture);
                }
            });
        }

        /// <summary>
        /// Navigate to the page for adding a new lecture
        /// </summary>
        private async Task NavigateToAddLectureAsync()
        {
            if (GroupId <= 0)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select a valid group first.", "OK");
                return;
            }

            await Shell.Current.GoToAsync($"AddLecturePage?GroupId={GroupId}");
        }
    }
}
