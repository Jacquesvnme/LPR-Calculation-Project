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
    Task<(bool IsSuccess, string Message, ExportReport exportReport)> StartSolver(
        SolverAlgorithm solverAlgorithm,
        LinearProgram linearProgram
    );
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
    /// Starts the solver based on the selected algorithm.
    /// </summary>
    /// <param name="solverAlgorithm"></param>
    /// <returns></returns>
    public async Task<(bool IsSuccess, string Message, ExportReport exportReport)> StartSolver(
        SolverAlgorithm solverAlgorithm,
        LinearProgram linearProgram
    )
    {
        switch (solverAlgorithm)
        {
            case SolverAlgorithm.PrimalSimplex:
                var primal_simplex = await _primal_Simplex_Algorithm.Execute(linearProgram);
                return new(
                    primal_simplex.Success,
                    primal_simplex.Message,
                    primal_simplex.exportTableData
                );

            case SolverAlgorithm.Revised_PrimalSimplex:
                var revised_primal_simplex = await _revised_Primal_Simplex_Algorithm.Execute(
                    linearProgram
                );
                return new(
                    revised_primal_simplex.Success,
                    revised_primal_simplex.Message,
                    revised_primal_simplex.exportTableData
                );

            case SolverAlgorithm.BranchAndBound:
                var branch_and_bound = await _b_B_Simplex_Algorithm.Execute(linearProgram);
                return new(
                    branch_and_bound.Success,
                    branch_and_bound.Message,
                    branch_and_bound.exportTableData
                );

            case SolverAlgorithm.Revised_BranchAndBound:
                var revised_branch_and_bound = await _revised_B_B_Simplex_Algorithm.Execute(
                    linearProgram
                );
                return new(
                    revised_branch_and_bound.Success,
                    revised_branch_and_bound.Message,
                    revised_branch_and_bound.exportTableData
                );

            case SolverAlgorithm.CuttingPlane:
                var cutting_plane = await _cutting_Plane_Algorithm.Execute(linearProgram);
                return new(
                    cutting_plane.Success,
                    cutting_plane.Message,
                    cutting_plane.exportTableData
                );

            case SolverAlgorithm.Revised_CuttingPlane:
                var revised_cutting_plane = await _revised_Cutting_Plane_Algorithm.Execute(
                    linearProgram
                );
                return new(
                    revised_cutting_plane.Success,
                    revised_cutting_plane.Message,
                    revised_cutting_plane.exportTableData
                );

            case SolverAlgorithm.BranchAndBoundKnapsack:
                var branch_and_bound_knapsack = await _b_B_Knapsack_Algorithm.Execute(
                    linearProgram
                );
                return new(
                    branch_and_bound_knapsack.Success,
                    branch_and_bound_knapsack.Message,
                    branch_and_bound_knapsack.exportTableData
                );

            case SolverAlgorithm.NonLinearProblem:
                var non_linear_problem = await _nonLinearProblem.Execute(linearProgram);
                return new(
                    non_linear_problem.Success,
                    non_linear_problem.Message,
                    non_linear_problem.exportTableData
                );

            default:
                return new(false, "Invalid solver algorithm", null);
        }
    }
}
