using System.IO;
using Microsoft.EntityFrameworkCore;
using Ostathi.Models;

namespace Ostathi;

public class AppDbContext : DbContext
{
    // Define the Students and Majors tables in the database
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Major> Majors => Set<Major>();

    private readonly string _dbPath;

    public AppDbContext()
    {
        // Determine the local database file storage path inside the app's data directory on the device
        var databasePath = FileSystem.AppDataDirectory;
        _dbPath = Path.Combine(databasePath, "Ostathi.db3");
        
        // Ensure the database and tables are created with the updated schema
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configure Entity Framework to use SQLite with the specified file path
        optionsBuilder.UseSqlite($"Filename={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed initial default majors into the Majors table upon creation
        modelBuilder.Entity<Major>().HasData(
            new Major { Id = 1, Name = "علوم الحاسب (CIS)" },
            new Major { Id = 2, Name = "تقنية المعلومات (IT)" },
            new Major { Id = 3, Name = "هندسة البرمجيات (SE)" }
        );
    }
}
