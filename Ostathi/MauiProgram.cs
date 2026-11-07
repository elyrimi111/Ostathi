using Microsoft.Extensions.Logging;
using Ostathi.viewModels;

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
        builder.Services.AddTransient<Ostathi.Services.StudentService>();
        builder.Services.AddTransient<MajorService>();
        
        builder.Services.AddTransient<AddaUpdateStudentsViewModel>();
        builder.Services.AddTransient<AddUpdateStudent>();

        // Register Majors management page and its ViewModel
        builder.Services.AddTransient<MajorsPage>();
        builder.Services.AddTransient<MajorsViewMod>();

        // Register MainLayoutViewModel and MainLayoutView with correct matching names
        builder.Services.AddTransient<MainLayoutViewModel>();
        builder.Services.AddTransient<MainLayoutView>();

        // -----------------------------------------------------------

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
