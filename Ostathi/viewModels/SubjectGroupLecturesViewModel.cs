using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Ostathi.Models;
using Ostathi.Services;

namespace Ostathi.ViewModels
{
    [QueryProperty(nameof(GroupId), nameof(GroupId))]
    [QueryProperty(nameof(SubjectId), nameof(SubjectId))]
    public class SubjectGroupLecturesViewModel : BaseViewModel
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
                    _ = LoadDataAsync();
                }
            }
        }

        // Subject ID passed via Shell navigation
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

        // Group Name displayed in the header
        private string _groupName = "Loading...";
        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        // Subject Name displayed in the header
        private string _subjectName = "Loading...";
        public string SubjectName
        {
            get => _subjectName;
            set => SetProperty(ref _subjectName, value);
        }

        // Collection of lectures for this specific group and subject
        public ObservableCollection<Lecture> LecturesList { get; } = new();

        // Currently selected lecture in the list
        private Lecture? _selectedLecture;
        public Lecture? SelectedLecture
        {
            get => _selectedLecture;
            set
            {
                SetProperty(ref _selectedLecture, value);
                if (value != null)
                {
                    // Optional: Navigate when selected from list
                    _ = NavigateToEditLectureAsync(value);
                    SelectedLecture = null; // Reset selection
                }
            }
        }

        // UI Commands (No Add command anymore, only Edit and Delete)
        public ICommand EditLectureCommand { get; }
        public ICommand DeleteLectureCommand { get; }
        public ICommand RefreshCommand { get; }

        public SubjectGroupLecturesViewModel(AppDbContext dbContext, LucterService lecturesService)
        {
            _dbContext = dbContext;
            _lecturesService = lecturesService;

            // Initialize commands
            EditLectureCommand = new Command<Lecture>(async (lecture) => await NavigateToEditLectureAsync(lecture));
            DeleteLectureCommand = new Command<Lecture>(async (lecture) => await DeleteLectureAsync(lecture));
            RefreshCommand = new Command(async () => await LoadDataAsync());
        }

        /// <summary>
        /// Load group details, subject name, and filtered lectures from the database
        /// </summary>
        public async Task LoadDataAsync()
        {
            try
            {
                if (GroupId <= 0 || SubjectId <= 0) return;

                IsBusy = true;

                // 1. Fetch Group Name
                var group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == GroupId);
                if (group != null)
                {
                    GroupName = group.Name;
                }

                // 2. Fetch Subject Name
                var subject = await _dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == SubjectId);
                string targetSubjectName = subject?.Name ?? string.Empty;
                SubjectName = targetSubjectName;

                // 3. Automatically generate 12 lectures if they don't exist yet
                await _lecturesService.GenerateDefaultLecturesAsync(GroupId, targetSubjectName);

                // 4. Fetch Lectures matching this Group and Subject Name
                var allLectures = await _lecturesService.GetAllLecturesAsync();
                var filteredLectures = allLectures
                    .Where(l => l.GroupId == GroupId && string.Equals(l.SubjectName, targetSubjectName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(l => l.LectureDate)
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

        /// <summary>
        /// Navigate to lecture details/edit page passing LectureId
        /// </summary>
        private async Task NavigateToEditLectureAsync(Lecture lecture)
        {
            if (lecture == null) return;
            await Shell.Current.GoToAsync($"{nameof(LectureDetailsPage)}?LectureId={lecture.Id}");
        }

        /// <summary>
        /// Delete a specific lecture item
        /// </summary>
        private async Task DeleteLectureAsync(Lecture lecture)
        {
            if (lecture == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm Delete", "Are you sure you want to delete this lecture?", "Yes", "Cancel");
            if (confirm)
            {
                try
                {
                    await _lecturesService.DeleteLectureAsync(lecture);
                    LecturesList.Remove(lecture);
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
                }
            }
        }
    }
}
