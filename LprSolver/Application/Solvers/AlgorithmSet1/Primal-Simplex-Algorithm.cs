using System.Globalization;
using LprSolver.Application.SolverUtils;
using LprSolver.Models;

namespace LprSolver.Application.Solvers.AlgorithmSet1;

/// <summary>
/// Interface for the Algorithm.
/// </summary>
public interface IPrimal_Simplex_Algorithm
{
    Task<(bool Success, string Message, ExportReport exportTableData)> Execute(
        LinearProgram linearProgram
    );

    Task<(
        bool Success,
        string Message,
        List<double[,]> Tables,
        List<int> PivotColumns,
        List<int> PivotRows,
        List<string> ColumnNames
    )> Execute_WithoutFormatting(LinearProgram linearProgram);
}

public class Primal_Simplex_Algorithm : IPrimal_Simplex_Algorithm
{
    private const int MAXIMUMITERATIONS = 1000;

    /// <summary>
    /// Class constructor for the Application class.
    /// </summary>
    public Primal_Simplex_Algorithm()
    {
        // Dependency injection if required can be added here.
    }

    /// <summary>
    /// Main method to execute the Algorithm.
    /// </summary>
    public async Task<(bool Success, string Message, ExportReport exportTableData)> Execute(
        LinearProgram linearProgram
    )
    {
        var (simplexTables, pivotColumns, pivotRows, columnNames) = SolvePrimalSimplex(
            linearProgram
        );

        return await ExportPrimalSimplex(simplexTables, pivotColumns, pivotRows, columnNames);
    }

    /// <summary>
    /// Main method, with formatting excluded.
    /// </summary>
    public async Task<(
        bool Success,
        string Message,
        List<double[,]> Tables,
        List<int> PivotColumns,
        List<int> PivotRows,
        List<string> ColumnNames
    )> Execute_WithoutFormatting(LinearProgram linearProgram)
    {
        var (simplexTables, pivotColumns, pivotRows, columnNames) = SolvePrimalSimplex(
            linearProgram
        );

        return new(true, "No Issues", simplexTables, pivotColumns, pivotRows, columnNames);
    }

    /// <summary>
    /// Solves the linear program using the primal simplex algorithm.
    /// </summary>
    /// <returns></returns>
    private (
        List<double[,]> Tables,
        List<int> PivotColumns,
        List<int> PivotRows,
        List<string> ColumnNames
    ) SolvePrimalSimplex(LinearProgram linearProgram)
    {
        if (linearProgram == null)
        {
            throw new ArgumentNullException(
                nameof(linearProgram),
                "Linear program cannot be null."
            );
        }

        var (currentTableau, columnNames) = PrimalSimplexUtils.BuildInitialTableau(
            linearProgram.DeepCopy()
        );

        if (PrimalSimplexUtils.HasNegativeRightHandSide(currentTableau))
        {
            throw new InvalidOperationException(
                "The initial tableau has a negative RHS and requires preprocessing or Phase I."
            );
        }

        // Main solving loop
        var iteration = 0;
        var tables = new List<double[,]> { (double[,])currentTableau.Clone() };

        List<int> pivotRows = new();
        List<int> pivotColumns = new();

        while (true)
        {
            var pivotColumnIndex = PrimalSimplexUtils.FindPivotColumn(currentTableau);
            if (pivotColumnIndex == -1)
            {
                // No negative values remain in row zero.
                // The current solution is optimal.
                break;
            }

            if (iteration >= MAXIMUMITERATIONS)
            {
                // Infeasible solution

                throw new InvalidOperationException(
                    $"The primal simplex algorithm exceeded {MAXIMUMITERATIONS} iterations."
                );
            }

            var pivotRowIndex = PrimalSimplexUtils.FindPivotRow(currentTableau, pivotColumnIndex);
            if (pivotRowIndex == -1)
            {
                // No positive entry exists in the pivot column.
                // The problem is unbounded.
                throw new InvalidOperationException(
                    "The linear program is unbounded because no valid pivot row was found."
                );
            }

            pivotColumns.Add(pivotColumnIndex);
            pivotRows.Add(pivotRowIndex);

            currentTableau = PrimalSimplexUtils.Pivot(
                currentTableau,
                pivotRowIndex,
                pivotColumnIndex
            );
            tables.Add((double[,])currentTableau.Clone());
            iteration++;
        }

        return new(tables, pivotColumns, pivotRows, columnNames);
    }

    /// <summary>
    /// Exports the results of the primal simplex algorithm into an ExportReport object.
    /// </summary>
    /// <returns></returns>
    private Task<(bool Success, string Message, ExportReport exportTableData)> ExportPrimalSimplex(
        List<double[,]> simplexTables,
        List<int> pivotColumns,
        List<int> pivotRows,
        List<string> columnNames
    )
    {
        ArgumentNullException.ThrowIfNull(simplexTables);
        ArgumentNullException.ThrowIfNull(pivotColumns);
        ArgumentNullException.ThrowIfNull(pivotRows);
        ArgumentNullException.ThrowIfNull(columnNames);

        var exportReport = new ExportReport
        {
            AdditionalData = Export_AdditionalData(pivotColumns, pivotRows, columnNames),
            ImportantDetails = Export_ImportantDetails(pivotColumns, pivotRows, columnNames),
            SensitivityAnalysis = Export_SensitivityAnalysis(),
            Tables = Export_ExportTable(simplexTables, columnNames),
        };

        return Task.FromResult((true, "Primal simplex tables created successfully.", exportReport));
    }

    private AdditionalData Export_AdditionalData(
        List<int> pivotColumns,
        List<int> pivotRows,
        List<string> columnNames
    )
    {
        var additionalData = new AdditionalData
        {
            Title = "Additional Data for Primal Simplex Solver",
        };

        additionalData.Rows.Add($"Total Columns: {pivotColumns.Count}");
        additionalData.Rows.Add($"Total Rows: {pivotRows.Count}");

        var columns = "";
        foreach (var column in pivotColumns)
        {
            columns += $"{columnNames[column]} (Index: {column}), ";
        }
        additionalData.Rows.Add(columns);

        return additionalData;
    }

    private ImportantDetails Export_ImportantDetails(
        List<int> pivotColumns,
        List<int> pivotRows,
        List<string> columnNames
    )
    {
        var importantDetails = new ImportantDetails
        {
            Title = "Important Details for Primal Simplex Solver",
        };

        for (int i = 0; i < pivotColumns.Count; i++)
        {
            var pivotColumn = pivotColumns[i];
            var pivotRow = pivotRows[i];
            var columnName = columnNames[pivotColumn];
            importantDetails.Rows.Add(
                $"Iteration {i + 1}: Pivot Column = {columnName} (Index: {pivotColumn}), Pivot Row = {pivotRow}"
            );
        }

        return importantDetails;
    }

    private SensitivityAnalysis Export_SensitivityAnalysis()
    {
        var sensitivityAnalysis = new SensitivityAnalysis
        {
            Title = "Sensitivity Analysis for Primal Simplex Solver",
            Rows = new List<string>() { "None available" },
        };

        return sensitivityAnalysis;
    }

    private ExportTable Export_ExportTable(List<double[,]> simplexTables, List<string> columnNames)
    {
        var result = new ExportTable { Title = "Export Tables for Primal Simplex Solver" };

        for (var tableIndex = 0; tableIndex < simplexTables.Count; tableIndex++)
        {
            var table = simplexTables[tableIndex];
            var rows = new List<List<string>>();

            var headings = new List<string> { "Row" };
            headings.AddRange(columnNames);
            headings.Add("RHS");
            rows.Add(headings);

            for (var row = 0; row < table.GetLength(0); row++)
            {
                var values = new List<string> { row.ToString() };

                for (var column = 0; column < table.GetLength(1); column++)
                {
                    values.Add(table[row, column].ToString("0.##", CultureInfo.InvariantCulture));
                }

                rows.Add(values);
            }

            result.Tables.Add(tableIndex == 0 ? "Table 0 (Initial)" : $"\nTable {tableIndex}");
            result.Tables.Add(rows);
        }

        return result;
    }
}
