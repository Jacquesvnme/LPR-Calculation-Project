using LprSolver.Enums;
using LprSolver.Models;
using LprSolver.Services;
using Spectre.Console;

namespace LprSolver.Application;

/// <summary>
/// This class represents the main application for the Linear Programming Solver.
/// </summary>
public class Application
{
    private readonly IImporter _importer;
    private readonly ISolverSelection _solver;
    private readonly IExporter _exporter;
    private readonly IMenu _menu;

    /// <summary>
    /// Class constructor for the Application class.
    /// </summary>
    public Application(IImporter importer, ISolverSelection solver, IExporter exporter, IMenu menu)
    {
        _menu = menu;
        _importer = importer;
        _solver = solver;
        _exporter = exporter;
    }

    /// <summary>
    /// This method is the entry point for the application.
    /// This method is called from the Program.cs file and is responsible for running the application.
    /// </summary>
    public async Task Run()
    {
        while (true)
        {
            await ConsoleExtensions.ResetConsole();

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an action")
                    .AddChoices("Import and solve a model", "Exit")
            );

            if (action == "Exit")
            {
                return;
            }

            await ConsoleExtensions.ResetConsole();
            await StartSolverProcess();
        }
    }

    /// <summary>
    /// This method handles the process of importing a linear programming model
    /// selecting a solver algorithm, and starting the solver process.
    /// Afterwards it will handle the exporting of the results.
    /// </summary>
    /// <returns></returns>
    private async Task StartSolverProcess()
    {
        var importedFilePath = await _menu.DisplaySourceMenu();
        if (!importedFilePath.IsSuccess)
        {
            await ConsoleExtensions.MarkupError(importedFilePath.Message);
            await ConsoleExtensions.Sleep(3);
            return;
        }

        await ConsoleExtensions.CompletedEvent("File imported successfully...");

        var importedLinearProgram = await _importer.ImportDataFromTextFile(
            importedFilePath.FilePath
        );
        if (!importedLinearProgram.IsSuccess || importedLinearProgram.LinearProgram is null)
        {
            await ConsoleExtensions.MarkupError(importedLinearProgram.Message);
            await ConsoleExtensions.Sleep(3);
            return;
        }

        await ConsoleExtensions.CompletedEvent("Data imported successfully...");

        SolverAlgorithm userSelectedOption = await _menu.GetUserSelectedOption();
        if (userSelectedOption == SolverAlgorithm.INVALID_OPTION)
        {
            await ConsoleExtensions.MarkupError(importedLinearProgram.Message);
            await ConsoleExtensions.Sleep(3);
            return;
        }

        await ConsoleExtensions.CompletedEvent("Algorithm selected...");

        List<AlgorithmAnalysisOptions> algorithmOptions = await _menu.GetAlgorithmAnalysisOptions();
        if (algorithmOptions.Contains(AlgorithmAnalysisOptions.INVALID_OPTION))
        {
            await ConsoleExtensions.CompletedEvent("No options selected");
        }
        else
        {
            await ConsoleExtensions.CompletedEvent(
                $"{algorithmOptions.Count()} option(s) selected"
            );
        }

        var solverResult = await _solver.StartSolver(
            userSelectedOption,
            importedLinearProgram.LinearProgram
        );
        if (!solverResult.IsSuccess)
        {
            await ConsoleExtensions.MarkupError(solverResult.Message);
            await ConsoleExtensions.Sleep(3);
            return;
        }

        await ConsoleExtensions.CompletedEvent("Algorithm Solved...");

        var exportResult = await _exporter.ExportDataToTextFile();
        if (!exportResult.IsSuccess)
        {
            await ConsoleExtensions.MarkupError(exportResult.Message);
            await ConsoleExtensions.Sleep(3);
            return;
        }

        await ConsoleExtensions.CompletedEvent("Algorithm Exported...");

        return;
    }
}
