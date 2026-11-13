using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi
{
    // استقبال كائن مادة المجموعة المستهدف تلقائياً عبر تنقل الـ Shell
    [QueryProperty(nameof(CurrentGroupSubject), "GroupSubject")]
    public partial class ManageSingleGroupSubjectViewModel : ObservableObject
    {
        // خاصية كائن مادة المجموعة المعين
        [ObservableProperty]
        private GroupSubject? _currentGroupSubject;

        #region Commands for Interactive Cards and Buttons

        public ICommand SaveChangesCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand OpenAttendanceCommand { get; }
        public ICommand OpenYearWorkCommand { get; }
        public ICommand OpenAssignmentsCommand { get; }
        public ICommand OpenNotificationsCommand { get; }
        public ICommand OpenLecturesCommand { get; }
        public ICommand OpenMidtermCommand { get; }
        public ICommand OpenFinalExamCommand { get; }

        #endregion

        public ManageSingleGroupSubjectViewModel()
        {
            // تهيئة الأوامر والانتقالات مع تمرير المعرفات اللازمة للشاشات
            SaveChangesCommand = new AsyncRelayCommand(SaveChangesAsync);
            ExportCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(ExportMasterPage)));
            
            OpenAttendanceCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(AttendancePage)));
            OpenYearWorkCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(YearWorkPage)));
            OpenAssignmentsCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(AssignmentsPage)));
            OpenNotificationsCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(NotificationsPage)));
            OpenLecturesCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(LecturesPage)));
            OpenMidtermCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(MidtermExamPage)));
            OpenFinalExamCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(FinalExamPage)));
        }

        // دفظ التعديلات
        private async Task SaveChangesAsync()
        {
            if (CurrentGroupSubject == null) return;
            await Shell.Current.DisplayAlert("نجاح", "تم حفظ التعديلات بنجاح", "موافق");
        }

        // دالة مساعدة للانتقال إلى الصفحات الفرعية مع تمرير معرّف مادة المجموعة أو المجموعة إن أمكن
        protected virtual async Task GoToAsync(string pageName)
        {
            if (CurrentGroupSubject == null)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("تنبيه", "الرجاء اختيار مادة المجموعة أولاً", "موافق");
                }
                return;
            }

            if (Shell.Current != null)
            {
                // يمكنك تمرير GroupSubjectId أو GroupId بحسب متطلبات الشاشات الفرعية لديك
                await Shell.Current.GoToAsync($"{pageName}?GroupSubjectId={CurrentGroupSubject.Id}&GroupId={CurrentGroupSubject.GroupId}");
            }
        }
    }
}
