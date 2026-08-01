using Microsoft.Extensions.DependencyInjection;
using App = LprSolver.Application.Application;

namespace LprSolver;

class Program
{
    /// <summary>
    /// This is the main entry point of the application.
    /// This method sets up the dependency injection container, resolves the main application class, and runs the application.
    /// Code related starting point sits with ./Application/Application.cs > Run() method.
    /// </summary>
    static async Task Main(string[] args)
    {
        // Setup DI container
        var serviceProvider = new ServiceCollection()
            .AddScoped<App>()
            .AddConfigureServices()
            .BuildServiceProvider();

        // Resolve and run the App
        var app = serviceProvider.GetRequiredService<App>();
        await app.Run();
    }
}
