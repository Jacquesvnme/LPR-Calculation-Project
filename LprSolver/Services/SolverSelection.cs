using LprSolver.Application.Solvers.AlgorithmSet1;
using LprSolver.Application.Solvers.AlgorithmSet2;
using LprSolver.Application.Solvers.AlgorithmSet3;
using LprSolver.Application.Solvers.AlgorithmSet4;
using LprSolver.Enums;
using LprSolver.Models;
using Spectre.Console;

namespace LprSolver.Services;

public interface ISolverSelection
{
    Task<SolverAlgorithm> GetUserSelectedOption();
    Task StartSolver(SolverAlgorithm solverAlgorithm, LinearProgram linearProgram);
}

public class SolverSelection : ISolverSelection
{
    private readonly IPrimal_Simplex_Algorithm _primal_Simplex_Algorithm;
    private readonly IRevised_Primal_Simplex_Algorithm _revised_Primal_Simplex_Algorithm;
    private readonly IB_B_Simplex_Algorithm _b_B_Simplex_Algorithm;
    private readonly IRevised_B_B_Simplex_Algorithm _revised_B_B_Simplex_Algorithm;
    private readonly ICutting_Plane_Algorithm _cutting_Plane_Algorithm;
    private readonly IRevised_Cutting_Plane_Algorithm _revised_Cutting_Plane_Algorithm;
    private readonly IB_B_Knapsack_Algorithm _b_B_Knapsack_Algorithm;
    private readonly INonLinearProblem _nonLinearProblem;

    public SolverSelection(
        IPrimal_Simplex_Algorithm primal_Simplex_Algorithm,
        IRevised_Primal_Simplex_Algorithm revised_Primal_Simplex_Algorithm,
        IB_B_Simplex_Algorithm b_B_Simplex_Algorithm,
        IRevised_B_B_Simplex_Algorithm revised_B_B_Simplex_Algorithm,
        ICutting_Plane_Algorithm cutting_Plane_Algorithm,
        IRevised_Cutting_Plane_Algorithm revised_Cutting_Plane_Algorithm,
        IB_B_Knapsack_Algorithm b_B_Knapsack_Algorithm,
        INonLinearProblem nonLinearProblem
    )
    {
        //Set 1
        _primal_Simplex_Algorithm = primal_Simplex_Algorithm;
        _revised_Primal_Simplex_Algorithm = revised_Primal_Simplex_Algorithm;
        //Set 2
        _b_B_Simplex_Algorithm = b_B_Simplex_Algorithm;
        _revised_B_B_Simplex_Algorithm = revised_B_B_Simplex_Algorithm;
        // Set 3
        _cutting_Plane_Algorithm = cutting_Plane_Algorithm;
        _revised_Cutting_Plane_Algorithm = revised_Cutting_Plane_Algorithm;
        // Set 4
        _b_B_Knapsack_Algorithm = b_B_Knapsack_Algorithm;
        // Set 5
        _nonLinearProblem = nonLinearProblem;
    }

    /// <summary>
    /// Gets the user-selected option
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task<SolverAlgorithm> GetUserSelectedOption()
    {
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select algorithm")
                .AddChoices(
                    "Primal Simplex",
                    "Revised Primal Simplex",
                    "Branch and Bound",
                    "Revised Branch and Bound",
                    "Cutting Plane",
                    "Revised Cutting Plane",
                    "Branch and Bound Knapsack",
                    "Non Linear"
                )
        );

        return selected switch
        {
            "Primal Simplex" => SolverAlgorithm.PrimalSimplex,
            "Revised Primal Simplex" => SolverAlgorithm.Revised_PrimalSimplex,
            "Branch and Bound" => SolverAlgorithm.BranchAndBound,
            "Revised Branch and Bound" => SolverAlgorithm.Revised_BranchAndBound,
            "Cutting Plane" => SolverAlgorithm.CuttingPlane,
            "Revised Cutting Plane" => SolverAlgorithm.Revised_CuttingPlane,
            "Branch and Bound Knapsack" => SolverAlgorithm.BranchAndBoundKnapsack,
            "Non Linear" => SolverAlgorithm.NonLinearProblem,
            _ => SolverAlgorithm.INVALID_OPTION,
        };
    }

    /// <summary>
    /// Starts the solver based on the selected algorithm.
    /// </summary>
    /// <param name="solverAlgorithm"></param>
    /// <returns></returns>
    public async Task StartSolver(SolverAlgorithm solverAlgorithm, LinearProgram linearProgram)
    {
        switch (solverAlgorithm)
        {
            case SolverAlgorithm.PrimalSimplex:
                _primal_Simplex_Algorithm.Execute(linearProgram);
                break;

            case SolverAlgorithm.Revised_PrimalSimplex:
                _revised_Primal_Simplex_Algorithm.Execute(linearProgram);
                break;

            case SolverAlgorithm.BranchAndBound:
                _b_B_Simplex_Algorithm.Execute(linearProgram);
                break;

            case SolverAlgorithm.Revised_BranchAndBound:
                _revised_B_B_Simplex_Algorithm.Execute(linearProgram);
                break;

            case SolverAlgorithm.CuttingPlane:
                _cutting_Plane_Algorithm.Execute(linearProgram);
                break;

            case SolverAlgorithm.Revised_CuttingPlane:
                _revised_Cutting_Plane_Algorithm.Execute(linearProgram);
                break;

            case SolverAlgorithm.BranchAndBoundKnapsack:
                _b_B_Knapsack_Algorithm.Execute(linearProgram);
                break;

            case SolverAlgorithm.NonLinearProblem:
                _nonLinearProblem.Execute(linearProgram);
                break;
        }
    }
}
