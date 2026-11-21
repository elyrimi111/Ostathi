using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Ostathi.Services;

namespace Ostathi
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly AppDbContext _dbContext = new();
        private readonly ScheduleService _scheduleService = new();

        #region Properties (Text & State Data)

        private int? _nextLectureId;
        public int? NextLectureId
        {
            get => _nextLectureId;
            set { _nextLectureId = value; OnPropertyChanged(); }
        }

        private string _nextLectureTitle = "لا توجد محاضرات قادمة";
        public string NextLectureTitle
        {
            get => _nextLectureTitle;
            set { _nextLectureTitle = value; OnPropertyChanged(); }
        }

        private string _nextLectureLocation = "📍 المكان: غير محدد";
        public string NextLectureLocation
        {
            get => _nextLectureLocation;
            set { _nextLectureLocation = value; OnPropertyChanged(); }
        }

        private string _nextLectureTime = "⏰ التوقيت: لا يوجد موعد قادم";
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
        public ICommand OpenAssignmentDetailCommand { get; }
        public ICommand RefreshNextLectureCommand { get; }

        #endregion

        #region Constructor

        public HomeViewModel()
        {
            StartAttendanceCommand = new Command(OnStartAttendanceExecute);
            ManageScheduleCommand = new Command(OnManageScheduleExecute);
            ViewAttendanceLogCommand = new Command(OnViewAttendanceLogExecute);
            ContactRepresentativeCommand = new Command(OnContactRepresentativeExecute);
            SendNotificationCommand = new Command(OnSendNotificationExecute);
            OpenAssignmentDetailCommand = new Command(OnOpenAssignmentDetailExecute);
            RefreshNextLectureCommand = new Command(async () => await LoadNextUpcomingLectureAsync());

            MessengerService.LectureAdded += OnLectureAdded;

            _ = LoadNextUpcomingLectureAsync();
        }

        #endregion

        #region Upcoming Lecture Loading

        private void OnLectureAdded()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _ = LoadNextUpcomingLectureAsync();
            });
        }

        public async Task LoadNextUpcomingLectureAsync()
        {
            try
            {
                var allLectures = await _dbContext.Lectures
                    .AsNoTracking()
                    .ToListAsync();

                var upcomingLecture = _scheduleService.GetNextLecture(allLectures, DateTime.Now);

                if (upcomingLecture == null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        NextLectureId = null;
                        NextLectureTitle = "لا توجد محاضرات قادمة حالياً";
                        NextLectureLocation = "📍 المكان: لا توجد قاعة مجدولة";
                        NextLectureTime = "⏰ التوقيت: --:--";
                    });
                    return;
                }

                string subjectName = string.Empty;
                string groupName = string.Empty;

                if (upcomingLecture.GroupSubjectId.HasValue)
                {
                    var groupSubject = await _dbContext.GroupSubjects
                        .AsNoTracking()
                        .FirstOrDefaultAsync(gs => gs.Id == upcomingLecture.GroupSubjectId.Value);

                    if (groupSubject != null)
                    {
                        var subject = await _dbContext.Subjects
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Id == groupSubject.SubjectId);
                        var group = await _dbContext.Groups
                            .AsNoTracking()
                            .FirstOrDefaultAsync(g => g.Id == groupSubject.GroupId);

                        subjectName = subject?.Name ?? string.Empty;
                        groupName = group?.Name ?? string.Empty;
                    }
                }

                string lecturePrefix = upcomingLecture.LectureNumber > 0
                    ? $"محاضرة {upcomingLecture.LectureNumber}"
                    : $"محاضرة #{upcomingLecture.Id}";

                string formattedTitle = (!string.IsNullOrWhiteSpace(subjectName) && !string.IsNullOrWhiteSpace(groupName))
                    ? $"{subjectName} - {groupName}"
                    : (!string.IsNullOrWhiteSpace(subjectName) ? $"{subjectName} ({lecturePrefix})" : lecturePrefix);

                string hallText = string.IsNullOrWhiteSpace(upcomingLecture.HallName)
                    ? "غير محدد"
                    : upcomingLecture.HallName;

                string dayText = ScheduleManagementViewModel.ToArabicDayName(upcomingLecture.DayOfWeek);

                string timeRangeText = upcomingLecture.StartTime != default || upcomingLecture.EndTime != default
                    ? $"{upcomingLecture.StartTime:HH:mm} - {upcomingLecture.EndTime:HH:mm}"
                    : upcomingLecture.LectureDate.ToString("yyyy/MM/dd");

                string timeText = $"{dayText} • {timeRangeText}";

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    NextLectureId = upcomingLecture.Id;
                    NextLectureTitle = formattedTitle;
                    NextLectureLocation = $"📍 المكان: {hallText}";
                    NextLectureTime = $"⏰ التوقيت: {timeText}";
                });
            }
            catch (Exception ex)
            {
                var err = ex.InnerException?.Message ?? ex.Message;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    NextLectureTitle = "تعذر تحميل المحاضرة القادمة";
                    NextLectureLocation = "📍 المكان: --";
                    NextLectureTime = $"⏰ {err}";
                });
            }
        }

        #endregion

        #region Execution Methods & Event Handlers

        private async void OnStartAttendanceExecute()
        {
            if (NextLectureId.HasValue && NextLectureId.Value > 0)
            {
                await Shell.Current.GoToAsync($"{nameof(LectureDetailsPage)}?LectureId={NextLectureId.Value}");
                return;
            }

            await Shell.Current.GoToAsync(nameof(AttendancePage));
        }

        private async void OnManageScheduleExecute()
        {
            await Shell.Current.GoToAsync(nameof(ScheduleManagementPage));
        }

        private void OnViewAttendanceLogExecute()
        {
            App.Current?.MainPage?.DisplayAlert("سجل التحضير", "تم النقر على سجل التحضير والغياب", "حسناً");
        }

        private void OnContactRepresentativeExecute()
        {
            App.Current?.MainPage?.DisplayAlert("التواصل", "جاري فتح نافذة التواصل مع المندوب...", "حسناً");
        }

        private void OnSendNotificationExecute()
        {
            App.Current?.MainPage?.DisplayAlert("الإشعارات", "فتح نافذة إرسال إشعار عام للطلاب", "حسناً");
        }

        private async void OnOpenAssignmentDetailExecute()
        {
            await Shell.Current.GoToAsync(nameof(AssignmentDetailManagementPage));
        }

        #endregion
    }
}
