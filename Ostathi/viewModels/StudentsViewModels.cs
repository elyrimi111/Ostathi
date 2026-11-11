using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi
{
    public class StudentsViewModel : BaseViewModel
    {
        private readonly StudentService _studentService;
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
        public ICommand StudentTappedCommand { get; } // أمر الضغط على الطالب لعرض التفاصيل

        #endregion

        #region Constructors

        // Default constructor fallback for XAML / DI instantiations
        public StudentsViewModel() : this(new StudentService(new AppDbContext()))
        {
        }

        public StudentsViewModel(StudentService studentService)
        {
            _studentService = studentService ?? new StudentService(new AppDbContext());

            // Initialize commands
            ImportSheetCommand = new Command(async () => await OnImportSheetExecuteAsync());
            AddStudentCommand = new Command(async () => await OnAddStudentExecuteAsync());
            LoadStudentsCommand = new Command(async () => await LoadStudentsAsync());
            EditStudentCommand = new Command<Student>(async (student) => await OnEditStudentExecuteAsync(student));
            DeleteStudentCommand = new Command<Student>(async (student) => await OnDeleteStudentExecuteAsync(student));
            StudentTappedCommand = new Command<Student>(async (student) => await OnStudentTappedExecuteAsync(student));

            // Subscribe to real-time additions/updates notification
            MessengerService.StudentAdded += OnStudentAddedNotification;

            // Automatically load students from database upon instantiation
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

        // Fetch all students from the database asynchronously
        public async Task LoadStudentsAsync()
        {
            try
            {
                IsBusy = true;

                // Load initial real data directly from SQLite via StudentService
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

        // Navigate to StudentDetailsPage when tapping a student card
        private async Task OnStudentTappedExecuteAsync(Student? student)
        {
            if (student == null || Shell.Current == null) return;

            // Go to StudentDetailsPage passing StudentId via query parameters
            await Shell.Current.GoToAsync($"{nameof(StudentDetailsPage)}?StudentId={student.Id}");
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

        // Filter student list dynamically by Search Query, Academic Year, and Group
        private void FilterStudents()
        {
            var query = _allStudents.AsEnumerable();

            // 1. Apply search query filter (matches FullName or AcademicId)
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = query.Where(s => 
                    s.FullName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(s.AcademicId) && s.AcademicId.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                );
            }

            // 2. Apply Academic Year filter
            if (!string.IsNullOrWhiteSpace(SelectedYear) && !SelectedYear.Equals("كل السنوات", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.AcademicYear.Equals(SelectedYear, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Apply Group filter
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

            // Update collection items directly on the UI thread
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
