using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Ostathi.Services;
using Ostathi.ViewModels;

namespace Ostathi.ViewModels
{
    public class MainLayoutViewModel : BindableObject
    {
        private int _selectedIndex = 0;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        private View? _currentView;
        public View? CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly HomeViewModel _homeViewModel;
        private readonly View _homeView;
        private readonly StudentsViewModel _studentsViewModel;
        private readonly View _studentsView;
        private readonly SettingsViewModel _settingsViewModel;
        
        // Fields for Dependency Injection and Views
        private readonly IServiceProvider _serviceProvider;
        private readonly GroupSubjectsManagementView _groupSubjectsManagementView;
        private readonly GroupSubjectsManagementViewModel _groupSubjectsManagementViewModel;
        private readonly View _reportsView = new ReportsPage();

        public ICommand SelectTabCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand NotificationsCommand { get; }
        public ICommand ScheduleCommand { get; }
        public ICommand GroupsCommand { get; }
        public ICommand OpenDynamicAttendanceCommand { get; }
        public ICommand OpenScheduleManagementCommand { get; }

        private bool _isSidebarVisible;
        public bool IsSidebarVisible
        {
            get => _isSidebarVisible;
            set
            {
                _isSidebarVisible = value;
                OnPropertyChanged();
            }
        }

        // Constructor with Dependency Injection
        public MainLayoutViewModel(
            StudentService studentService, 
            IServiceProvider serviceProvider,
            GroupSubjectsManagementView groupSubjectsManagementView,
            GroupSubjectsManagementViewModel groupSubjectsManagementViewModel)
        {
            _serviceProvider = serviceProvider;

            // Initialize Students ViewModel and its View
            _studentsViewModel = new StudentsViewModel(studentService);
            _studentsView = new StudentsPage(_studentsViewModel);

            // Initialize Home ViewModel and its View
            _homeViewModel = new HomeViewModel();
            _homeView = new HomePage(_homeViewModel);

            // Initialize Settings ViewModel
            _settingsViewModel = new SettingsViewModel();

            // Assign injected Group Subjects Management components
            _groupSubjectsManagementView = groupSubjectsManagementView;
            _groupSubjectsManagementViewModel = groupSubjectsManagementViewModel;

            // Set default active view to Home
            _currentView = _homeView;

            // Initialize Commands
            SelectTabCommand = new Command<string>(OnSelectTabExecuted);
            HomeCommand = new Command(OnHomeExecuted);
            SettingsCommand = new Command(OnSettingsExecuted);
            NotificationsCommand = new Command(OnNotificationsExecuted);
            ScheduleCommand = new Command(async () => await OnScheduleExecutedAsync());
            GroupsCommand = new Command(async () => await OnGroupsExecutedAsync());
            
            OpenDynamicAttendanceCommand = new Command(OnOpenDynamicAttendanceExecuted);
            OpenScheduleManagementCommand = new Command(OnOpenScheduleManagementExecuted);
        }

        // Handle bottom navigation tab selection
        private void OnSelectTabExecuted(string indexStr)
        {
            if (int.TryParse(indexStr, out int index))
            {
                SelectedIndex = index;

                switch (index)
                {
                    case 0:
                        HomeCommand?.Execute(null);
                        break;
                    case 1:
                        ScheduleCommand?.Execute(null);
                        break;
                    case 2:
                        GroupsCommand?.Execute(null);
                        break;
                }
            }
        }

        // Switch view to Home
        private void OnHomeExecuted()
        {
            CurrentView = _homeView;
        }

        // Navigate to Settings page
        private async void OnSettingsExecuted()
        {
            var settingsPage = new SettingsPage(_settingsViewModel);
            await Shell.Current.Navigation.PushAsync(settingsPage);
        }

        // Navigate to Dynamic Attendance / Hub creation page
        private async void OnOpenDynamicAttendanceExecuted()
        {
            await Shell.Current.GoToAsync(nameof(CreateHubPage));
        }

        // Navigate to Schedule Management page
        private async void OnOpenScheduleManagementExecuted()
        {
            await Shell.Current.GoToAsync(nameof(ScheduleManagementPage));
        }

        // Switch view to Reports / Notifications
        private void OnNotificationsExecuted()
        {
            CurrentView = _reportsView;
        }

        // Switch view to Students and refresh data
        private async Task OnScheduleExecutedAsync()
        {
            CurrentView = _studentsView;
            
            // Refresh and load students from database whenever the students view is opened
            await _studentsViewModel.LoadStudentsAsync();
        }

        // Switch view to Group Subjects Management screen and refresh matrix data
        private async Task OnGroupsExecutedAsync()
        {
            CurrentView = _groupSubjectsManagementView;
            
            // Reload matrix data and filter options whenever the view is opened
            if (_groupSubjectsManagementViewModel != null)
            {
                await _groupSubjectsManagementViewModel.LoadInitialDataAsync();
            }
        }
    }
}
