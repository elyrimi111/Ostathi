using Microsoft.Extensions.Logging;

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
                // The file names are the ones under Resources/Fonts; the second argument is the
                // alias a FontFamily setter uses. Both are resolved against that folder rather
                // than against the APK's assets/, which belongs to the IDE (roadmap 20.3.3).
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // --- تسجيل الصفحات والـ ViewModels لنظام حقن التبعيات (DI) ---
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<SettingsViewModel>();

        builder.Services.AddTransient<ScheduleManagementPage>();
        builder.Services.AddTransient<ScheduleManagementViewModel>();

        builder.Services.AddTransient<AddUpdateLecturePage>();
        builder.Services.AddTransient<AddUpdateLectureViewModel>();

        builder.Services.AddTransient<LecturesPage>();
        builder.Services.AddTransient<LecturesViewModel>();
        // -----------------------------------------------------------

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
