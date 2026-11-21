using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ostathi.Services;

namespace Ostathi;

public class LectureSaveResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class LucterService
{
    private readonly AppDbContext _context;

    public LucterService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Lecture>> GetAllLecturesAsync()
    {
        return await _context.Lectures
            .OrderBy(l => l.LectureNumber)
            .ThenBy(l => l.StartTime)
            .ToListAsync();
    }

    public async Task<Lecture?> GetLectureByIdAsync(int id)
    {
        return await _context.Lectures.FindAsync(id);
    }

    public async Task<List<Lecture>> GetLecturesByGroupSubjectIdAsync(int groupSubjectId)
    {
        return await _context.Lectures
            .Where(l => l.GroupSubjectId.HasValue && l.GroupSubjectId.Value == groupSubjectId)
            .OrderBy(l => l.LectureNumber)
            .ThenBy(l => l.LectureDate)
            .ToListAsync();
    }

    public async Task GenerateDefaultLecturesAsync(int groupSubjectId, int count = 12)
    {
        if (groupSubjectId <= 0 || count <= 0)
            return;

        var existingCount = await _context.Lectures
            .CountAsync(l => l.GroupSubjectId.HasValue && l.GroupSubjectId.Value == groupSubjectId);

        if (existingCount >= count)
            return;

        int startIndex = existingCount + 1;
        var createdLectures = new List<Lecture>();

        for (int i = startIndex; i <= count; i++)
        {
            var date = DateTime.Today.AddDays((i - 1) * 7);
            var lecture = new Lecture
            {
                LectureNumber = i,
                GroupSubjectId = groupSubjectId,
                HallName = "قاعة 1",
                DayOfWeek = ScheduleService.ToOstathiWeekDay(date.DayOfWeek),
                LectureDate = date,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(9, 30),
                IsCancelled = false
            };

            createdLectures.Add(lecture);
            _context.Lectures.Add(lecture);
        }

        await _context.SaveChangesAsync();

        foreach (var lecture in createdLectures)
        {
            bool linkExists = await _context.GroupSubjectLucters
                .AnyAsync(x => x.GroupSubjectId == groupSubjectId && x.LectureId == lecture.Id);

            if (!linkExists)
            {
                _context.GroupSubjectLucters.Add(new GroupSubjectLucters
                {
                    GroupSubjectId = groupSubjectId,
                    LectureId = lecture.Id,
                    LectureNumber = lecture.LectureNumber,
                    IsTaken = false
                });
            }
        }

        await _context.SaveChangesAsync();
        MessengerService.NotifyLectureAdded();
    }

    public async Task<int> AddLectureAsync(Lecture lecture)
    {
        if (lecture.LectureNumber <= 0)
        {
            int maxNumber = await _context.Lectures
                .Select(l => (int?)l.LectureNumber)
                .MaxAsync() ?? 0;

            lecture.LectureNumber = maxNumber + 1;
        }

        if (lecture.DayOfWeek == default && lecture.LectureDate != default)
        {
            lecture.DayOfWeek = ScheduleService.ToOstathiWeekDay(lecture.LectureDate.DayOfWeek);
        }

        _context.Lectures.Add(lecture);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> UpdateLectureAsync(Lecture lecture)
    {
        var existingLecture = await GetLectureByIdAsync(lecture.Id);

        if (existingLecture != null)
        {
            existingLecture.LectureNumber = lecture.LectureNumber > 0
                ? lecture.LectureNumber
                : existingLecture.LectureNumber;
            existingLecture.GroupSubjectId = lecture.GroupSubjectId;
            existingLecture.DayOfWeek = lecture.DayOfWeek != default
                ? lecture.DayOfWeek
                : ScheduleService.ToOstathiWeekDay(lecture.LectureDate.DayOfWeek);
            existingLecture.HallName = lecture.HallName;
            existingLecture.LectureDate = lecture.LectureDate;
            existingLecture.StartTime = lecture.StartTime;
            existingLecture.EndTime = lecture.EndTime;
            existingLecture.IsCancelled = lecture.IsCancelled;
            existingLecture.Notes = lecture.Notes;
        }
        else
        {
            _context.Lectures.Update(lecture);
        }

        return await _context.SaveChangesAsync();
    }

    public async Task<int> SaveLectureAsync(Lecture lecture)
    {
        int result = lecture.Id == 0
            ? await AddLectureAsync(lecture)
            : await UpdateLectureAsync(lecture);

        MessengerService.NotifyLectureAdded();
        return result;
    }

    public async Task<int> DeleteLectureAsync(Lecture lecture)
    {
        var tracked = await GetLectureByIdAsync(lecture.Id);
        _context.Lectures.Remove(tracked ?? lecture);
        int result = await _context.SaveChangesAsync();
        MessengerService.NotifyLectureAdded();
        return result;
    }

    public async Task<LectureSaveResult> SaveLectureAttendanceAndStatusAsync(
        Lecture lecture,
        int groupSubjectId,
        string hallName,
        WeekDay dayOfWeek,
        DateTime lectureDate,
        bool isTaken,
        IEnumerable<StudentAttendanceItem> studentsAttendance)
    {
        try
        {
            lecture.GroupSubjectId = groupSubjectId > 0 ? groupSubjectId : null;
            lecture.HallName = hallName;
            lecture.DayOfWeek = dayOfWeek;
            lecture.LectureDate = lectureDate;

            await UpdateLectureAsync(lecture);

            int lectureNum = lecture.LectureNumber > 0 ? lecture.LectureNumber : 1;

            if (lecture.GroupSubjectId.HasValue)
            {
                var link = await _context.GroupSubjectLucters
                    .FirstOrDefaultAsync(x => x.LectureId == lecture.Id);

                if (link != null)
                {
                    link.IsTaken = isTaken;
                    link.LectureNumber = lectureNum;
                }
                else
                {
                    await _context.GroupSubjectLucters.AddAsync(new GroupSubjectLucters
                    {
                        GroupSubjectId = lecture.GroupSubjectId.Value,
                        LectureId = lecture.Id,
                        LectureNumber = lectureNum,
                        IsTaken = isTaken
                    });
                }
            }

            var existingRecords = await _context.Attendances
                .Where(a => a.LectureId == lecture.Id)
                .ToListAsync();

            var existingByStudentId = existingRecords.ToDictionary(a => a.StudentId, a => a);

            foreach (var item in studentsAttendance)
            {
                if (existingByStudentId.TryGetValue(item.StudentId, out var existingRecord))
                {
                    existingRecord.Status = item.Status;
                    existingRecord.Date = lectureDate;
                }
                else
                {
                    await _context.Attendances.AddAsync(new Attendance
                    {
                        StudentId = item.StudentId,
                        LectureId = lecture.Id,
                        Status = item.Status,
                        Date = lectureDate
                    });
                }
            }

            await _context.SaveChangesAsync();
            MessengerService.NotifyLectureAdded();

            return new LectureSaveResult
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new LectureSaveResult
            {
                Success = false,
                ErrorMessage = ex.InnerException?.Message ?? ex.Message
            };
        }
    }
}
