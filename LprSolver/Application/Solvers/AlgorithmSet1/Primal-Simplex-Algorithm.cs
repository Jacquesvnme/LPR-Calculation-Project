using LprSolver.Models;

namespace LprSolver.Application.Solvers.AlgorithmSet1;

/// <summary>
/// Interface for the Algorithm.
/// </summary>
public interface IPrimal_Simplex_Algorithm
{
    void Execute(LinearProgram linearProgram);
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
    public void Execute(LinearProgram linearProgram)
    {
        // Add code here to implement the Algorithm.
        // Keep in mind that the return data should match the expected output format for the application.

        // Call your own custom methods inside this class but make them private to avoid exposing them outside of this class.
        OtherMethods();
    }

    private void OtherMethods()
    {
        //dummy method
    }
}
