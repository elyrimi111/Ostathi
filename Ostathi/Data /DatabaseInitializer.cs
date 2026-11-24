using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public static class DatabaseInitializer
{
    // Delete the entire database, recreate it from scratch, and seed initial data
    public static async Task InitializeAsync(AppDbContext dbContext)
    {
        if (dbContext == null) return;

        await dbContext.Database.CloseConnectionAsync();

        // Clear pooled SQLite connections so file handles are released before deletion
        SqliteConnection.ClearAllPools();

        // Drop and recreate the database schema from the current EF Core model
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        // Enforce foreign key constraints per connection in SQLite
        await dbContext.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");

        // Seed default majors so the major picker is populated immediately
        await SeedMajorsAsync(dbContext);
    }

    // Populate default academic majors if the Majors table is empty
    private static async Task SeedMajorsAsync(AppDbContext dbContext)
    {
        if (await dbContext.Majors.AnyAsync()) return;

        var defaultMajors = new[]
        {
            new Major { Name = "علوم الحاسوب", DurationYears = 4 },
            new Major { Name = "تقنية المعلومات", DurationYears = 4 },
            new Major { Name = "هندسة البرمجيات", DurationYears = 5 }
        };

        await dbContext.Majors.AddRangeAsync(defaultMajors);
        await dbContext.SaveChangesAsync();
    }
}
