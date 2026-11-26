using System;
using Microsoft.Extensions.Logging;
using Ostathi.ViewModels;
using Ostathi.Services;

namespace Ostathi;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // --- Register pages and ViewModels for Dependency Injection (DI) ---
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<SettingsViewModel>();

        builder.Services.AddTransient<ScheduleManagementPage>();
        builder.Services.AddTransient<ScheduleManagementViewModel>();

        builder.Services.AddTransient<AddUpdateLecturePage>();
        builder.Services.AddTransient<AddUpdateLectureViewModel>();

        builder.Services.AddTransient<LecturesPage>();
        builder.Services.AddTransient<LecturesViewModel>();

        // Register database context and application services
        builder.Services.AddDbContext<AppDbContext>();
        builder.Services.AddTransient<StudentService>();
        builder.Services.AddTransient<MajorService>();
        builder.Services.AddTransient<GroupService>();
        builder.Services.AddTransient<GroupSubjectService>();
        builder.Services.AddTransient<SubjectService>();
        builder.Services.AddTransient<LucterService>();
        builder.Services.AddTransient<ExportService>();

        builder.Services.AddTransient<AddaUpdateStudentsViewModel>();
        builder.Services.AddTransient<AddUpdateStudent>();

        builder.Services.AddTransient<AddUpdateGroupViewModel>();
        builder.Services.AddTransient<AddUpdateGroup>();

        builder.Services.AddTransient<GroupsPage>();
        builder.Services.AddTransient<GroupsViewModel>();

        // Register Subject pages and ViewModels
        builder.Services.AddTransient<SubjectsPage>();
        builder.Services.AddTransient<SubjectsViewModel>();
        builder.Services.AddTransient<AddUpdateSubject>();
        builder.Services.AddTransient<AddUpdateSubjectViewModel>();

        builder.Services.AddTransient<GroupSubjectsPageViewModel>();
        builder.Services.AddTransient<GroupSubjectsPage>();

        builder.Services.AddTransient<GroupSubjectStudentsViewModel>();
        builder.Services.AddTransient<GroupSubjectStudentsPage>();

        builder.Services.AddTransient<AddGroupSubjectPage>();
        builder.Services.AddTransient<AddUpdateGroupSubjectViewModel>();

        builder.Services.AddTransient<MajorsPage>();
        builder.Services.AddTransient<MajorsViewMod>();

        builder.Services.AddTransient<GroupSubjectsManagementViewModel>();
        builder.Services.AddTransient<GroupSubjectsManagementView>();

        builder.Services.AddTransient<MainLayoutViewModel>();
        builder.Services.AddTransient<MainLayoutView>();
        builder.Services.AddTransient<GroupDashboardViewModel>();
        builder.Services.AddTransient<GroupDashboardView>();

        builder.Services.AddTransient<CreateHubPage>();
        builder.Services.AddTransient<CreateHubViewModel>();

        builder.Services.AddTransient<AttendanceLogPageViewModel>();
        builder.Services.AddTransient<AttendanceLogPage>();

        builder.Services.AddTransient<GroupAttendanceManagementPageViewModel>();
        builder.Services.AddTransient<GroupAttendanceManagementPage>();

        builder.Services.AddTransient<AcademicSettingsPage>();
        builder.Services.AddTransient<AcademicSettingsPageViewModel>();

        builder.Services.AddTransient<AboutPage>();

        builder.Services.AddTransient<StudentAttendanceLogPageViewModel>();
        builder.Services.AddTransient<StudentAttendanceLogPage>();

        builder.Services.AddTransient<StudentDetailsPageViewModel>();
        builder.Services.AddTransient<StudentDetailsPage>();

        builder.Services.AddTransient<EditStudentGradesPageViewModel>();
        builder.Services.AddTransient<EditStudentGradesPage>();

        builder.Services.AddTransient<ManageSingleGroupSubjectViewModel>();
        builder.Services.AddTransient<ManageSingleGroupSubjectView>();

        builder.Services.AddTransient<SubjectGroupStudentsAttendanceViewModel>();
        builder.Services.AddTransient<SubjectGroupStudentsAttendanceView>();

        builder.Services.AddTransient<SubjectGroupLecturesViewModel>();
        builder.Services.AddTransient<SubjectGroupLecturesPage>();

        builder.Services.AddTransient<LectureDetailsPage>();
        builder.Services.AddTransient<LectureDetailsViewModel>();

        builder.Services.AddTransient<SubjectGroupAssignmentsPage>();
        builder.Services.AddTransient<SubjectGroupAssignmentsViewModel>();

        builder.Services.AddTransient<AddUpdateAssignmentPage>();
        builder.Services.AddTransient<AddUpdateAssignmentViewModel>();

        builder.Services.AddTransient<AssignmentDetailManagementPage>();
        builder.Services.AddTransient<AssignmentDetailManagementViewModel>();

        // Register Notifications Page and ViewModel
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<NotificationsViewModel>();

        // Register Subject Group Grades Services, ViewModels and Pages
        builder.Services.AddTransient<SubjectGroupStudentGradeService>();

        // ViewModel الخاص بنظرة عامة على درجات الطلاب
        builder.Services.AddTransient<SubjectGroupGradesOverviewViewModel>();
        builder.Services.AddTransient<SubjectGroupGradesOverviewPage>();

        builder.Services.AddTransient<SubjectGroupStudentGradeViewModel>();
        builder.Services.AddTransient<SubjectGroupStudentGradeDetailsPage>();

        builder.Services.AddTransient<StudentCommunicationPage>();
        builder.Services.AddTransient<StudentCommunicationViewModel>();

        builder.Services.AddTransient<ExportMasterPage>();
        builder.Services.AddTransient<ExportMasterViewModel>();

        builder.Services.AddTransient<ImportGroupPage>();
        builder.Services.AddTransient<ImportGroupViewModel>();

        builder.Services.AddTransient<AddUpdateMajorViewModel>();
        builder.Services.AddTransient<AddUpdateMajorPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
