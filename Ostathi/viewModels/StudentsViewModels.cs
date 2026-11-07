using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ostathi.Models;
using Ostathi.Services;

namespace Ostathi
{
    public class StudentsViewModel : BindableObject
    {
        private readonly StudentService? _studentService;
        private List<Student> _allStudents = new();

        #region Fields & Properties

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    FilterStudents(); // Trigger filtering when search text changes
                }
            }
        }

        private string? _selectedSpecialization;
        public string? SelectedSpecialization
        {
            get => _selectedSpecialization;
            set
            {
                if (_selectedSpecialization != value)
                {
                    _selectedSpecialization = value;
                    OnPropertyChanged();
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
                if (_selectedYear != value)
                {
                    _selectedYear = value;
                    OnPropertyChanged();
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
                if (_selectedGroup != value)
                {
                    _selectedGroup = value;
                    OnPropertyChanged();
                    FilterStudents();
                }
            }
        }

        // Collection of filtered students bound to the UI CollectionView
        private ObservableCollection<Student> _filteredStudents;
        public ObservableCollection<Student> FilteredStudents
        {
            get => _filteredStudents;
            set
            {
                _filteredStudents = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand ImportSheetCommand { get; }
        public ICommand AddStudentCommand { get; }
        public ICommand LoadStudentsCommand { get; }
        public ICommand EditStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }

        #endregion

        #region Constructors

        // Default parameterless constructor
        public StudentsViewModel() : this(null!)
        {
        }

        // Primary constructor used for Dependency Injection
        public StudentsViewModel(StudentService? studentService)
        {
            _studentService = studentService;

            // Initialize commands
            ImportSheetCommand = new Command(async () => await OnImportSheetExecuteAsync());
            AddStudentCommand = new Command(async () => await OnAddStudentExecuteAsync());
            LoadStudentsCommand = new Command(async () => await LoadStudentsAsync());
            EditStudentCommand = new Command<Student>(async (student) => await OnEditStudentExecuteAsync(student));
            DeleteStudentCommand = new Command<Student>(async (student) => await OnDeleteStudentExecuteAsync(student));

            // Initialize collection
            FilteredStudents = new ObservableCollection<Student>();

            // Subscribe to real-time updates via MessengerService
            MessengerService.StudentAdded += async () =>
            {
                await LoadStudentsAsync();
            };

            // Automatically load students from database upon instantiation
            _ = LoadStudentsAsync();
        }

        #endregion

        #region Methods

        // Fetch all students from SQLite database via StudentService
        public async Task LoadStudentsAsync()
        {
            if (_studentService == null) return;

            try
            {
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
            // Navigate to Add/Update student page using modern Shell routing
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync(nameof(AddUpdateStudent));
            }
        }

        private async Task OnEditStudentExecuteAsync(Student? student)
        {
            if (student == null || Shell.Current == null) return;

            // Pass the student object to AddUpdateStudent page via navigation parameters
            var navigationParameters = new Dictionary<string, object>
            {
                { "Student", student }
            };

            await Shell.Current.GoToAsync(nameof(AddUpdateStudent), navigationParameters);
        }

        private async Task OnDeleteStudentExecuteAsync(Student? student)
        {
            if (student == null || _studentService == null || Shell.Current == null) return;

            string fullName = $"{student.FirstName} {student.LastName}".Trim();

            bool confirm = await Shell.Current.DisplayAlert(
                "تأكيد الحذف",
                $"هل أنت متأكد من رغبتك في حذف الطالب '{fullName}'؟",
                "نعم",
                "إلغاء"
            );

            if (confirm)
            {
                await _studentService.DeleteStudentAsync(student);
                await LoadStudentsAsync();
            }
        }

        // Filter the student list based on search query, academic year, and group
        private void FilterStudents()
        {
            var query = _allStudents.AsEnumerable();

            // 1. Apply search filter (by full name concatenation or Academic ID)
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = query.Where(s => 
                    ($"{s.FirstName} {s.SecondName} {s.ThirdName} {s.LastName}".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.AcademicId) && s.AcademicId.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                );
            }

            // 2. Apply Academic Year filter (ignoring default selection)
            if (!string.IsNullOrWhiteSpace(SelectedYear) && !SelectedYear.Equals("كل السنوات", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.AcademicYear.Equals(SelectedYear, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Apply Group filter (ignoring default selection)
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

            var finalResults = new ObservableCollection<Student>(query);

            // Update the observable collection for UI binding safely on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                FilteredStudents = finalResults;
            });
        }

        #endregion
    }
}
