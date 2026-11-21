using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ostathi.Services;

namespace Ostathi;

public class GroupSubjectLuctersService
{
    private readonly AppDbContext _context;

    public GroupSubjectLuctersService()
    {
        _context = new AppDbContext();
    }

    public GroupSubjectLuctersService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<GroupSubjectLucters>> GetAllAsync()
    {
        return await _context.GroupSubjectLucters
            .OrderBy(x => x.GroupSubjectId)
            .ThenBy(x => x.LectureNumber)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<GroupSubjectLucters?> GetByIdAsync(int id)
    {
        return await _context.GroupSubjectLucters.FindAsync(id);
    }

    public async Task<GroupSubjectLucters?> GetByLectureIdAsync(int lectureId)
    {
        return await _context.GroupSubjectLucters
            .FirstOrDefaultAsync(x => x.LectureId == lectureId);
    }

    public async Task<List<GroupSubjectLucters>> GetByGroupSubjectIdAsync(int groupSubjectId)
    {
        return await _context.GroupSubjectLucters
            .Where(x => x.GroupSubjectId == groupSubjectId)
            .OrderBy(x => x.LectureNumber)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<List<GroupSubjectLucters>> GetUntakenByGroupSubjectIdAsync(int groupSubjectId)
    {
        return await _context.GroupSubjectLucters
            .Where(x => x.GroupSubjectId == groupSubjectId && !x.IsTaken)
            .OrderBy(x => x.LectureNumber)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<GroupSubjectLucters?> GetNextUntakenAsync(int groupSubjectId, int? excludeId = null)
    {
        var query = _context.GroupSubjectLucters
            .Where(x => x.GroupSubjectId == groupSubjectId && !x.IsTaken);

        if (excludeId.HasValue && excludeId.Value > 0)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query
            .OrderBy(x => x.LectureNumber)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetNextLectureNumberAsync(int groupSubjectId)
    {
        int maxNumber = await _context.GroupSubjectLucters
            .Where(x => x.GroupSubjectId == groupSubjectId)
            .Select(x => (int?)x.LectureNumber)
            .MaxAsync() ?? 0;

        return maxNumber + 1;
    }

    public async Task<GroupSubjectLucters> AddAsync(GroupSubjectLucters item)
    {
        if (item.LectureNumber <= 0)
        {
            item.LectureNumber = await GetNextLectureNumberAsync(item.GroupSubjectId);
        }

        _context.GroupSubjectLucters.Add(item);
        await _context.SaveChangesAsync();
        MessengerService.NotifyLectureAdded();

        return item;
    }

    public async Task<GroupSubjectLucters> CreateLinkAsync(int groupSubjectId, int lectureId, bool isTaken = false, int? lectureNumber = null)
    {
        var item = new GroupSubjectLucters
        {
            GroupSubjectId = groupSubjectId,
            LectureId = lectureId,
            LectureNumber = lectureNumber ?? await GetNextLectureNumberAsync(groupSubjectId),
            IsTaken = isTaken
        };

        return await AddAsync(item);
    }

    public async Task<int> UpdateAsync(GroupSubjectLucters item)
    {
        var existing = await GetByIdAsync(item.Id);
        if (existing != null)
        {
            existing.GroupSubjectId = item.GroupSubjectId;
            existing.LectureId = item.LectureId;
            existing.LectureNumber = item.LectureNumber;
            existing.IsTaken = item.IsTaken;
        }
        else
        {
            _context.GroupSubjectLucters.Add(item);
        }

        int result = await _context.SaveChangesAsync();
        MessengerService.NotifyLectureAdded();
        return result;
    }

    public async Task<bool> MarkAsTakenAsync(int id, bool isTaken = true)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return false;

        existing.IsTaken = isTaken;
        await _context.SaveChangesAsync();
        MessengerService.NotifyLectureAdded();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return false;

        _context.GroupSubjectLucters.Remove(existing);
        await _context.SaveChangesAsync();
        MessengerService.NotifyLectureAdded();
        return true;
    }

    public async Task<bool> DeleteByLectureIdAsync(int lectureId)
    {
        var existing = await GetByLectureIdAsync(lectureId);
        if (existing == null) return false;

        _context.GroupSubjectLucters.Remove(existing);
        await _context.SaveChangesAsync();
        MessengerService.NotifyLectureAdded();
        return true;
    }
}
