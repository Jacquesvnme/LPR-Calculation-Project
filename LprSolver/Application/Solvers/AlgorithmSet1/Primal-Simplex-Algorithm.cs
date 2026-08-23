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
}

public class Primal_Simplex_Algorithm : IPrimal_Simplex_Algorithm
{
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
        await SolvePrimalSimplex(linearProgram);

        return await ExportPrimalSimplex();
    }

    /// <summary>
    /// Solves the linear program using the primal simplex algorithm.
    /// </summary>
    /// <returns></returns>
    private async Task SolvePrimalSimplex(LinearProgram linearProgram)
    {
        var normalizedValues = PrimalSimplexUtils.NormalizeObjective(linearProgram.Objective.);
    }

    /// <summary>
    /// Exports the results of the primal simplex algorithm into an ExportReport object.
    /// </summary>
    /// <returns></returns>
    private async Task<(bool Success, string Message, ExportReport exportTableData)> ExportPrimalSimplex()
    {
        var tables = new List<object>();

        var exportReport = new ExportReport
        {
            AdditionalData = new AdditionalData(),
            ImportantDetails = new ImportantDetails(),
            SensitivityAnalysis = new SensitivityAnalysis(),
            Tables = new ExportTable { Tables = tables },
        };

        return new(true, "Dummy primal simplex table created successfully.", exportReport);
    }
}
