using System.Globalization;
using LprSolver.Application.Solvers.AlgorithmSet1;
using LprSolver.Application.SolverUtils;
using LprSolver.Models;

namespace LprSolver.Application.Solvers.AlgorithmSet3;

/// <summary>
/// Interface for the Algorithm.
/// </summary>
public interface ICutting_Plane_Algorithm
{
    Task<(bool Success, string Message, ExportReport exportTableData)> Execute(
        LinearProgram linearProgram
    );
}

public class Cutting_Plane_Algorithm : ICutting_Plane_Algorithm
{
    private const int MAXIMUM_ITERATIONS = 1000;
    private readonly IPrimal_Simplex_Algorithm _primalSimplex;

    /// <summary>
    /// Class constructor for the Application class.
    /// </summary>
    public Cutting_Plane_Algorithm(IPrimal_Simplex_Algorithm primalSimplex)
    {
        // Dependency injection if required can be added here.

        _primalSimplex = primalSimplex;
    }

    /// <summary>
    /// Main method to execute the Algorithm.
    /// </summary>
    public async Task<(bool Success, string Message, ExportReport exportTableData)> Execute(
        LinearProgram linearProgram
    )
    {
        var PrimalSimplex = await _primalSimplex.Execute_WithoutFormatting(linearProgram);
        if (
            !PrimalSimplex.Success
            || PrimalSimplex.Tables.Count == 0
            || PrimalSimplex.ColumnNames.Count == 0
        )
        {
            return new(false, "Primal simplex failed for cutting plane.", null);
        }

        //Create a working copy and send it onwards
        var cuttingPlaneResult = SolveCuttingPlane(
            (double[,])PrimalSimplex.Tables[^1].Clone(),
            PrimalSimplex.ColumnNames
        );
        if (!cuttingPlaneResult.Success)
        {
            return new(false, cuttingPlaneResult.Message, null);
        }

        return await ExportCuttingPlane(
            cuttingPlaneResult.simplexTables,
            cuttingPlaneResult.pivotColumns,
            cuttingPlaneResult.pivotRows,
            cuttingPlaneResult.columnNames
        );
    }

    private (
        bool Success,
        string Message,
        List<object> simplexTables,
        List<int> pivotColumns,
        List<int> pivotRows,
        List<string> columnNames
    ) SolveCuttingPlane(double[,] initialTableau, List<string> columnNames)
    {
        if (initialTableau == null || columnNames == null)
        {
            return new(false, "Required data is empty", null, null, null, null);
        }

        // Cloning the existing data into new variables
        // Perserving the old data if needed
        var currentTableau = (double[,])initialTableau.Clone();
        var tables = new List<object> { (double[,])currentTableau.Clone() };
        var currentColumnNames = new List<string>(columnNames);

        // Values for keeping track of pivots and cut iteration
        var pivotColumns = new List<int>();
        var pivotRows = new List<int>();
        var cutNumber = 1;

        while (true)
        {
            // Determines the cutting plane index, row and values
            var cuttingIndexResult = CuttingPlaneUtils.DetermineCuttingIndex(
                currentTableau,
                currentColumnNames
            );

            // If cutting index is -1, then we have an integer solution or failure
            if (!cuttingIndexResult.Success)
            {
                return new(
                    false,
                    cuttingIndexResult.Message,
                    tables,
                    pivotColumns,
                    pivotRows,
                    currentColumnNames
                );
            }

            if (cuttingIndexResult.CuttingRow == -1)
            {
                break;
            }

            // Fail-safe, program has reached max iterations
            if (cutNumber > MAXIMUM_ITERATIONS)
            {
                return new(
                    false,
                    $"The cutting-plane algorithm exceeded {MAXIMUM_ITERATIONS} cuts.",
                    tables,
                    pivotColumns,
                    pivotRows,
                    currentColumnNames
                );
            }

            var cutResult = CuttingPlaneUtils.AddGomoryFractionalCut(
                currentTableau,
                currentColumnNames,
                cuttingIndexResult.CuttingRow,
                cutNumber
            );

            if (!cutResult.Success)
            {
                return new(
                    false,
                    cutResult.Message,
                    tables,
                    pivotColumns,
                    pivotRows,
                    currentColumnNames
                );
            }

            currentTableau = cutResult.Tableau;
            currentColumnNames = cutResult.ColumnNames;

            // Keeping history of results
            tables.Add((double[,])currentTableau.Clone());
            cutNumber++;

            // Normal dual simplex
            var dualSimplexResult = DualSimplexUtils.Solve(currentTableau);
            if (!dualSimplexResult.Success)
            {
                return new(
                    false,
                    dualSimplexResult.Message,
                    tables,
                    pivotColumns,
                    pivotRows,
                    currentColumnNames
                );
            }

            // Keeping history of results
            currentTableau = dualSimplexResult.Tableau;
            pivotColumns.AddRange(dualSimplexResult.PivotColumns);
            pivotRows.AddRange(dualSimplexResult.PivotRows);

            // Keeping history of results
            foreach (var table in dualSimplexResult.Tables)
            {
                tables.Add((double[,])table.Clone());
            }
        }

        return new(
            true,
            "The cutting-plane algorithm found an integer solution.",
            tables,
            pivotColumns,
            pivotRows,
            currentColumnNames
        );
    }

    private async Task<(
        bool Success,
        string Message,
        ExportReport exportTableData
    )> ExportCuttingPlane(
        List<object> simplexTables,
        List<int> pivotColumns,
        List<int> pivotRows,
        List<string> columnNames
    )
    {
        // Determine the number of generated cuts
        // Allocates simplexTables as an expected double[,] value
        var initialTableau = simplexTables[0] as double[,];
        var generatedCutCount = 0;

        if (initialTableau != null)
        {
            var originalColumnCount = initialTableau.GetLength(1) - 1;
            generatedCutCount = columnNames.Count - originalColumnCount;

            if (generatedCutCount < 0)
            {
                generatedCutCount = 0;
            }
        }

        // Add the additional information
        var additionalData = new AdditionalData
        {
            Title = "Additional Data for Cutting Plane Solver",
            Rows = new List<string>
            {
                $"Generated Cuts: {generatedCutCount}",
                $"Dual Simplex Pivots: {pivotColumns.Count}",
                $"Recorded Tableaus: {simplexTables.Count}",
            },
        };

        // Add the important information
        var importantDetails = new ImportantDetails
        {
            Title = "Important Details for Cutting Plane Solver",
        };

        for (var index = 0; index < pivotColumns.Count; index++)
        {
            var pivotColumn = pivotColumns[index];
            importantDetails.Rows.Add(
                $"Dual pivot {index + 1}: Column = {columnNames[pivotColumn]} (Index: {pivotColumn}), Row = {pivotRows[index]}"
            );
        }

        if (importantDetails.Rows.Count == 0)
        {
            importantDetails.Rows.Add("No dual-simplex pivots were required.");
        }

        // Add the tables information
        var exportTables = new ExportTable { Title = "Cutting Plane Tableaus" };

        for (var tableIndex = 0; tableIndex < simplexTables.Count; tableIndex++)
        {
            // loop through the tables and add them to the exportTables object
            if (simplexTables[tableIndex] is not double[,] tableau)
            {
                continue;
            }

            // Add the headings and rows for the tableau
            var tableauColumnCount = tableau.GetLength(1) - 1;
            var tableColumnNames = columnNames.Take(tableauColumnCount).ToList();
            var rows = new List<List<string>>();
            var headings = new List<string> { "Row" };
            headings.AddRange(tableColumnNames);
            headings.Add("RHS");
            rows.Add(headings);

            // Add the rows line for line,
            // formatting the values to 3 decimal place and ignoring sensitive formatting
            for (var row = 0; row < tableau.GetLength(0); row++)
            {
                var values = new List<string> { row.ToString() };
                for (var column = 0; column < tableau.GetLength(1); column++)
                {
                    values.Add(tableau[row, column].ToString("0.###", CultureInfo.InvariantCulture));
                }
                rows.Add(values);
            }

            // Add the table to the exportTables object with a title indicating the table index
            exportTables.Tables.Add(
                tableIndex == 0 ? "Table 0 (Primal optimum)" : $"\nTable {tableIndex}"
            );
            exportTables.Tables.Add(rows);
        }

        // Compile the final export report with all the gathered information
        var exportReport = new ExportReport
        {
            AdditionalData = additionalData,
            ImportantDetails = importantDetails,
            SensitivityAnalysis = new SensitivityAnalysis
            {
                Title = "Sensitivity Analysis for Cutting Plane Solver",
                Rows = new List<string> { "None available" },
            },
            Tables = exportTables,
        };

        // Console print all information
        ConsolePrinter.PrintOutputData(exportReport);

        return new(true, "Cutting-plane tables created successfully.", exportReport);
    }
}
