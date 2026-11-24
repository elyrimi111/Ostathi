using System.IO;
using Microsoft.Maui.Storage;

namespace Ostathi;

public static class DatabaseConfig
{
    private const string DatabaseName = "Ostathi.db3";

    // Resolve the local SQLite file path inside the app sandbox directory
    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseName);

    // Build SQLite connection string with shared cache and timeout for mobile stability
    public static string ConnectionString =>
        $"Data Source={DatabasePath};Cache=Shared;Default Timeout=5;";
}
