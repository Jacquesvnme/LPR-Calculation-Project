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
        // Add code here to implement the Algorithm.
        // Keep in mind that the return data should match the expected output format for the application.

        // Call your own custom methods inside this class but make them private to avoid exposing them outside of this class.
        OtherMethods();

        var tables = new List<object>();

        var exportReport = new ExportReport
        {
            AdditionalData = new List<string>(),
            ImportantDetails = new List<string>(),
            SensitivityAnalysis = new List<string>(),
            Tables = tables,
        };

        return new(true, "Dummy non-linear problem table created successfully.", exportReport);
    }

    private void OtherMethods()
    {
        //dummy method
    }
}
