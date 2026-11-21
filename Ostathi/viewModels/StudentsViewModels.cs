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
    public class StudentsViewModel : BaseViewModel
    {
        private readonly StudentService _studentService;
        private readonly AppDbContext _dbContext;
        private readonly SubjectGroupStudentGradeService _gradeService;
        private List<Student> _allStudents = new();

        #region Fields & Properties

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    FilterStudents();
                }
            }
        }

        private string? _selectedSpecialization;
        public string? SelectedSpecialization
        {
            get => _selectedSpecialization;
            set
            {
                if (SetProperty(ref _selectedSpecialization, value))
                {
                    FilterStudents();
                }
            }
        }

        private string? _selectedYear;
        public string? SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    FilterStudents();
                }
            }
        }

        private string? _selectedGroup;
        public string? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    FilterStudents();
                }
            }
        }

        public ObservableCollection<Student> FilteredStudents { get; set; } = new();

        #endregion

        #region Commands

        public ICommand ImportSheetCommand { get; }
        public ICommand AddStudentCommand { get; }
        public ICommand LoadStudentsCommand { get; }
        public ICommand EditStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand StudentTappedCommand { get; }

        #endregion

        #region Constructors

        public StudentsViewModel()
            : this(new StudentService(new AppDbContext()), new AppDbContext(), new SubjectGroupStudentGradeService(new AppDbContext()))
        {
        }

        public StudentsViewModel(StudentService studentService)
            : this(studentService, new AppDbContext(), new SubjectGroupStudentGradeService(new AppDbContext()))
        {
        }

        public StudentsViewModel(
            StudentService studentService,
            AppDbContext dbContext,
            SubjectGroupStudentGradeService gradeService)
        {
            _studentService = studentService ?? new StudentService(new AppDbContext());
            _dbContext = dbContext ?? new AppDbContext();
            _gradeService = gradeService ?? new SubjectGroupStudentGradeService(_dbContext);

            ImportSheetCommand = new Command(async () => await OnImportSheetExecuteAsync());
            AddStudentCommand = new Command(async () => await OnAddStudentExecuteAsync());
            LoadStudentsCommand = new Command(async () => await LoadStudentsAsync());
            EditStudentCommand = new Command<Student>(async (student) => await OnEditStudentExecuteAsync(student));
            DeleteStudentCommand = new Command<Student>(async (student) => await OnDeleteStudentExecuteAsync(student));
            StudentTappedCommand = new Command<Student>(async (student) => await OnStudentTappedExecuteAsync(student));

            MessengerService.StudentAdded += OnStudentAddedNotification;

            _ = LoadStudentsAsync();
        }

        #endregion

        #region Methods

        private void OnStudentAddedNotification()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await LoadStudentsAsync();
            });
        }

        public async Task LoadStudentsAsync()
        {
            try
            {
                IsBusy = true;

                var studentsFromDb = await _studentService.GetStudentsAsync();
                _allStudents = studentsFromDb ?? new List<Student>();

                FilterStudents();
            }
            catch (Exception ex)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Error", $"Failed to load students: {ex.Message}", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OnImportSheetExecuteAsync()
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Import Sheet", "Import sheet button clicked", "OK");
            }
        }

        private async Task OnAddStudentExecuteAsync()
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync(nameof(AddUpdateStudent));
            }
        }

      private async Task OnStudentTappedExecuteAsync(Student? student)
      {
       if (student == null || Shell.Current == null) return;

       int groupSubjectId = 0;
       int gradeId = 0;

       if (student.GroupId > 0)
       {
        var groupSubject = await _dbContext.GroupSubjects
            .AsNoTracking()
            .FirstOrDefaultAsync(gs => gs.GroupId == student.GroupId);

        if (groupSubject != null)
        {
            groupSubjectId = groupSubject.Id;

            var existingGrades = await _gradeService.GetByGroupSubjectIdAsync(groupSubjectId);
            var gradeRecord = existingGrades.FirstOrDefault(g => g.StudentId == student.Id);
            gradeId = gradeRecord?.Id ?? 0;
        }
       }

    await Shell.Current.GoToAsync(
        $"{nameof(StudentDetailsPage)}?StudentId={student.Id}&GroupSubjectId={groupSubjectId}&GradeId={gradeId}");
}

        private async Task OnEditStudentExecuteAsync(Student? student)
        {
            if (student == null || Shell.Current == null) return;

            var navigationParameters = new Dictionary<string, object>
            {
                { "Student", student }
            };

            await Shell.Current.GoToAsync(nameof(AddUpdateStudent), navigationParameters);
        }

        private async Task OnDeleteStudentExecuteAsync(Student? student)
        {
            if (student == null || Shell.Current == null) return;

            string fullName = student.FullName;

            bool confirm = await Shell.Current.DisplayAlert(
                "Delete Confirmation",
                $"Are you sure you want to delete the student '{fullName}'?",
                "Yes",
                "Cancel"
            );

            if (confirm)
            {
                await _studentService.DeleteStudentAsync(student);
                await LoadStudentsAsync();
            }
        }

        private void FilterStudents()
        {
            var query = _allStudents.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = query.Where(s =>
                    s.FullName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(s.AcademicId) && s.AcademicId.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                );
            }

            if (!string.IsNullOrWhiteSpace(SelectedYear) && !SelectedYear.Equals("كل السنوات", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.AcademicYear.Equals(SelectedYear, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SelectedGroup) && !SelectedGroup.Equals("كل المجموعات", StringComparison.OrdinalIgnoreCase))
            {
                int targetGroupId = SelectedGroup switch
                {
                    "مجموعة C#" => 1,
                    "مجموعة قواعد البيانات" => 2,
                    "مجموعة MAUI" => 3,
                    _ => 0
                };

                if (targetGroupId > 0)
                {
                    query = query.Where(s => s.GroupId == targetGroupId);
                }
            }

            var resultsList = query.ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                FilteredStudents.Clear();
                foreach (var student in resultsList)
                {
                    FilteredStudents.Add(student);
                }
            });
        }

        #endregion
    }
}
