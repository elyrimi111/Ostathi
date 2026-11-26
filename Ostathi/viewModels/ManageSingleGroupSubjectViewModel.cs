using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Ostathi;

namespace Ostathi.ViewModels
{
    // Automatically receive the target GroupSubject object via Shell navigation
    [QueryProperty(nameof(CurrentGroupSubject), "GroupSubject")]
    public partial class ManageSingleGroupSubjectViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext = new();

        [ObservableProperty]
        private GroupSubject? _currentGroupSubject;

        [ObservableProperty]
        private string _groupName = "غير محدد";

        [ObservableProperty]
        private string _subjectName = "غير محدد";

        #region Commands for Interactive Cards and Buttons

        public ICommand ExportCommand { get; }
        public ICommand OpenAttendanceCommand { get; }
        public ICommand OpenAssignmentsCommand { get; }
        public ICommand OpenStudentCommunicationCommand { get; }
        public ICommand OpenLecturesCommand { get; }
        public ICommand OpenGradesCommand { get; }
        public ICommand OpenGroupStudentsCommand { get; }

        #endregion

        public ManageSingleGroupSubjectViewModel()
        {
            ExportCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(ExportMasterPage)));
            OpenAttendanceCommand = new AsyncRelayCommand(OpenSubjectGroupAttendanceAsync);
            OpenAssignmentsCommand = new AsyncRelayCommand(OpenSubjectGroupAssignmentsAsync);
            OpenLecturesCommand = new AsyncRelayCommand(OpenSubjectGroupLecturesAsync);
            OpenStudentCommunicationCommand = new AsyncRelayCommand(OpenStudentCommunicationAsync);
            OpenGradesCommand = new AsyncRelayCommand(async () => await GoToAsync(nameof(SubjectGroupGradesOverviewPage)));
            OpenGroupStudentsCommand = new AsyncRelayCommand(OpenGroupStudentsAsync);
        }

        partial void OnCurrentGroupSubjectChanged(GroupSubject? value)
        {
            if (value == null)
            {
                GroupName = "غير محدد";
                SubjectName = "غير محدد";
                return;
            }

            _ = LoadGroupAndSubjectNamesAsync(value.GroupId, value.SubjectId);
        }

        private async Task LoadGroupAndSubjectNamesAsync(int groupId, int subjectId)
        {
            try
            {
                var group = await _dbContext.Groups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == groupId);

                var subject = await _dbContext.Subjects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == subjectId);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    GroupName = group?.Name ?? $"مجموعة #{groupId}";
                    SubjectName = subject?.Name ?? $"مادة #{subjectId}";
                });
            }
            catch
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    GroupName = $"مجموعة #{groupId}";
                    SubjectName = $"مادة #{subjectId}";
                });
            }
        }

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

        private async Task OpenGroupStudentsAsync()
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
                    $"{nameof(GroupSubjectStudentsPage)}?GroupId={CurrentGroupSubject.GroupId}&SubjectId={CurrentGroupSubject.SubjectId}&GroupSubjectId={CurrentGroupSubject.Id}");
            }
        }

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
