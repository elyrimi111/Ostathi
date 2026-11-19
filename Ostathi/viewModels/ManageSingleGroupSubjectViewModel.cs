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

        public ICommand ExportCommand { get; }
        public ICommand OpenAttendanceCommand { get; }
        public ICommand OpenAssignmentsCommand { get; }
        public ICommand OpenStudentCommunicationCommand { get; }
        public ICommand OpenLecturesCommand { get; }
        public ICommand OpenGradesCommand { get; }

        #endregion

        public ManageSingleGroupSubjectViewModel()
        {
            // Initialize commands corresponding strictly to the current UI buttons
            ExportCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(ExportMasterPage)));
            
            // Open SubjectGroupStudentsAttendanceView with GroupId and SubjectId parameters
            OpenAttendanceCommand = new AsyncRelayCommand(OpenSubjectGroupAttendanceAsync);

            // Open SubjectGroupAssignmentsPage with GroupId and SubjectId parameters
            OpenAssignmentsCommand = new AsyncRelayCommand(OpenSubjectGroupAssignmentsAsync);

            // Open SubjectGroupLecturesPage with GroupId and SubjectId parameters
            OpenLecturesCommand = new AsyncRelayCommand(OpenSubjectGroupLecturesAsync);

            // الانتقال إلى شاشة "التواصل مع الطلاب" وتمرير GroupId
            OpenStudentCommunicationCommand = new AsyncRelayCommand(OpenStudentCommunicationAsync);
            
            // ربط أمر الدرجات بشاشة النظرة العامة الجديدة لدرجات مجموعة المادة
            OpenGradesCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(SubjectGroupGradesOverviewPage)));
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

        // الانتقال إلى شاشة التواصل مع الطلاب والمندوب وتمرير GroupId
        private async Task OpenStudentCommunicationAsync()
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
                await Shell.Current.GoToAsync($"{nameof(StudentCommunicationPage)}?GroupId={CurrentGroupSubject.GroupId}");
            }
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
