using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Ostathi.Models;
using Ostathi.Services;

namespace Ostathi.ViewModels
{
    [QueryProperty(nameof(GroupId), nameof(GroupId))]
    [QueryProperty(nameof(SubjectId), nameof(SubjectId))]
    public class SubjectGroupStudentsAttendanceViewModel : BindableObject
    {
        private readonly AppDbContext _dbContext;
        private readonly StudentService _studentService;

        private int _groupId;
        public int GroupId
        {
            get => _groupId;
            set
            {
                if (_groupId != value)
                {
                    _groupId = value;
                    OnPropertyChanged();
                    _ = TryLoadDataAsync();
                }
            }
        }

        private int _subjectId;
        public int SubjectId
        {
            get => _subjectId;
            set
            {
                if (_subjectId != value)
                {
                    _subjectId = value;
                    OnPropertyChanged();
                    _ = TryLoadDataAsync();
                }
            }
        }

        private string _groupName = string.Empty;
        public string GroupName
        {
            get => _groupName;
            set { if (_groupName != value) { _groupName = value; OnPropertyChanged(); } }
        }

        private string _subjectName = string.Empty;
        public string SubjectName
        {
            get => _subjectName;
            set { if (_subjectName != value) { _subjectName = value; OnPropertyChanged(); } }
        }

        private int _totalRequiredLectures = 12;
        public int TotalRequiredLectures
        {
            get => _totalRequiredLectures;
            set
            {
                if (_totalRequiredLectures != value)
                {
                    _totalRequiredLectures = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ConductedLecturesSummary));
                }
            }
        }

        private int _conductedLecturesCount;
        public int ConductedLecturesCount
        {
            get => _conductedLecturesCount;
            set
            {
                if (_conductedLecturesCount != value)
                {
                    _conductedLecturesCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ConductedLecturesSummary));
                }
            }
        }

        public string ConductedLecturesSummary => $"تم إنجاز {ConductedLecturesCount} محاضرة من أصل {TotalRequiredLectures}";

        public ObservableCollection<StudentAttendanceSummary> StudentsAttendanceList { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand StudentSelectedCommand { get; }

        public SubjectGroupStudentsAttendanceViewModel(AppDbContext dbContext, StudentService studentService)
        {
            _dbContext = dbContext;
            _studentService = studentService;
            RefreshCommand = new Command(async () => await LoadAttendanceDataAsync(GroupId, SubjectId));
            
            // Command executed when a student is selected from the list
            StudentSelectedCommand = new Command<StudentAttendanceSummary>(async (studentSummary) => await OnStudentSelectedAsync(studentSummary));
        }

        private async Task OnStudentSelectedAsync(StudentAttendanceSummary studentSummary)
        {
            if (studentSummary == null) return;

            // Navigate to StudentAttendanceLogPage passing GroupId, StudentId, and SubjectId
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync($"{nameof(StudentAttendanceLogPage)}?GroupId={GroupId}&StudentId={studentSummary.StudentId}&SubjectId={SubjectId}");
            }
        }

        private async Task TryLoadDataAsync()
        {
            if (GroupId > 0 && SubjectId > 0)
            {
                await LoadAttendanceDataAsync(GroupId, SubjectId);
            }
        }

        public async Task LoadAttendanceDataAsync(int groupId, int subjectId)
        {
            try
            {
                var group = await _dbContext.Groups.FindAsync(groupId);
                var subject = await _dbContext.Subjects.FindAsync(subjectId);
                
                string targetSubjectName = subject?.Name ?? string.Empty;

                var students = await _studentService.GetStudentsByGroupAsync(groupId);

                var conductedLectures = await _dbContext.Lectures
                    .Where(l => l.GroupId == groupId && l.SubjectName == targetSubjectName)
                    .ToListAsync();

                int conductedCount = conductedLectures.Count;
                var lectureIds = conductedLectures.Select(l => l.Id).ToList();

                var attendanceRecords = await _dbContext.Attendances
                    .Where(a => lectureIds.Contains(a.LectureId) && a.Status == AttendanceStatus.Present)
                    .ToListAsync();

                var summaryList = students.Select(student =>
                {
                    int attended = attendanceRecords.Count(a => a.StudentId == student.Id);

                    return new StudentAttendanceSummary
                    {
                        StudentId = student.Id,
                        StudentName = student.FullName,
                        AcademicNumber = string.IsNullOrEmpty(student.AcademicId) ? $"ID: {student.Id}" : student.AcademicId,
                        AttendedCount = attended,
                        TotalRequiredLectures = TotalRequiredLectures,
                        ConductedLecturesCount = conductedCount
                    };
                }).ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    GroupName = group?.Name ?? $"مجموعة رقم: {groupId}";
                    SubjectName = targetSubjectName;
                    ConductedLecturesCount = conductedCount;

                    StudentsAttendanceList.Clear();
                    foreach (var item in summaryList)
                    {
                        StudentsAttendanceList.Add(item);
                    }
                });
            }
            catch (Exception ex)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("خطأ", $"فشل تحميل بيانات الحضور: {ex.Message}", "موافق");
                }
            }
        }
    }
}
