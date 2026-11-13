using System;
using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register navigation routes for pages used in the application
        Routing.RegisterRoute(nameof(AddUpdateStudent), typeof(AddUpdateStudent));
        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(CreateHubPage), typeof(CreateHubPage)); // Route for quick add hub
        Routing.RegisterRoute(nameof(ScheduleManagementPage), typeof(ScheduleManagementPage)); // Route for lecture schedule management
        Routing.RegisterRoute(nameof(AssignmentDetailManagementPage), typeof(AssignmentDetailManagementPage)); // Route for single assignment and project management

        // Routes for daily attendance, cumulative attendance, academic year work, assignments, notifications, and lectures management pages
        Routing.RegisterRoute(nameof(AttendancePage), typeof(AttendancePage)); // Route for daily attendance recording
        Routing.RegisterRoute(nameof(AttendanceLogPage), typeof(AttendanceLogPage)); // Route for cumulative attendance log
        Routing.RegisterRoute(nameof(GroupAttendanceManagementPage), typeof(GroupAttendanceManagementPage)); // Route for group attendance management
        Routing.RegisterRoute(nameof(StudentAttendanceLogPage), typeof(StudentAttendanceLogPage)); // Route for individual student attendance log page
        Routing.RegisterRoute(nameof(YearWorkPage), typeof(YearWorkPage));
        Routing.RegisterRoute(nameof(AssignmentsPage), typeof(AssignmentsPage));
        Routing.RegisterRoute(nameof(NotificationsPage), typeof(NotificationsPage));
        Routing.RegisterRoute(nameof(LecturesPage), typeof(LecturesPage));
        
        // Route for Add/Update Lecture page
        Routing.RegisterRoute(nameof(AddUpdateLecturePage), typeof(AddUpdateLecturePage));

        // Route for Add/Update Group page
        Routing.RegisterRoute(nameof(AddUpdateGroup), typeof(AddUpdateGroup));

        // Route for Group Subjects Management (NEW)
        Routing.RegisterRoute(nameof(GroupSubjectsManagementView), typeof(GroupSubjectsManagementView));

        // Routes for exams and data import/export management pages
        Routing.RegisterRoute(nameof(MidtermExamPage), typeof(MidtermExamPage));
        Routing.RegisterRoute(nameof(FinalExamPage), typeof(FinalExamPage));
        Routing.RegisterRoute(nameof(ExportMasterPage), typeof(ExportMasterPage));
        Routing.RegisterRoute(nameof(ImportRosterPage), typeof(ImportRosterPage));
        
        // Route for Majors management page
        Routing.RegisterRoute(nameof(MajorsPage), typeof(MajorsPage));
        
        // Route for Academic Settings page
        Routing.RegisterRoute(nameof(AcademicSettingsPage), typeof(AcademicSettingsPage));

        // Route for About page
        Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
        
        // Route for Student Details page
        Routing.RegisterRoute(nameof(StudentDetailsPage), typeof(StudentDetailsPage));

        // Route for Edit Student Grades page
        Routing.RegisterRoute(nameof(EditStudentGradesPage), typeof(EditStudentGradesPage));
        
        // Route for Manage Single Group Subject (NEW)
        Routing.RegisterRoute(nameof(ManageSingleGroupSubjectView), typeof(ManageSingleGroupSubjectView));

        // Route for Subject Group Students Attendance Matrix (NEW)
        Routing.RegisterRoute(nameof(SubjectGroupStudentsAttendanceView), typeof(SubjectGroupStudentsAttendanceView));
    }
}
