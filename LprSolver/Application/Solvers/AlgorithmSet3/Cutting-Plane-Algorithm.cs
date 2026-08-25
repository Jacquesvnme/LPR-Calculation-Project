using LprSolver.Application.Solvers.AlgorithmSet1;
using LprSolver.Enums;
using LprSolver.Models;
using LprSolver.Services;

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
        if (PrimalSimplex.Success == null)
        {
            return new(false, "Primal simplex failed for cutting plane.", null);
        }

        //Create a working copy and send it onwards
        var (simplexTables, pivotColumns, pivotRows, columnNames) = SolveCuttingPlane(
            (double[,])PrimalSimplex.Tables[^1].Clone(),
            PrimalSimplex.ColumnNames
        );

        return await ExportCuttingPlane(simplexTables, pivotColumns, pivotRows, columnNames);
    }

    private (
        List<object> simplexTables,
        List<int> pivotColumns,
        List<int> pivotRows,
        List<string> columnNames
    ) SolveCuttingPlane()
    {
        // empty for now

        return (new List<object>(), new List<int>(), new List<int>(), new List<string>());
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
