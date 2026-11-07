using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi.viewModels
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
        private readonly View _groupsView = new groupsPage();
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

        // Constructor with Dependency Injection for StudentService
        public MainLayoutViewModel(StudentService studentService)
        {
            // Initialize StudentsViewModel with the required StudentService
            _studentsViewModel = new StudentsViewModel(studentService);
            _studentsView = new StudentsPage(_studentsViewModel);

            _homeViewModel = new HomeViewModel();
            _homeView = new HomePage(_homeViewModel);

            _settingsViewModel = new SettingsViewModel();

            _currentView = _homeView;

            SelectTabCommand = new Command<string>(OnSelectTabExecuted);
            HomeCommand = new Command(OnHomeExecuted);
            SettingsCommand = new Command(OnSettingsExecuted);
            NotificationsCommand = new Command(OnNotificationsExecuted);
            ScheduleCommand = new Command(async () => await OnScheduleExecutedAsync());
            GroupsCommand = new Command(OnGroupsExecuted);
            
            OpenDynamicAttendanceCommand = new Command(OnOpenDynamicAttendanceExecuted);
            OpenScheduleManagementCommand = new Command(OnOpenScheduleManagementExecuted);
        }

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

        private void OnHomeExecuted()
        {
            CurrentView = _homeView;
        }

        private async void OnSettingsExecuted()
        {
            var settingsPage = new SettingsPage(_settingsViewModel);
            await Shell.Current.Navigation.PushAsync(settingsPage);
        }

        private async void OnOpenDynamicAttendanceExecuted()
        {
            await Shell.Current.GoToAsync(nameof(CreateHubPage));
        }

        private async void OnOpenScheduleManagementExecuted()
        {
            await Shell.Current.GoToAsync(nameof(ScheduleManagementPage));
        }

        private void OnNotificationsExecuted()
        {
            CurrentView = _reportsView;
        }

        private async Task OnScheduleExecutedAsync()
        {
            CurrentView = _studentsView;
            
            // Refresh and load students from database whenever the students view is opened
            await _studentsViewModel.LoadStudentsAsync();
        }

        private void OnGroupsExecuted()
        {
            CurrentView = _groupsView;
        }
    }
}
