using System;

namespace Ostathi.Services
{
    public static class MessengerService
    {
        public static event Action? StudentAdded;

        public static void NotifyStudentAdded()
        {
            StudentAdded?.Invoke();
        }
    }
}
