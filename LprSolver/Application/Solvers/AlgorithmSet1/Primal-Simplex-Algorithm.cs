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
        if (linearProgram == null)
        {
            throw new ArgumentNullException(
                nameof(linearProgram),
                "Linear program cannot be null."
            );
        }

        var workingCopy = linearProgram.DeepCopy();
        var initialTableau = PrimalSimplexUtils.BuildInitialTableau(workingCopy);

        // Main solving loop
        var counter = 0;
        var tables = new List<double[,]>();
        while (true)
        {
            var pivotColumnIndex = PrimalSimplexUtils.FindPivotColumn(initialTableau);
            if (pivotColumnIndex == -1)
            {
                // No negative values remain in row zero.
                // The current solution is optimal.
                break;
            }

            var pivotRowIndex = PrimalSimplexUtils.FindPivotRow(initialTableau, pivotColumnIndex);
            if (pivotRowIndex == -1)
            {
                // No positive entry exists in the pivot column.
                // The problem is unbounded.
                break;
            }

            counter++;
            var table = PrimalSimplexUtils.Pivot(
                initialTableau,
                pivotRowIndex,
                pivotColumnIndex
            );
            tables.Add(table);
        }
    }

    /// <summary>
    /// Exports the results of the primal simplex algorithm into an ExportReport object.
    /// </summary>
    /// <returns></returns>
    private async Task<(
        bool Success,
        string Message,
        ExportReport exportTableData
    )> ExportPrimalSimplex()
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
