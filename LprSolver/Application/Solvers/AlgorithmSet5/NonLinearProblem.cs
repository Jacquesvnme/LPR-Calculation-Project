using LprSolver.Models;

namespace LprSolver.Application.Solvers.AlgorithmSet4;

/// <summary>
/// Interface for the Algorithm.
/// </summary>
public interface INonLinearProblem
{
    Task<(bool Success, string Message, ExportReport exportTableData)> Execute(
        LinearProgram linearProgram
    );
}

public class NonLinearProblem : INonLinearProblem
{
    /// <summary>
    /// Class constructor for the Application class.
    /// </summary>
    public NonLinearProblem()
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
        return new(false, "Method not implemented.", null);
    }
}
