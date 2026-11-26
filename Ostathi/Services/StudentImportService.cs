using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public class StudentImportService
{
    private readonly AppDbContext _dbContext;

    public StudentImportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Parse raw text (from CSV/TXT file or clipboard) into structured student items
    public List<ParsedStudentDto> ParseStudentsFromText(string rawText, int groupId)
    {
        var results = new List<ParsedStudentDto>();
        if (string.IsNullOrWhiteSpace(rawText))
            return results;

        var lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        int seq = 1;
        bool isFirstLine = true;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Skip header row if present
            if (isFirstLine)
            {
                isFirstLine = false;
                if (line.Contains("الاسم", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("Name", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            var parts = line.Split(new[] { ',', ';', '\t', '|' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.Trim().Trim('"'))
                            .Where(p => !string.IsNullOrWhiteSpace(p))
                            .ToArray();

            if (parts.Length == 0)
                continue;

            string fullName;
            string academicId = string.Empty;

            if (parts.Length >= 2)
            {
                if (parts[0].All(char.IsDigit))
                {
                    academicId = parts[0];
                    fullName = parts[1];
                }
                else if (parts[^1].All(char.IsDigit))
                {
                    fullName = parts[0];
                    academicId = parts[^1];
                }
                else
                {
                    fullName = parts[0];
                    academicId = parts[1];
                }
            }
            else
            {
                fullName = parts[0];
            }

            // Clean leading row numbering such as "1. " or "1- " or "1) "
            fullName = Regex.Replace(fullName, @"^\d+[\-\.\)]\s*", "").Trim();
            if (string.IsNullOrWhiteSpace(fullName))
                continue;

            var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string firstName = Truncate(nameParts.ElementAtOrDefault(0), 100, fullName);
            string secondName = Truncate(nameParts.ElementAtOrDefault(1), 100, "-");
            string thirdName = Truncate(nameParts.ElementAtOrDefault(2), 100, "-");
            string lastName = nameParts.Length > 3
                ? Truncate(string.Join(" ", nameParts.Skip(3)), 100, "-")
                : "-";

            string resolvedAcademicId = !string.IsNullOrWhiteSpace(academicId)
                ? Truncate(academicId, 50, string.Empty)
                : $"G{groupId}-{DateTime.Now:yyMMdd}-{seq++:000}";

            results.Add(new ParsedStudentDto
            {
                FullName = $"{firstName} {secondName} {thirdName} {lastName}".Replace(" -", "").Trim(),
                FirstName = firstName,
                SecondName = secondName,
                ThirdName = thirdName,
                LastName = lastName,
                AcademicId = resolvedAcademicId
            });
        }

        return results;
    }

    // Read text safely from a file stream using UTF-8 with BOM detection
    public async Task<string> ReadStreamTextAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync();
    }

    // Save imported students into SQLite using Group, Major, and StudentConfiguration rules
    public async Task<ImportResultDto> SaveImportedStudentsAsync(int groupId, IEnumerable<ParsedStudentDto> students)
    {
        var targetGroup = await _dbContext.Groups
            .Include(g => g.Major)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (targetGroup == null)
            throw new InvalidOperationException("المجموعة المستهدفة غير موجودة في قاعدة البيانات.");

        int majorId = targetGroup.Major?.Id ?? targetGroup.MajorId;

        string academicYear = Truncate(
            string.IsNullOrWhiteSpace(targetGroup.AcademicYear) ? "السنة الأولى" : targetGroup.AcademicYear,
            100,
            "السنة الأولى");

        // AcademicId has a unique index (IX_Students_AcademicId) across all students
        var existingAcademicIds = await _dbContext.Students
            .Select(s => s.AcademicId)
            .ToListAsync();

        var usedIds = new HashSet<string>(existingAcademicIds, StringComparer.OrdinalIgnoreCase);

        int addedCount = 0;
        int skippedCount = 0;
        int autoCounter = 1;

        foreach (var item in students)
        {
            string firstName = Truncate(item.FirstName, 100, string.Empty);
            if (string.IsNullOrWhiteSpace(firstName))
            {
                skippedCount++;
                continue;
            }

            string secondName = Truncate(item.SecondName, 100, "-");
            string thirdName = Truncate(item.ThirdName, 100, "-");
            string lastName = Truncate(item.LastName, 100, "-");

            string candidateId = Truncate(item.AcademicId, 50, string.Empty);

            if (!string.IsNullOrWhiteSpace(candidateId) && usedIds.Contains(candidateId))
            {
                skippedCount++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(candidateId))
            {
                do
                {
                    candidateId = Truncate($"G{groupId}-{DateTime.Now:yyMMdd}-{autoCounter++:0000}", 50, string.Empty);
                }
                while (usedIds.Contains(candidateId));
            }

            var student = new Student
            {
                FirstName = firstName,
                SecondName = secondName,
                ThirdName = thirdName,
                LastName = lastName,
                AcademicId = candidateId,
                MajorId = majorId,
                GroupId = groupId,
                AcademicYear = academicYear
            };

            await _dbContext.Students.AddAsync(student);
            usedIds.Add(candidateId);
            addedCount++;
        }

        if (addedCount > 0)
        {
            await _dbContext.SaveChangesAsync();
        }

        return new ImportResultDto
        {
            AddedCount = addedCount,
            SkippedCount = skippedCount
        };
    }

    private static string Truncate(string? value, int maxLength, string fallback)
    {
        var clean = value?.Trim();
        if (string.IsNullOrWhiteSpace(clean))
            return fallback;

        return clean.Length <= maxLength ? clean : clean[..maxLength];
    }
}
