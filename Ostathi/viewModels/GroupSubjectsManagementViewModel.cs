using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.EntityFrameworkCore;
using Ostathi.Services;

namespace Ostathi.ViewModels
{
    public class GroupSubjectsManagementViewModel : BaseViewModel
    {
        private readonly GroupSubjectService _groupSubjectService;
        private readonly AppDbContext _dbContext;

        private ObservableCollection<GroupSubject> _allGroupSubjects = new();

        private ObservableCollection<GroupSubject> _filteredGroupSubjects = new();
        public ObservableCollection<GroupSubject> FilteredGroupSubjects
        {
            get => _filteredGroupSubjects;
            set => SetProperty(ref _filteredGroupSubjects, value);
        }

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

        // Commands
        public ICommand RemoveSubjectCommand { get; }
        public ICommand OpenAssignSubjectCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        
        // Command to open single group-subject management screen
        public ICommand OpenManageSubjectCommand { get; }

        public GroupSubjectsManagementViewModel(GroupSubjectService groupSubjectService, AppDbContext dbContext)
        {
            _groupSubjectService = groupSubjectService;
            _dbContext = dbContext;

            RemoveSubjectCommand = new Command<GroupSubject>(async (item) => await OnRemoveSubjectExecuted(item));
            OpenAssignSubjectCommand = new Command(async () => await OnOpenAssignSubjectExecuted());
            ClearFiltersCommand = new Command(OnClearFiltersExecuted);
            
            // Initialize navigation command
            OpenManageSubjectCommand = new Command<GroupSubject>(async (item) => await OnOpenManageSubjectExecuted(item));

            _ = LoadInitialDataAsync();
        }

        public async Task LoadInitialDataAsync()
        {
            try
            {
                IsBusy = true;
                var subjects = await _dbContext.Subjects.ToListAsync();
                var majors = await _dbContext.Majors.ToListAsync();

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

                var allRelations = await _dbContext.GroupSubjects
                    .Include(gs => gs.Group)
                    .Include(gs => gs.Subject)
                    .ToListAsync();

                _allGroupSubjects = new ObservableCollection<GroupSubject>(allRelations);
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
            }
        }

        private void ApplyFilters()
        {
            var query = _allGroupSubjects.AsEnumerable();

            if (SelectedSubject != null)
            {
                query = query.Where(x => x.SubjectId == SelectedSubject.Id);
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                FilteredGroupSubjects = new ObservableCollection<GroupSubject>(query);
            });
        }

        private void OnClearFiltersExecuted()
        {
            SelectedLevel = null;
            SelectedSubject = null;
            SelectedMajor = null;
            ApplyFilters();
        }

        private async Task OnRemoveSubjectExecuted(GroupSubject groupSubject)
        {
            if (groupSubject == null) return;

            bool confirm = await Shell.Current.DisplayAlert("تأكيد الإزالة", "هل أنت متأكد من إزالة هذه المادة من المجموعة؟", "نعم", "إلغاء");
            if (!confirm) return;

            var success = await _groupSubjectService.RemoveSubjectFromGroupAsync(groupSubject.Id);
            if (success)
            {
                _allGroupSubjects.Remove(groupSubject);
                FilteredGroupSubjects.Remove(groupSubject);
                await Shell.Current.DisplayAlert("نجاح", "تمت إزالة المادة بنجاح", "موافق");
            }
            else
            {
                await Shell.Current.DisplayAlert("خطأ", "فشل إزالة المادة", "موافق");
            }
        }

        private async Task OnOpenAssignSubjectExecuted()
        {
            await Shell.Current.DisplayAlert("إضافة مادة", "جارٍ فتح شاشة ربط مادة جديدة بالمجموعة...", "موافق");
        }

        // Execute navigation to the single group-subject view
        private async Task OnOpenManageSubjectExecuted(GroupSubject groupSubject)
        {
            if (groupSubject == null) return;

            var navigationParameters = new Dictionary<string, object>
            {
                { "GroupSubject", groupSubject }
            };

            await Shell.Current.GoToAsync(nameof(ManageSingleGroupSubjectView), navigationParameters);
        }
    }
}
