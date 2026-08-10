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
    /// <summary>
    /// Class constructor for the Application class.
    /// </summary>
    public Cutting_Plane_Algorithm()
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

        return new(true, "Dummy cutting plane table created successfully.", exportReport);
    }

    private void OtherMethods()
    {
        //dummy method
    }
}
