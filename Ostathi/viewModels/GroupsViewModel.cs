using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi.ViewModels
{
    public class GroupsViewModel : BaseViewModel
    {
        private readonly GroupService _groupService;
        private readonly AppDbContext _dbContext;
        private readonly GroupDashboardViewModel _dashboardViewModel;
        private readonly IServiceProvider _serviceProvider;

        public GroupDashboardViewModel DashboardViewModel => _dashboardViewModel;

        private View? _dynamicDashboardContent;
        public View? DynamicDashboardContent
        {
            get => _dynamicDashboardContent;
            set => SetProperty(ref _dynamicDashboardContent, value);
        }

        private ObservableCollection<Group> _groupsList = new();
        public ObservableCollection<Group> GroupsList
        {
            get => _groupsList;
            set => SetProperty(ref _groupsList, value);
        }

        private Group? _selectedGroup;
        public Group? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    if (_selectedGroup != null)
                    {
                        // تعيين الكائن للداشبورد
                        _dashboardViewModel.CurrentGroup = _selectedGroup;
                        
                        // تعيين الـ GroupId صراحة لكي يتم تفعيل الـ QueryProperty وجلب البيانات وتمريرها بنجاح
                        _dashboardViewModel.GroupId = _selectedGroup.Id;
                        
                        HasGroups = true;
                    }
                    else
                    {
                        _dashboardViewModel.CurrentGroup = null;
                        _dashboardViewModel.GroupId = 0; // تصفير المعرف
                        HasGroups = false;
                    }
                }
            }
        }

        // ==========================================
        // 1. الفلاتر والبيانات الاختبارية (المواد، التخصصات، السنوات)
        // ==========================================

        private ObservableCollection<ItemModel> _subjectsList = new();
        public ObservableCollection<ItemModel> SubjectsList
        {
            get => _subjectsList;
            set => SetProperty(ref _subjectsList, value);
        }

        private ItemModel? _selectedSubject;
        public ItemModel? SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                if (SetProperty(ref _selectedSubject, value))
                {
                    FilterGroupsBySelection();
                }
            }
        }

        private ObservableCollection<ItemModel> _specializationsList = new();
        public ObservableCollection<ItemModel> SpecializationsList
        {
            get => _specializationsList;
            set => SetProperty(ref _specializationsList, value);
        }

        private ItemModel? _selectedSpecialization;
        public ItemModel? SelectedSpecialization
        {
            get => _selectedSpecialization;
            set
            {
                if (SetProperty(ref _selectedSpecialization, value))
                {
                    FilterGroupsBySelection();
                }
            }
        }

        private ObservableCollection<ItemModel> _yearsList = new();
        public ObservableCollection<ItemModel> YearsList
        {
            get => _yearsList;
            set => SetProperty(ref _yearsList, value);
        }

        private ItemModel? _selectedYear;
        public ItemModel? SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    FilterGroupsBySelection();
                }
            }
        }

        // ==========================================

        // Flag to check if groups exist (controls dashboard visibility)
        private bool _hasGroups;
        public bool HasGroups
        {
            get => _hasGroups;
            set
            {
                if (SetProperty(ref _hasGroups, value))
                {
                    // Automatically update the inverse property to show/hide empty state view
                    HasNoGroups = !value;
                }
            }
        }

        // Inverse flag to check if no groups exist (controls empty state & alert view visibility)
        private bool _hasNoGroups = true;
        public bool HasNoGroups
        {
            get => _hasNoGroups;
            set => SetProperty(ref _hasNoGroups, value);
        }

        public ICommand OpenAddGroupCommand { get; }

        public GroupsViewModel(
            GroupService groupService,
            AppDbContext dbContext,
            GroupDashboardViewModel dashboardViewModel,
            IServiceProvider serviceProvider)
        {
            _groupService = groupService;
            _dbContext = dbContext;
            _dashboardViewModel = dashboardViewModel;
            _serviceProvider = serviceProvider;

            OpenAddGroupCommand = new Command(async () => await OnOpenAddGroupExecuted());

            // تعبئة البيانات الاختبارية للفلاتر عند التشغيل
            LoadMockFilterData();

            // Subscribe to group creation/update and deletion events
            MessengerService.GroupAdded += OnGroupAddedNotification;
            MessengerService.GroupDeleted += OnGroupAddedNotification;

            InitializeDashboardView();

            // Load groups asynchronously when ViewModel is initialized
            _ = LoadGroupsAsync();
        }

        // دالة تعبئة البيانات الاختبارية للأشرطة الثلاثة في الأعلى
        private void LoadMockFilterData()
        {
            SubjectsList = new ObservableCollection<ItemModel>
            {
                new ItemModel { Name = "برمجة كائنية C#" },
                new ItemModel { Name = "قواعد البيانات SQL" },
                new ItemModel { Name = "تطوير الهواتف MAUI" },
                new ItemModel { Name = "هندسة برمجيات" }
            };
            SelectedSubject = SubjectsList.FirstOrDefault();

            SpecializationsList = new ObservableCollection<ItemModel>
            {
                new ItemModel { Name = "علوم الحاسوب" },
                new ItemModel { Name = "نظم المعلومات" },
                new ItemModel { Name = "تكنولوجيا المعلومات" }
            };
            SelectedSpecialization = SpecializationsList.FirstOrDefault();

            YearsList = new ObservableCollection<ItemModel>
            {
                new ItemModel { Name = "السنة الأولى" },
                new ItemModel { Name = "السنة الثانية" },
                new ItemModel { Name = "السنة الثالثة" },
                new ItemModel { Name = "السنة الرابعة" }
            };
            SelectedYear = YearsList.FirstOrDefault();
        }

        // دالة لتصفية المجموعات بناءً على اختيار المستخدم (يمكن ربطها بقاعدة البيانات لاحقاً)
        private void FilterGroupsBySelection()
        {
            // يمكنك هنا تنفيذ الاستعلام لجلب المجموعات التابعة للمادة والتخصص والسنة المختارة
        }

        // Safely refresh groups list on the UI MainThread when notification arrives
        private void OnGroupAddedNotification()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await LoadGroupsAsync();
            });
        }

        private void InitializeDashboardView()
        {
            var dashboardView = _serviceProvider.GetService<GroupDashboardView>();

            if (dashboardView != null)
            {
                dashboardView.BindingContext = _dashboardViewModel;
                DynamicDashboardContent = dashboardView;
            }
        }

        // Fetch groups from database and update ObservableCollection and States
        public async Task LoadGroupsAsync()
        {
            try
            {
                IsBusy = true; // Show loading indicator via BaseViewModel

                var groups = await _groupService.GetGroupsAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    GroupsList.Clear();
                    if (groups != null)
                    {
                        foreach (var g in groups)
                        {
                            GroupsList.Add(g);
                        }
                    }

                    // Check if groups exist or if all were deleted
                    if (GroupsList.Any())
                    {
                        HasGroups = true; // Groups exist -> Show dashboard and tab strips

                        if (SelectedGroup == null || !GroupsList.Any(g => g.Id == SelectedGroup.Id))
                        {
                            SelectedGroup = GroupsList.First();
                        }
                        else
                        {
                            var currentId = SelectedGroup.Id;
                            SelectedGroup = null;
                            SelectedGroup = GroupsList.FirstOrDefault(g => g.Id == currentId) ?? GroupsList.First();
                        }
                    }
                    else
                    {
                        // If no groups exist at all
                        GroupsList.Clear();
                        SelectedGroup = null;
                        _dashboardViewModel.CurrentGroup = null;
                        HasGroups = false; // No groups -> Hide dashboard and show empty state view automatically
                    }
                });
            }
            catch (Exception ex)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Error", $"Failed to load groups: {ex.Message}", "OK");
                }
            }
            finally
            {
                IsBusy = false; // Hide loading indicator
            }
        }

        private async Task OnOpenAddGroupExecuted()
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync(nameof(AddUpdateGroup));
            }
        }
    }

    // كلاس الموديل الخاص بالعناصر النصية للأشرطة العلوية
    public class ItemModel
    {
        public string Name { get; set; } = string.Empty;
    }
}
