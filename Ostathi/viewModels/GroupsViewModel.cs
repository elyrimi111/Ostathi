using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi.ViewModels
{
    public class GroupsViewModel : BindableObject
    {
        #region Commands

        public ICommand OpenAddGroupCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand OpenAttendanceCommand { get; }
        public ICommand OpenYearWorkCommand { get; }
        public ICommand OpenAssignmentsCommand { get; }
        public ICommand OpenNotificationsCommand { get; }
        public ICommand OpenLecturesCommand { get; }
        public ICommand OpenMidtermCommand { get; }
        public ICommand OpenFinalExamCommand { get; }

        #endregion

        #region Constructor

        public GroupsViewModel()
        {
            // ربط الأوامر بدوال التنفيذ الخاصة بالتنقل والتحكم
            OpenAddGroupCommand = new Command(async () => await OnOpenAddGroupExecuted());
            ImportCommand = new Command(async () => await OnImportExecuted());
            ExportCommand = new Command(async () => await OnExportExecuted());
            OpenAttendanceCommand = new Command(async () => await OnOpenAttendanceExecuted());
            OpenYearWorkCommand = new Command(async () => await OnOpenYearWorkExecuted());
            OpenAssignmentsCommand = new Command(async () => await OnOpenAssignmentsExecuted());
            OpenNotificationsCommand = new Command(async () => await OnOpenNotificationsExecuted());
            OpenLecturesCommand = new Command(async () => await OnOpenLecturesExecuted());
            OpenMidtermCommand = new Command(async () => await OnOpenMidtermExecuted());
            OpenFinalExamCommand = new Command(async () => await OnOpenFinalExamExecuted());
        }

        #endregion

        #region Execution Methods

        private async Task OnOpenAddGroupExecuted()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new AddUpdateGroup());
        }

        private async Task OnImportExecuted()
        {
            // الانتقال إلى صفحة استيراد الكشف
            await Shell.Current.GoToAsync(nameof(ImportRosterPage));
        }

        private async Task OnExportExecuted()
        {
            // الانتقال إلى صفحة التصدير الشامل
            await Shell.Current.GoToAsync(nameof(ExportMasterPage));
        }

        private async Task OnOpenAttendanceExecuted()
        {
            // الانتقال إلى صفحة سجل الحضور
            await Shell.Current.GoToAsync(nameof(AttendanceLogPage));
        }

        private async Task OnOpenYearWorkExecuted()
        {
            // الانتقال إلى صفحة أعمال السنة
            await Shell.Current.GoToAsync(nameof(YearWorkPage));
        }

        private async Task OnOpenAssignmentsExecuted()
        {
            // الانتقال إلى صفحة التكاليف
            await Shell.Current.GoToAsync(nameof(AssignmentsPage));
        }

        private async Task OnOpenNotificationsExecuted()
        {
            // الانتقال إلى صفحة الإشعارات
            await Shell.Current.GoToAsync(nameof(NotificationsPage));
        }

        private async Task OnOpenLecturesExecuted()
        {
            // الانتقال إلى صفحة المحاضرات
            await Shell.Current.GoToAsync(nameof(LecturesPage));
        }

        private async Task OnOpenMidtermExecuted()
        {
            // الانتقال إلى صفحة الاختبار النصفي
            await Shell.Current.GoToAsync(nameof(MidtermExamPage));
        }

        private async Task OnOpenFinalExamExecuted()
        {
            // الانتقال إلى صفحة الاختبار النهائي
            await Shell.Current.GoToAsync(nameof(FinalExamPage));
        }

        #endregion
    }
}
