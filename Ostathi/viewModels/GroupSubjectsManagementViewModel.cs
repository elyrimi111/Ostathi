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
    public class GroupSubjectDisplayItem
    {
        public GroupSubject GroupSubject { get; set; } = null!;
        public int Id => GroupSubject.Id;
        public int GroupId => GroupSubject.GroupId;
        public int SubjectId => GroupSubject.SubjectId;

        public string GroupName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string MajorName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Status => GroupSubject.Status;
        public string? Notes => GroupSubject.Notes;
    }

    public class GroupSubjectsManagementViewModel : BaseViewModel
    {
        private readonly GroupSubjectService _groupSubjectService;
        private readonly AppDbContext _dbContext;

        private List<GroupSubjectDisplayItem> _allItems = new();
        private Dictionary<int, Group> _groupsById = new();
        private Dictionary<int, Subject> _subjectsById = new();
        private Dictionary<int, Major> _majorsById = new();

        private ObservableCollection<GroupSubjectDisplayItem> _filteredGroupSubjects = new();
        public ObservableCollection<GroupSubjectDisplayItem> FilteredGroupSubjects
        {
            get => _filteredGroupSubjects;
            set
            {
                if (SetProperty(ref _filteredGroupSubjects, value))
                {
                    OnPropertyChanged(nameof(IsEmpty));
                }
            }
        }

        public bool IsNotBusy => !IsBusy;
        public bool IsEmpty => !IsBusy && FilteredGroupSubjects.Count == 0;

        public ObservableCollection<string> LevelsList { get; set; } = new();
        public ObservableCollection<Subject> SubjectsList { get; set; } = new();
        public ObservableCollection<Major> MajorsList { get; set; } = new();

        private string? _selectedLevel;
        public string? SelectedLevel
        {
            get => _selectedLevel;
            set { if (SetProperty(ref _selectedLevel, value)) ApplyFilters(); }
        }

        private Subject? _selectedSubject;
        public Subject? SelectedSubject
        {
            get => _selectedSubject;
            set { if (SetProperty(ref _selectedSubject, value)) ApplyFilters(); }
        }

        private Major? _selectedMajor;
        public Major? SelectedMajor
        {
            get => _selectedMajor;
            set { if (SetProperty(ref _selectedMajor, value)) ApplyFilters(); }
        }

        public ICommand RemoveSubjectCommand { get; }
        public ICommand OpenAssignSubjectCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand OpenManageSubjectCommand { get; }

        public GroupSubjectsManagementViewModel(GroupSubjectService groupSubjectService, AppDbContext dbContext)
        {
            _groupSubjectService = groupSubjectService;
            _dbContext = dbContext;

            RemoveSubjectCommand = new Command<GroupSubjectDisplayItem>(async (item) => await OnRemoveSubjectExecuted(item));
            OpenAssignSubjectCommand = new Command(async () => await OnOpenAssignSubjectExecuted());
            ClearFiltersCommand = new Command(OnClearFiltersExecuted);
            OpenManageSubjectCommand = new Command<GroupSubjectDisplayItem>(async (item) => await OnOpenManageSubjectExecuted(item));

            _ = LoadInitialDataAsync();
        }

        private void UpdateBusyState()
        {
            OnPropertyChanged(nameof(IsNotBusy));
            OnPropertyChanged(nameof(IsEmpty));
        }

        // Load subjects, majors, groups, and group-subject assignments
        public async Task LoadInitialDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                UpdateBusyState();

                var subjects = await _dbContext.Subjects.AsNoTracking().ToListAsync();
                var majors = await _dbContext.Majors.AsNoTracking().ToListAsync();
                var groups = await _dbContext.Groups.AsNoTracking().ToListAsync();
                var allRelations = await _dbContext.GroupSubjects.AsNoTracking().ToListAsync();

                _subjectsById = subjects.ToDictionary(s => s.Id);
                _majorsById = majors.ToDictionary(m => m.Id);
                _groupsById = groups.ToDictionary(g => g.Id);

                _allItems = allRelations.Select(gs =>
                {
                    _groupsById.TryGetValue(gs.GroupId, out var grp);
                    _subjectsById.TryGetValue(gs.SubjectId, out var subj);
                    Major? maj = null;
                    if (grp != null)
                    {
                        _majorsById.TryGetValue(grp.MajorId, out maj);
                    }

                    return new GroupSubjectDisplayItem
                    {
                        GroupSubject = gs,
                        GroupName = grp?.Name ?? $"مجموعة #{gs.GroupId}",
                        SubjectName = subj?.Name ?? $"مادة #{gs.SubjectId}",
                        MajorName = maj?.Name ?? string.Empty,
                        AcademicYear = grp?.AcademicYear ?? string.Empty
                    };
                }).ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SubjectsList.Clear();
                    foreach (var s in subjects) SubjectsList.Add(s);

                    MajorsList.Clear();
                    foreach (var m in majors) MajorsList.Add(m);

                    LevelsList.Clear();
                    LevelsList.Add("المستوى الأول");
                    LevelsList.Add("المستوى الثاني");
                    LevelsList.Add("المستوى الثالث");
                    LevelsList.Add("المستوى الرابع");
                    LevelsList.Add("المستوى الخامس");
                });

                ApplyFilters();
            }
            catch (Exception ex)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("خطأ", $"فشل تحميل البيانات: {ex.Message}", "موافق");
                }
            }
            finally
            {
                IsBusy = false;
                UpdateBusyState();
            }
        }

        // Apply active filters for subject, major, and academic level
        private void ApplyFilters()
        {
            var query = _allItems.AsEnumerable();

            if (SelectedSubject != null)
            {
                query = query.Where(x => x.SubjectId == SelectedSubject.Id);
            }

            if (SelectedMajor != null)
            {
                query = query.Where(x =>
                    _groupsById.TryGetValue(x.GroupId, out var grp) &&
                    grp.MajorId == SelectedMajor.Id);
            }

            if (!string.IsNullOrWhiteSpace(SelectedLevel))
            {
                query = query.Where(x =>
                    !string.IsNullOrWhiteSpace(x.AcademicYear) &&
                    x.AcademicYear.Contains(SelectedLevel, StringComparison.OrdinalIgnoreCase));
            }

            var filteredList = query.ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                FilteredGroupSubjects = new ObservableCollection<GroupSubjectDisplayItem>(filteredList);
                OnPropertyChanged(nameof(IsEmpty));
            });
        }

        // Reset all active filters
        private void OnClearFiltersExecuted()
        {
            _selectedLevel = null;
            _selectedSubject = null;
            _selectedMajor = null;

            OnPropertyChanged(nameof(SelectedLevel));
            OnPropertyChanged(nameof(SelectedSubject));
            OnPropertyChanged(nameof(SelectedMajor));

            ApplyFilters();
        }

        // Remove selected subject assignment from group
        private async Task OnRemoveSubjectExecuted(GroupSubjectDisplayItem? item)
        {
            if (item == null || Shell.Current == null) return;

            bool confirm = await Shell.Current.DisplayAlert("تأكيد الإزالة", "هل أنت متأكد من إزالة هذه المادة من المجموعة؟", "نعم", "إلغاء");
            if (!confirm) return;

            var success = await _groupSubjectService.RemoveSubjectFromGroupAsync(item.Id);
            if (success)
            {
                _allItems.RemoveAll(x => x.Id == item.Id);
                FilteredGroupSubjects.Remove(item);
                OnPropertyChanged(nameof(IsEmpty));
                await Shell.Current.DisplayAlert("نجاح", "تمت إزالة المادة بنجاح", "موافق");
            }
            else
            {
                await Shell.Current.DisplayAlert("خطأ", "فشل إزالة المادة", "موافق");
            }
        }

        // Navigate to assign subject page
        private async Task OnOpenAssignSubjectExecuted()
        {
            if (Shell.Current == null) return;
            await Shell.Current.GoToAsync(nameof(AddGroupSubjectPage));
        }

        // Navigate to single group-subject management view
        private async Task OnOpenManageSubjectExecuted(GroupSubjectDisplayItem? item)
        {
            if (item == null || Shell.Current == null) return;

            var navigationParameters = new Dictionary<string, object>
            {
                { "GroupSubject", item.GroupSubject }
            };

            await Shell.Current.GoToAsync(nameof(ManageSingleGroupSubjectView), navigationParameters);
        }
    }
}
