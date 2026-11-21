using System;
using System.Collections.Generic;
using System.Linq;

namespace Ostathi;

public class ScheduleService
{
    public static WeekDay ToOstathiWeekDay(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Saturday => WeekDay.Saturday,
        DayOfWeek.Sunday => WeekDay.Sunday,
        DayOfWeek.Monday => WeekDay.Monday,
        DayOfWeek.Tuesday => WeekDay.Tuesday,
        DayOfWeek.Wednesday => WeekDay.Wednesday,
        DayOfWeek.Thursday => WeekDay.Thursday,
        DayOfWeek.Friday => WeekDay.Friday,
        _ => WeekDay.Saturday
    };

    public List<Lecture> GetTodayLectures(IEnumerable<Lecture> lectures, DateTime? referenceTime = null)
    {
        var now = referenceTime ?? DateTime.Now;
        var today = now.Date;
        var currentWeekDay = ToOstathiWeekDay(now.DayOfWeek);

        return lectures
            .Where(l => !l.IsCancelled &&
                        (l.LectureDate.Date == today || l.DayOfWeek == currentWeekDay))
            .OrderBy(l => l.StartTime)
            .ToList();
    }

    public Lecture? GetCurrentLecture(IEnumerable<Lecture> lectures, DateTime? referenceTime = null)
    {
        var now = referenceTime ?? DateTime.Now;
        var currentTime = TimeOnly.FromDateTime(now);

        return GetTodayLectures(lectures, now)
            .FirstOrDefault(l => currentTime >= l.StartTime && currentTime <= l.EndTime);
    }

    public Lecture? GetNextLecture(IEnumerable<Lecture> lectures, DateTime? referenceTime = null)
    {
        var now = referenceTime ?? DateTime.Now;
        var currentTime = TimeOnly.FromDateTime(now);

        var nextToday = GetTodayLectures(lectures, now)
            .FirstOrDefault(l => l.StartTime > currentTime);

        if (nextToday is not null)
            return nextToday;

        for (int dayOffset = 1; dayOffset <= 7; dayOffset++)
        {
            var targetDate = now.Date.AddDays(dayOffset);
            var targetWeekDay = ToOstathiWeekDay(targetDate.DayOfWeek);

            var nextInFutureDays = lectures
                .Where(l => !l.IsCancelled &&
                            (l.LectureDate.Date == targetDate || l.DayOfWeek == targetWeekDay))
                .OrderBy(l => l.StartTime)
                .FirstOrDefault();

            if (nextInFutureDays is not null)
                return nextInFutureDays;
        }

        return null;
    }

    public List<Lecture> GetLecturesByDay(IEnumerable<Lecture> lectures, WeekDay day)
    {
        return lectures
            .Where(l => !l.IsCancelled && l.DayOfWeek == day)
            .OrderBy(l => l.StartTime)
            .ToList();
    }

    public TimeSpan? GetTimeUntilNextLecture(IEnumerable<Lecture> lectures, DateTime? referenceTime = null)
    {
        var now = referenceTime ?? DateTime.Now;
        var next = GetNextLecture(lectures, now);

        if (next is null)
            return null;

        var nextStart = GetNextOccurrenceDateTime(next, now);
        var diff = nextStart - now;

        return diff > TimeSpan.Zero ? diff : TimeSpan.Zero;
    }

    private DateTime GetNextOccurrenceDateTime(Lecture lecture, DateTime now)
    {
        if (lecture.LectureDate.Date == now.Date)
            return now.Date.Add(lecture.StartTime.ToTimeSpan());

        if (lecture.LectureDate.Date > now.Date)
            return lecture.LectureDate.Date.Add(lecture.StartTime.ToTimeSpan());

        var currentWeekDayIndex = (int)ToOstathiWeekDay(now.DayOfWeek);
        var targetWeekDayIndex = (int)lecture.DayOfWeek;

        int daysToAdd = targetWeekDayIndex - currentWeekDayIndex;
        if (daysToAdd <= 0)
            daysToAdd += 7;

        return now.Date.AddDays(daysToAdd).Add(lecture.StartTime.ToTimeSpan());
    }
}
