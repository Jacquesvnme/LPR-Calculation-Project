using System.Globalization;
using LprSolver.Application.SolverUtils;
using LprSolver.Models;

namespace LprSolver.Application.Solvers.AlgorithmSet2;

/// <summary>
/// Interface for the Algorithm.
/// </summary>
public interface IB_B_Simplex_Algorithm
{
    Task<(bool Success, string Message, ExportReport exportTableData)> Execute(
        LinearProgram linearProgram
    );
}

public class B_B_Simplex_Algorithm : IB_B_Simplex_Algorithm
{
    /// <summary>
    /// Class constructor for the Application class.
    /// </summary>
    public B_B_Simplex_Algorithm()
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
        // Add code here to implement the Algorithm.
        // Keep in mind that the return data should match the expected output format for the application.

        // Call your own custom methods inside this class but make them private to avoid exposing them outside of this class.
        OtherMethods();

        var tables = new List<object>();

        var exportReport = new ExportReport
        {
            AdditionalData = new AdditionalData(),
            ImportantDetails = new ImportantDetails(),
            SensitivityAnalysis = new SensitivityAnalysis(),
            Tables = new ExportTable { Tables = tables },
        };

        return new(
            true,
            "Dummy branch and bound simplex table created successfully.",
            exportReport
        );
    }

    private void OtherMethods()
    {
        //dummy method
    }

    private Task<(bool Success, string Message, ExportReport exportTableData)> ExportBBSimplex(
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

        return Task.FromResult((true, "Branch and bound simplex tables created successfully.", exportReport));
    }

    private AdditionalData Export_AdditionalData(
        List<int> pivotColumns,
        List<int> pivotRows,
        List<string> columnNames
    )
    {
        var additionalData = new AdditionalData
        {
            Title = "Additional Data for Branch And Bound Simplex Solver",
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
            Title = "Important Details for Branch And Bound Simplex Solver",
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
            Title = "Sensitivity Analysis for Branch And Bound Simplex Solver",
            Rows = new List<string>() { "None available" },
        };

        return sensitivityAnalysis;
    }

    private ExportTable Export_ExportTable(List<double[,]> simplexTables, List<string> columnNames)
    {
        var result = new ExportTable { Title = "Export Tables for Branch And Bound Simplex Solver" };

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
