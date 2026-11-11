using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi
{
    public class HomeViewModel : BaseViewModel
    {
        #region Properties (Text & State Data)

        private string _nextLectureTitle = "مجموعة البرمجة المتقدمة (C#)";
        public string NextLectureTitle
        {
            get => _nextLectureTitle;
            set { _nextLectureTitle = value; OnPropertyChanged(); }
        }

        private string _nextLectureLocation = "📍 المكان: قاعة 3 - مبنى الحاسب الآلي";
        public string NextLectureLocation
        {
            get => _nextLectureLocation;
            set { _nextLectureLocation = value; OnPropertyChanged(); }
        }

        private string _nextLectureTime = "⏰ التوقيت: 10:00 صباحاً - 12:00 ظهراً";
        public string NextLectureTime
        {
            get => _nextLectureTime;
            set { _nextLectureTime = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands (UI Actions & Interactions)

        public ICommand StartAttendanceCommand { get; }
        public ICommand ManageScheduleCommand { get; }
        public ICommand ViewAttendanceLogCommand { get; }
        public ICommand ContactRepresentativeCommand { get; }
        public ICommand SendNotificationCommand { get; }
        public ICommand OpenAssignmentDetailCommand { get; } // أمر فتح شاشة إدارة التكليف/المشروع

        #endregion

        #region Constructor

        public HomeViewModel()
        {
            // Initialize and bind commands to their respective methods
            StartAttendanceCommand = new Command(OnStartAttendanceExecute);
            ManageScheduleCommand = new Command(OnManageScheduleExecute);
            ViewAttendanceLogCommand = new Command(OnViewAttendanceLogExecute);
            ContactRepresentativeCommand = new Command(OnContactRepresentativeExecute);
            SendNotificationCommand = new Command(OnSendNotificationExecute);
            OpenAssignmentDetailCommand = new Command(OnOpenAssignmentDetailExecute); // تهيئة الأمر الجديد
        }

        #endregion

        #region Execution Methods & Event Handlers

        private async void OnStartAttendanceExecute()
        {
            // Navigate to the attendance page (Ensure AttendancePage route is registered in AppShell)
            await Shell.Current.GoToAsync(nameof(AttendancePage));
        }

        private async void OnManageScheduleExecute()
        {
            // Navigate directly to schedule management page
            await Shell.Current.GoToAsync(nameof(ScheduleManagementPage));
        }

        private void OnViewAttendanceLogExecute()
        {
            // Logic to open attendance and absence logs
            App.Current?.MainPage?.DisplayAlert("سجل التحضير", "تم النقر على سجل التحضير والغياب", "حسناً");
        }

        private void OnContactRepresentativeExecute()
        {
            // Logic to communicate with the batch representative
            App.Current?.MainPage?.DisplayAlert("التواصل", "جاري فتح نافذة التواصل مع المندوب...", "حسناً");
        }

        private void OnSendNotificationExecute()
        {
            // Logic to send a broadcast notification to students
            App.Current?.MainPage?.DisplayAlert("الإشعارات", "فتح نافذة إرسال إشعار عام للطلاب", "حسناً");
        }

        private async void OnOpenAssignmentDetailExecute()
        {
            // الانتقال إلى شاشة إدارة التكليف الواحد عند الضغط على أي بطاقة من أسفل الصفحة
            await Shell.Current.GoToAsync(nameof(AssignmentDetailManagementPage));
        }

        #endregion
    }
}
