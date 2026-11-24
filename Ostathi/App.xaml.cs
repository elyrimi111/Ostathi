using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class App : Application
{
    private bool _isDatabaseInitialized;

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        // Initialize the SQLite database once when the window is created
        _ = EnsureDatabaseInitializedAsync(window);

        return window;
    }

    // Resolve AppDbContext from DI and run DatabaseInitializer once per app session
    private async Task EnsureDatabaseInitializedAsync(Window window)
    {
        if (_isDatabaseInitialized) return;
        _isDatabaseInitialized = true;

        try
        {
            var services = window.Handler?.MauiContext?.Services
                           ?? Handler?.MauiContext?.Services
                           ?? Application.Current?.Handler?.MauiContext?.Services;

            // Wait briefly if MauiContext services are not attached yet
            int retries = 0;
            while (services == null && retries < 10)
            {
                await Task.Delay(50);
                services = window.Handler?.MauiContext?.Services
                           ?? Handler?.MauiContext?.Services
                           ?? Application.Current?.Handler?.MauiContext?.Services;
                retries++;
            }

            if (services == null) return;

            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<AppDbContext>();

            if (dbContext != null)
            {
                await DatabaseInitializer.InitializeAsync(dbContext);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
        }
    }
}
