using System.Globalization;
using System.Text.RegularExpressions;
using LprSolver.Enums;
using LprSolver.Models;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace LprSolver.Services;

public interface IImporter
{
    Task<(bool IsSuccess, string Message, LinearProgram? LinearProgram)> ImportDataFromTextFile(
        string absoluteFilePath
    );
    Task<ImporterFilePathResponse> DisplayImporterMenu();
}

public class Importer : IImporter
{
    private readonly IConfiguration _configuration;

    public Importer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// This method displays a menu to the user for selecting an input path.
    /// This method also does some basic validation.
    /// </summary>
    /// <returns></returns>
    public async Task<ImporterFilePathResponse> DisplayImporterMenu()
    {
        await ConsoleExtensions.MarkupDefault("Importer Menu");
        AnsiConsole.WriteLine("");

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select algorithm")
                .AddChoices("Input path", "Default Source")
        );

        string inputPath = string.Empty;
        switch (selected)
        {
            case "Input path":
                var path = AnsiConsole.Prompt(
                    new TextPrompt<string>("[green]Input model path:[/] ").Validate(path =>
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
                var configurationPath = _configuration.GetSection("DataLocation");

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
    /// This method reads the contents of a text file and attempts to parse it into a LinearProgram object.
    /// </summary>
    /// <param name="absoluteFilePath"></param>
    /// <returns></returns>
    public async Task<(
        bool IsSuccess,
        string Message,
        LinearProgram? LinearProgram
    )> ImportDataFromTextFile(string absoluteFilePath)
    {
        if (string.IsNullOrWhiteSpace(absoluteFilePath))
        {
            return (false, "A file path is required.", null);
        }

        try
        {
            var lines = (await File.ReadAllLinesAsync(absoluteFilePath)).ToList();
            return ParseLinearProgram(lines);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    /// <summary>
    /// This methods start the process of main data needed for the application
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    private static (
        bool IsSuccess,
        string Message,
        LinearProgram? LinearProgram
    ) ParseLinearProgram(List<string> lines)
    {
        // removes whitespaces inside the text
        var modelLines = lines
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

        // if less then 3 then we do not have the required data
        if (modelLines.Length < 3)
        {
            return (
                false,
                "The model must contain an objective line, at least one constraint line, and a restriction line.",
                null
            );
        }

        var objectiveResult = ParseObjective(modelLines[0]);
        if (!objectiveResult.IsSuccess || objectiveResult.Objective is null)
        {
            return (false, objectiveResult.Message, null);
        }

        var constraintsResult = ParseConstraints(modelLines[1..^1]);
        if (!constraintsResult.IsSuccess || constraintsResult.Constraints is null)
        {
            return (false, constraintsResult.Message, null);
        }

        var restrictionResult = ParseRestriction(modelLines[^1]);
        if (!restrictionResult.IsSuccess || restrictionResult.Restriction is null)
        {
            return (false, restrictionResult.Message, null);
        }

        var validationResult = ValidateVariableCounts(
            objectiveResult.Objective,
            constraintsResult.Constraints,
            restrictionResult.Restriction
        );
        if (!validationResult.IsSuccess)
        {
            return (false, validationResult.Message, null);
        }

        var linearProgram = new LinearProgram(
            string.Empty,
            true,
            objectiveResult.Objective,
            constraintsResult.Constraints,
            restrictionResult.Restriction
        );

        return (true, string.Empty, linearProgram);
    }

    /// <summary>
    /// Retrieves and formats the required data for the Objectives
    /// </summary>
    private static (bool IsSuccess, string Message, Objective? Objective) ParseObjective(
        string objectiveLine
    )
    {
        OptimizationDirection direction;
        switch (objectiveLine)
        {
            case string line when line.Contains("max", StringComparison.OrdinalIgnoreCase):
                direction = OptimizationDirection.Maximize;
                break;
            case string line when line.Contains("min", StringComparison.OrdinalIgnoreCase):
                direction = OptimizationDirection.Minimize;
                break;
            default:
                return (false, "The objective line must contain 'max' or 'min'.", null);
        }

        // Removes the direction word so that only the signed coefficients remain.
        var coefficientText = Regex.Replace(
            objectiveLine,
            @"\b(max|min)\b",
            string.Empty,
            RegexOptions.IgnoreCase
        );

        var coefficientResult = ParseCoefficients(coefficientText);
        if (!coefficientResult.IsSuccess || coefficientResult.Coefficients is null)
        {
            return (false, coefficientResult.Message, null);
        }

        var objective = new Objective
        {
            Direction = direction,
            Objectives = coefficientResult.Coefficients,
        };

        return (true, string.Empty, objective);
    }

    /// <summary>
    /// Retrieves and formats the required data for the Constraints
    /// </summary>
    private static (bool IsSuccess, string Message, List<Constraint>? Constraints) ParseConstraints(
        IEnumerable<string> constraintLines
    )
    {
        var constraints = new List<Constraint>();

        foreach (var constraintLine in constraintLines)
        {
            var constraintResult = ParseConstraint(constraintLine);
            if (!constraintResult.IsSuccess || constraintResult.Constraint is null)
            {
                return (false, constraintResult.Message, null);
            }

            constraints.Add(constraintResult.Constraint);
        }

        return (true, string.Empty, constraints);
    }

    /// <summary>
    /// This method parses a single constraint line
    /// Gets and sets the RHS and relation.
    /// </summary>
    /// <param name="constraintLine"></param>
    /// <returns></returns>
    private static (bool IsSuccess, string Message, Constraint? Constraint) ParseConstraint(
        string constraintLine
    )
    {
        string relationText;
        ConstraintRelation relation;

        switch (constraintLine)
        {
            case string line when line.Contains("<=", StringComparison.Ordinal):
                relationText = "<=";
                relation = ConstraintRelation.LessOrEqual;
                break;
            case string line when line.Contains(">=", StringComparison.Ordinal):
                relationText = ">=";
                relation = ConstraintRelation.GreaterOrEqual;
                break;
            case string line when line.Contains("=", StringComparison.Ordinal):
                relationText = "=";
                relation = ConstraintRelation.Equal;
                break;
            default:
                return (false, "Each constraint must contain <=, >=, or =.", null);
        }

        var parts = constraintLine.Split(relationText, StringSplitOptions.TrimEntries);
        if (
            parts.Length != 2
            || !double.TryParse(
                parts[1],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var rightHandSide
            )
        )
        {
            return (
                false,
                "Each constraint must have a numeric value on the right-hand side.",
                null
            );
        }

        var coefficientResult = ParseCoefficients(parts[0]);
        if (!coefficientResult.IsSuccess || coefficientResult.Coefficients is null)
        {
            return (false, coefficientResult.Message, null);
        }

        var constraint = new Constraint
        {
            Coefficients = coefficientResult.Coefficients,
            Relation = relation,
            RightHandSide = rightHandSide,
        };

        return (true, string.Empty, constraint);
    }

    /// <summary>
    /// Retrieves and formats the required data for the Restrictions
    /// </summary>
    /// <param name="restrictionLine"></param>
    /// <returns></returns>
    private static (bool IsSuccess, string Message, Restriction? Restriction) ParseRestriction(
        string restrictionLine
    )
    {
        var restrictionTokens = restrictionLine.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        var restrictions = new List<VariableRestriction>();

        foreach (var token in restrictionTokens)
        {
            var normalizedToken = token.ToLowerInvariant();
            VariableRestriction variableRestriction;

            switch (normalizedToken)
            {
                case "+":
                    variableRestriction = VariableRestriction.NonNegative;
                    break;
                case "-":
                    variableRestriction = VariableRestriction.NonPositive;
                    break;
                case "urs":
                    variableRestriction = VariableRestriction.Unrestricted;
                    break;
                case "int":
                    variableRestriction = VariableRestriction.Integer;
                    break;
                case "bin":
                    variableRestriction = VariableRestriction.Binary;
                    break;
                default:
                    return (false, $"'{token}' is not a valid variable restriction.", null);
            }

            restrictions.Add(variableRestriction);
        }

        var restriction = new Restriction { Restrictions = restrictions };
        return (true, string.Empty, restriction);
    }

    /// <summary>
    /// This method parses the coefficients from a given text, ensuring they have a sign and are numeric.
    /// </summary>
    /// <param name="coefficientText"></param>
    /// <returns></returns>
    private static (bool IsSuccess, string Message, List<double>? Coefficients) ParseCoefficients(
        string coefficientText
    )
    {
        var matches = Regex.Matches(coefficientText, @"(?<sign>[+-])\s*(?<value>\d+(?:\.\d+)?)");

        if (matches.Count == 0)
        {
            return (false, "At least one signed coefficient is required.", null);
        }

        var coefficients = new List<double>();

        foreach (Match match in matches)
        {
            var wasParsed = double.TryParse(
                match.Groups["value"].Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var value
            );

            if (!wasParsed)
            {
                return (false, "A coefficient could not be read as a number.", null);
            }

            var sign = match.Groups["sign"].Value;
            if (sign == "-")
            {
                value = -value;
            }

            coefficients.Add(value);
        }

        return (true, string.Empty, coefficients);
    }

    /// <summary>
    /// Final count validation to see that the rows and objectives counts match.
    /// If not then the data was not parsed correctly or incorrect data was given to begin with.
    /// </summary>
    /// <param name="objective"></param>
    /// <param name="constraints"></param>
    /// <param name="restriction"></param>
    /// <returns></returns>
    private static (bool IsSuccess, string Message) ValidateVariableCounts(
        Objective objective,
        List<Constraint> constraints,
        Restriction restriction
    )
    {
        var variableCount = objective.Objectives.Count;

        foreach (var constraint in constraints)
        {
            if (constraint.Coefficients.Count != variableCount)
            {
                return (
                    false,
                    "Every constraint must have the same number of coefficients as the objective."
                );
            }
        }

        if (restriction.Restrictions.Count != variableCount)
        {
            return (
                false,
                "The restriction line must have one restriction for every decision variable."
            );
        }

        return (true, string.Empty);
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
