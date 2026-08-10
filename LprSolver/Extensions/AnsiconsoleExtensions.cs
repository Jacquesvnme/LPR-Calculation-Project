using Spectre.Console;
using Spectre.Console.Extensions;

namespace LprSolver.Extensions;

public static class ConsoleExtensions
{
    /// <summary>
    /// Clears the terminal and prints the application header with the name and description of the application.
    /// </summary>
    /// <param name="console"></param>
    public static Task ResetConsole()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("LPR381").Color(Color.CornflowerBlue));
        AnsiConsole.MarkupLine("[grey]Linear and integer programming solver[/]");
        AnsiConsole.WriteLine("");

        return Task.CompletedTask;
    }

    public static async Task CompletedEvent(string Text)
    {
        AnsiConsole.MarkupLine($"[blue]Info:[/] {Text}...");
        await Task.Delay(TimeSpan.FromSeconds(1));
    }

    public static Task MarkupDefault(string text)
    {
        AnsiConsole.MarkupLine($"[gray]{text}[/]");
        return Task.CompletedTask;
    }

    public static Task MarkupError(string text)
    {
        AnsiConsole.MarkupLine($"[red]> {text}[/]");
        return Task.CompletedTask;
    }

    public static async Task Sleep(int timeToSleep)
    {
        await Task.Delay(TimeSpan.FromSeconds(timeToSleep));
    }
}
