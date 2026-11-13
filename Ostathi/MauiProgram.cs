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

        builder.Services.AddTransient<AddaUpdateStudentsViewModel>();
        builder.Services.AddTransient<AddUpdateStudent>();

        builder.Services.AddTransient<AddUpdateGroupViewModel>();
        builder.Services.AddTransient<AddUpdateGroup>();

        builder.Services.AddTransient<MajorsPage>();
        builder.Services.AddTransient<MajorsViewMod>();

        builder.Services.AddTransient<GroupSubjectsManagementViewModel>();
        builder.Services.AddTransient<GroupSubjectsManagementView>();

        builder.Services.AddTransient<MainLayoutViewModel>();
        builder.Services.AddTransient<MainLayoutView>();
        builder.Services.AddTransient<GroupDashboardViewModel>();
        builder.Services.AddTransient<GroupDashboardView>();

        builder.Services.AddTransient<AttendanceLogPageViewModel>();
        builder.Services.AddTransient<AttendanceLogPage>();

        builder.Services.AddTransient<GroupAttendanceManagementPageViewModel>();
        builder.Services.AddTransient<GroupAttendanceManagementPage>();
        
        builder.Services.AddTransient<LucterService>();
        
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

        // تسجيل شاشة كشف حضور طلاب المادة والمجموعة الجديدة
        builder.Services.AddTransient<SubjectGroupStudentsAttendanceViewModel>();
        builder.Services.AddTransient<SubjectGroupStudentsAttendanceView>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
