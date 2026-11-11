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
        
        builder.Services.AddTransient<AddaUpdateStudentsViewModel>();
        builder.Services.AddTransient<AddUpdateStudent>();

        // Register AddUpdateGroup and its ViewModel
        builder.Services.AddTransient<AddUpdateGroupViewModel>();
        builder.Services.AddTransient<AddUpdateGroup>();

        // Register Majors management page and its ViewModel
        builder.Services.AddTransient<MajorsPage>();
        builder.Services.AddTransient<MajorsViewMod>();

        // --- تسجيل شاشة المجموعات والـ ViewModel الخاص بها ---
        builder.Services.AddTransient<GroupsViewModel>();
        builder.Services.AddTransient<groupsPage>();

        // Register MainLayoutViewModel and MainLayoutView with correct matching names
        builder.Services.AddTransient<MainLayoutViewModel>();
        builder.Services.AddTransient<MainLayoutView>();
        builder.Services.AddTransient<GroupDashboardViewModel>();
        
        // Register GroupDashboardView for DI injection
        builder.Services.AddTransient<GroupDashboardView>();

        // --- Register Attendance Tracking Feature ---
        builder.Services.AddTransient<AttendanceLogPageViewModel>();
        builder.Services.AddTransient<AttendanceLogPage>();

        // --- Register Group Attendance Management Feature (NEW) ---
        builder.Services.AddTransient<GroupAttendanceManagementPageViewModel>();
        builder.Services.AddTransient<GroupAttendanceManagementPage>();
        
        // Register database context and application services
        builder.Services.AddTransient<LucterService>();
        
        // --- Register Academic Settings Feature ---
        builder.Services.AddTransient<AcademicSettingsPage>();
        builder.Services.AddTransient<AcademicSettingsPageViewModel>();

        builder.Services.AddTransient<AboutPage>();

        builder.Services.AddTransient<StudentAttendanceLogPageViewModel>();
        builder.Services.AddTransient<StudentAttendanceLogPage>();
        
        // --- Register Student Details Feature ---
        builder.Services.AddTransient<StudentDetailsPageViewModel>();
        builder.Services.AddTransient<StudentDetailsPage>();

        // --- Register Edit Student Grades Feature (NEW) ---
        builder.Services.AddTransient<EditStudentGradesPageViewModel>();
        builder.Services.AddTransient<EditStudentGradesPage>();

        // -----------------------------------------------------------

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
