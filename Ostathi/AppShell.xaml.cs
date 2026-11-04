using Ostathi;

namespace Ostathi;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register navigation routes for pages used in the application
        Routing.RegisterRoute(nameof(AddUpdateStudent), typeof(AddUpdateStudent));
        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(AttendancePage), typeof(AttendancePage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(CreateHubPage), typeof(CreateHubPage)); // Route for quick add hub
        Routing.RegisterRoute(nameof(ScheduleManagementPage), typeof(ScheduleManagementPage)); // Route for lecture schedule management
        Routing.RegisterRoute(nameof(AssignmentDetailManagementPage), typeof(AssignmentDetailManagementPage)); // Route for single assignment and project management

        // Routes for cumulative attendance, academic year work, assignments, notifications, lectures, midterm exam, final exam, export master, and import roster management pages
        Routing.RegisterRoute(nameof(AttendanceLogPage), typeof(AttendanceLogPage));
        Routing.RegisterRoute(nameof(YearWorkPage), typeof(YearWorkPage));
        Routing.RegisterRoute(nameof(AssignmentsPage), typeof(AssignmentsPage));
        Routing.RegisterRoute(nameof(NotificationsPage), typeof(NotificationsPage));
        Routing.RegisterRoute(nameof(LecturesPage), typeof(LecturesPage));
        
        // تسجيل مسار شاشة إضافة وتعديل المحاضرات
        Routing.RegisterRoute(nameof(AddUpdateLecturePage), typeof(AddUpdateLecturePage));

        Routing.RegisterRoute(nameof(MidtermExamPage), typeof(MidtermExamPage));
        Routing.RegisterRoute(nameof(FinalExamPage), typeof(FinalExamPage));
        Routing.RegisterRoute(nameof(ExportMasterPage), typeof(ExportMasterPage));
        Routing.RegisterRoute(nameof(ImportRosterPage), typeof(ImportRosterPage));
        Routing.RegisterRoute(nameof(ScheduleManagementPage), typeof(ScheduleManagementPage));

    }
}
