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
        var tables = new List<object>();

        var exportReport = new ExportReport
        {
            AdditionalData = new AdditionalData(),
            ImportantDetails = new ImportantDetails(),
            SensitivityAnalysis = new SensitivityAnalysis(),
            Tables = new ExportTable { Tables = tables },
        };

        return new(true, "Dummy cutting plane table created successfully.", exportReport);
    }
}
