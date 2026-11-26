using System;
using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Main & Hub Routes (do not register MainLayoutView here if defined in AppShell.xaml)
        Routing.RegisterRoute(nameof(CreateHubPage), typeof(CreateHubPage));
        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(ScheduleManagementPage), typeof(ScheduleManagementPage));

        // Add / Update Routes
        Routing.RegisterRoute(nameof(AddUpdateStudent), typeof(AddUpdateStudent));
        Routing.RegisterRoute(nameof(AddUpdateGroup), typeof(AddUpdateGroup));
        Routing.RegisterRoute(nameof(AddUpdateLecturePage), typeof(AddUpdateLecturePage));
        Routing.RegisterRoute(nameof(AddUpdateAssignmentPage), typeof(AddUpdateAssignmentPage));
        Routing.RegisterRoute(nameof(AddGroupSubjectPage), typeof(AddGroupSubjectPage));
        Routing.RegisterRoute(nameof(AddUpdateSubject), typeof(AddUpdateSubject));
        Routing.RegisterRoute(nameof(AddUpdateMajorPage), typeof(AddUpdateMajorPage));

        // Attendance, Year Work & Assignments Routes
        Routing.RegisterRoute(nameof(AttendancePage), typeof(AttendancePage));
        Routing.RegisterRoute(nameof(AttendanceLogPage), typeof(AttendanceLogPage));
        Routing.RegisterRoute(nameof(GroupAttendanceManagementPage), typeof(GroupAttendanceManagementPage));
        Routing.RegisterRoute(nameof(StudentAttendanceLogPage), typeof(StudentAttendanceLogPage));
        Routing.RegisterRoute(nameof(YearWorkPage), typeof(YearWorkPage));
        Routing.RegisterRoute(nameof(AssignmentsPage), typeof(AssignmentsPage));
        Routing.RegisterRoute(nameof(AssignmentDetailManagementPage), typeof(AssignmentDetailManagementPage));
        Routing.RegisterRoute(nameof(NotificationsPage), typeof(NotificationsPage));
        Routing.RegisterRoute(nameof(LecturesPage), typeof(LecturesPage));

        // Group Subjects & Roster Management Routes
        Routing.RegisterRoute(nameof(GroupSubjectsPage), typeof(GroupSubjectsPage));
        Routing.RegisterRoute(nameof(GroupSubjectsManagementView), typeof(GroupSubjectsManagementView));
        Routing.RegisterRoute(nameof(ManageSingleGroupSubjectView), typeof(ManageSingleGroupSubjectView));
        Routing.RegisterRoute(nameof(GroupSubjectStudentsPage), typeof(GroupSubjectStudentsPage));
        Routing.RegisterRoute(nameof(SubjectGroupStudentsAttendanceView), typeof(SubjectGroupStudentsAttendanceView));
        Routing.RegisterRoute(nameof(SubjectGroupLecturesPage), typeof(SubjectGroupLecturesPage));
        Routing.RegisterRoute(nameof(LectureDetailsPage), typeof(LectureDetailsPage));
        Routing.RegisterRoute(nameof(SubjectGroupAssignmentsPage), typeof(SubjectGroupAssignmentsPage));
        Routing.RegisterRoute(nameof(SubjectGroupGradesOverviewPage), typeof(SubjectGroupGradesOverviewPage));
        Routing.RegisterRoute(nameof(SubjectGroupStudentGradeDetailsPage), typeof(SubjectGroupStudentGradeDetailsPage));
        Routing.RegisterRoute(nameof(StudentCommunicationPage), typeof(StudentCommunicationPage));

        // Exams, Import/Export & Settings Routes
        Routing.RegisterRoute(nameof(MidtermExamPage), typeof(MidtermExamPage));
        Routing.RegisterRoute(nameof(FinalExamPage), typeof(FinalExamPage));
        Routing.RegisterRoute(nameof(ExportMasterPage), typeof(ExportMasterPage));
        Routing.RegisterRoute(nameof(ImportRosterPage), typeof(ImportRosterPage));
        Routing.RegisterRoute(nameof(ImportGroupPage), typeof(ImportGroupPage));
        Routing.RegisterRoute(nameof(MajorsPage), typeof(MajorsPage));
        Routing.RegisterRoute(nameof(GroupsPage), typeof(GroupsPage));
        Routing.RegisterRoute(nameof(SubjectsPage), typeof(SubjectsPage));
        Routing.RegisterRoute(nameof(AcademicSettingsPage), typeof(AcademicSettingsPage));
        Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
        Routing.RegisterRoute(nameof(StudentDetailsPage), typeof(StudentDetailsPage));
        Routing.RegisterRoute(nameof(EditStudentGradesPage), typeof(EditStudentGradesPage));
    }
}
