using System.Linq.Expressions;
using LprSolver.Enums;
using LprSolver.Models;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace LprSolver.Services;

public interface IMenu
{
    Task<ImporterFilePathResponse> DisplaySourceMenu(MenuType menuType);
    Task<List<AlgorithmAnalysisOptions>> GetAlgorithmAnalysisOptions();
    Task<SolverAlgorithm> GetUserSelectedOption();
}

/// <summary>
/// Displays a menu and processes mininal amount of information related to the selection
/// </summary>
public class Menu : IMenu
{
    private readonly IConfiguration _configuration;

    public Menu(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// This method displays a menu to the user for selecting an input path.
    /// This method also does some basic validation.
    /// </summary>
    /// <returns></returns>
    public async Task<ImporterFilePathResponse> DisplaySourceMenu(MenuType menuType)
    {
        var MenuText = string.Empty;
        switch (menuType)
        {
            case MenuType.Exporter:
                MenuText = "Export";
                break;
            case MenuType.Importer:
                MenuText = "Import";
                break;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Select {MenuText} location")
                .AddChoices("Input path", "Default Source")
        );

        string inputPath = string.Empty;
        switch (selected)
        {
            case "Input path":
                var path = AnsiConsole.Prompt(
                    new TextPrompt<string>($"[green]Input {MenuText} absolute path:[/] ").Validate(
                        path =>
                            string.IsNullOrWhiteSpace(path)
                                ? ValidationResult.Error("A path is required.")
                                : ValidationResult.Success()
                    )
                );

                inputPath = ResolveExistingFilePath(path);
                if (inputPath == string.Empty)
                {
                    return new("No source loaded", false, string.Empty);
                }

                return new(string.Empty, true, inputPath);
            case "Default Source":
                var configurationPath = _configuration.GetSection($"{MenuText}Location");

                inputPath = ResolveExistingFilePath(configurationPath.Value);
                if (inputPath == string.Empty)
                {
                    return new("No source loaded", false, string.Empty);
                }

                return new(string.Empty, true, inputPath);
            default:
                return new("No source loaded", false, string.Empty);
        }
    }

    /// <summary>
    /// Gets the user-selected option
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task<SolverAlgorithm> GetUserSelectedOption()
    {
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select algorithm")
                .AddChoices(
                    "Primal Simplex",
                    "Revised Primal Simplex",
                    "Branch and Bound",
                    "Revised Branch and Bound",
                    "Cutting Plane",
                    "Revised Cutting Plane",
                    "Branch and Bound Knapsack",
                    "Non Linear"
                )
        );

        return selected switch
        {
            "Primal Simplex" => SolverAlgorithm.PrimalSimplex,
            "Revised Primal Simplex" => SolverAlgorithm.Revised_PrimalSimplex,
            "Branch and Bound" => SolverAlgorithm.BranchAndBound,
            "Revised Branch and Bound" => SolverAlgorithm.Revised_BranchAndBound,
            "Cutting Plane" => SolverAlgorithm.CuttingPlane,
            "Revised Cutting Plane" => SolverAlgorithm.Revised_CuttingPlane,
            "Branch and Bound Knapsack" => SolverAlgorithm.BranchAndBoundKnapsack,
            "Non Linear" => SolverAlgorithm.NonLinearProblem,
            _ => SolverAlgorithm.INVALID_OPTION,
        };
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public async Task<List<AlgorithmAnalysisOptions>> GetAlgorithmAnalysisOptions()
    {
        var selectedOptions = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select sensitivity analysis operations")
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to select, [green]<enter>[/] to confirm)[/]"
                )
                .NotRequired()
                .AddChoices(
                    "Display Non-Basic Variable Range",
                    "Apply Change to Non-Basic Variable",
                    "Display Basic Variable Range",
                    "Apply Change to Basic Variable",
                    "Display Constraint Right-Hand-Side Range",
                    "Apply Change to Constraint Right-Hand-Side",
                    "Display Variable Range in Non-Basic Variable Column",
                    "Apply Change to Variable in Non-Basic Variable Column",
                    "Add New Activity to Optimal Solution",
                    "Add New Constraint to Optimal Solution",
                    "Display Shadow Prices",
                    "Apply Duality to Programming Model",
                    "Solve Dual Programming Model",
                    "Verify Strong or Weak Duality"
                )
        );

        List<AlgorithmAnalysisOptions> SelectedAnalysisOptions = new();
        if (selectedOptions.Count <= 0)
        {
            SelectedAnalysisOptions.Add(AlgorithmAnalysisOptions.INVALID_OPTION);
        }

        foreach (var option in selectedOptions)
        {
            switch (option)
            {
                case "Display Non-Basic Variable Range":
                    SelectedAnalysisOptions.Add(
                        AlgorithmAnalysisOptions.DisplayNonBasicVariableRange
                    );
                    break;
                case "Apply Change to Non-Basic Variable":
                    SelectedAnalysisOptions.Add(
                        AlgorithmAnalysisOptions.ApplyNonBasicVariableChange
                    );
                    break;
                case "Display Basic Variable Range":
                    SelectedAnalysisOptions.Add(AlgorithmAnalysisOptions.DisplayBasicVariableRange);
                    break;
                case "Apply Change to Basic Variable":
                    SelectedAnalysisOptions.Add(AlgorithmAnalysisOptions.ApplyBasicVariableChange);
                    break;
                case "Display Constraint Right-Hand-Side Range":
                    SelectedAnalysisOptions.Add(
                        AlgorithmAnalysisOptions.DisplayConstraintRightHandSideRange
                    );
                    break;
                case "Apply Change to Constraint Right-Hand-Side":
                    SelectedAnalysisOptions.Add(
                        AlgorithmAnalysisOptions.ApplyConstraintRightHandSideChange
                    );
                    break;
                case "Display Variable Range in Non-Basic Variable Column":
                    SelectedAnalysisOptions.Add(
                        AlgorithmAnalysisOptions.DisplayNonBasicColumnVariableRange
                    );
                    break;
                case "Apply Change to Variable in Non-Basic Variable Column":
                    SelectedAnalysisOptions.Add(
                        AlgorithmAnalysisOptions.ApplyNonBasicColumnVariableChange
                    );
                    break;
                case "Add New Activity to Optimal Solution":
                    SelectedAnalysisOptions.Add(
                        AlgorithmAnalysisOptions.AddActivityToOptimalSolution
                    );
                    break;
                case "Add New Constraint to Optimal Solution":
                    SelectedAnalysisOptions.Add(
                        AlgorithmAnalysisOptions.AddConstraintToOptimalSolution
                    );
                    break;
                case "Display Shadow Prices":
                    SelectedAnalysisOptions.Add(AlgorithmAnalysisOptions.DisplayShadowPrices);
                    break;
                case "Apply Duality to Programming Model":
                    SelectedAnalysisOptions.Add(AlgorithmAnalysisOptions.ApplyDuality);
                    break;
                case "Solve Dual Programming Model":
                    SelectedAnalysisOptions.Add(AlgorithmAnalysisOptions.SolveDualProgrammingModel);
                    break;
                case "Verify Strong or Weak Duality":
                    SelectedAnalysisOptions.Add(AlgorithmAnalysisOptions.VerifyStrongOrWeakDuality);
                    break;
            }
        }

        return SelectedAnalysisOptions;
    }

    /// <summary>
    /// Checks if the provided input path is valid.
    /// If the path is valid and the file exists, it returns the absolute path; otherwise, it returns an empty string.
    /// </summary>
    /// <param name="inputPath"></param>
    /// <returns></returns>
    private static string ResolveExistingFilePath(string? inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return string.Empty;
        }

        try
        {
            var absolutePath = Path.GetFullPath(inputPath.Trim(), AppContext.BaseDirectory);

            return File.Exists(absolutePath) ? absolutePath : string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
