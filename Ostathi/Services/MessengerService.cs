using System;

namespace Ostathi.Services
{
    public static class MessengerService
    {
        // Notification events
        public static event Action? StudentAdded;
        public static event Action? GroupAdded;
        public static event Action? GroupDeleted; // New event for group deletion
        public static event Action? LectureAdded; // New event for lecture addition or update

        // Notify Student Added or Updated
        public static void NotifyStudentAdded()
        {
            StudentAdded?.Invoke();
        }

        // Notify Group Added or Updated
        public static void NotifyGroupAdded()
        {
            GroupAdded?.Invoke();
        }

        // Notify Group Deleted
        public static void NotifyGroupDeleted()
        {
            GroupDeleted?.Invoke();
        }

        // Notify Lecture Added or Updated
        public static void NotifyLectureAdded()
        {
            LectureAdded?.Invoke();
        }
    }
}
