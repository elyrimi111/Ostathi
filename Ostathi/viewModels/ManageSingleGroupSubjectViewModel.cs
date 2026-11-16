using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi.ViewModels
{
    // Automatically receive the target GroupSubject object via Shell navigation
    [QueryProperty(nameof(CurrentGroupSubject), "GroupSubject")]
    public partial class ManageSingleGroupSubjectViewModel : ObservableObject
    {
        // Target GroupSubject property
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
            // Initialize commands and navigation routes
            SaveChangesCommand = new AsyncRelayCommand(SaveChangesAsync);
            ExportCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(ExportMasterPage)));
            
            // Open SubjectGroupStudentsAttendanceView with GroupId and SubjectId parameters
            OpenAttendanceCommand = new AsyncRelayCommand(OpenSubjectGroupAttendanceAsync);

            // Open SubjectGroupLecturesPage with GroupId and SubjectId parameters
            OpenLecturesCommand = new AsyncRelayCommand(OpenSubjectGroupLecturesAsync);

            // Open SubjectGroupAssignmentsPage with GroupId and SubjectId parameters
            OpenAssignmentsCommand = new AsyncRelayCommand(OpenSubjectGroupAssignmentsAsync);

            OpenYearWorkCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(YearWorkPage)));
            OpenNotificationsCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(NotificationsPage)));
            OpenMidtermCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(MidtermExamPage)));
            OpenFinalExamCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(FinalExamPage)));
        }

        // Navigate to SubjectGroupStudentsAttendanceView passing GroupId and SubjectId
        private async Task OpenSubjectGroupAttendanceAsync()
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
                await Shell.Current.GoToAsync(
                    $"{nameof(SubjectGroupStudentsAttendanceView)}?GroupId={CurrentGroupSubject.GroupId}&SubjectId={CurrentGroupSubject.SubjectId}");
            }
        }

        // Navigate to SubjectGroupLecturesPage passing GroupId and SubjectId
        private async Task OpenSubjectGroupLecturesAsync()
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
                await Shell.Current.GoToAsync(
                    $"{nameof(SubjectGroupLecturesPage)}?GroupId={CurrentGroupSubject.GroupId}&SubjectId={CurrentGroupSubject.SubjectId}");
            }
        }

        // Navigate to SubjectGroupAssignmentsPage passing GroupId and SubjectId
        private async Task OpenSubjectGroupAssignmentsAsync()
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
                await Shell.Current.GoToAsync(
                    $"{nameof(SubjectGroupAssignmentsPage)}?GroupId={CurrentGroupSubject.GroupId}&SubjectId={CurrentGroupSubject.SubjectId}");
            }
        }

        // Save changes
        private async Task SaveChangesAsync()
        {
            if (CurrentGroupSubject == null) return;
            await Shell.Current.DisplayAlert("نجاح", "تم حفظ التعديلات بنجاح", "موافق");
        }

        // Helper method to navigate to subpages passing GroupSubjectId and GroupId
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
                await Shell.Current.GoToAsync($"{pageName}?GroupSubjectId={CurrentGroupSubject.Id}&GroupId={CurrentGroupSubject.GroupId}");
            }
        }
    }
}
