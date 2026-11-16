using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Ostathi.Models;
using Ostathi.Services;

namespace Ostathi
{
    [QueryProperty(nameof(GroupId), nameof(GroupId))]
    public class LecturesViewModel : BaseViewModel
    {
        private readonly AppDbContext _dbContext;
        private readonly LucterService _lecturesService;

        // Group ID passed via Shell navigation
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

        // Current group details model for the GroupCard
        private Group? _currentGroup;
        public Group? CurrentGroup
        {
            get => _currentGroup;
            set => SetProperty(ref _currentGroup, value);
        }

        // Currently selected day for filtering (Default: السبت)
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

        // Total lectures count for stats
        private int _totalLectures;
        public int TotalLectures
        {
            get => _totalLectures;
            set => SetProperty(ref _totalLectures, value);
        }

        // Completed lectures count for stats
        private int _completedLectures;
        public int CompletedLectures
        {
            get => _completedLectures;
            set => SetProperty(ref _completedLectures, value);
        }

        // Full list of lectures fetched from DB for this group
        private List<Lecture> _allGroupLectures = new();

        // Filtered collection of lectures displayed in the UI based on SelectedDay
        public ObservableCollection<Lecture> LecturesList { get; } = new();

        // UI Commands
        public ICommand ClosePageCommand { get; }
        public ICommand SelectDayCommand { get; }
        public ICommand AddLectureCommand { get; }
        public ICommand RefreshCommand { get; }

        public LecturesViewModel(AppDbContext dbContext, LucterService lecturesService)
        {
            _dbContext = dbContext;
            _lecturesService = lecturesService;

            // Initialize commands
            ClosePageCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            SelectDayCommand = new Command<string>(day => SelectedDay = day);
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

                // 1. Fetch Group Details (Without .Include since Major is referenced by MajorId)
                CurrentGroup = await _dbContext.Groups
                    .FirstOrDefaultAsync(g => g.Id == GroupId);

                // 2. Fetch all lectures associated with this Group
                var allLectures = await _lecturesService.GetAllLecturesAsync();
                _allGroupLectures = allLectures.Where(l => l.GroupId == GroupId).ToList();

                // 3. Update Statistics (Consider lectures with past or current dates as completed)
                TotalLectures = _allGroupLectures.Count;
                CompletedLectures = _allGroupLectures.Count(l => l.LectureDate <= DateTime.Now);

                // 4. Apply filter for the initially selected day
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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LecturesList.Clear();

                var filtered = _allGroupLectures
                    .Where(l => string.Equals(l.Day, SelectedDay, StringComparison.OrdinalIgnoreCase))
                    .ToList();

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
