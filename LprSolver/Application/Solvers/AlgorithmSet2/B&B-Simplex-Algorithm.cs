using System.Globalization;
using LprSolver.Application.SolverUtils;
using LprSolver.Enums;
using LprSolver.Models;

namespace LprSolver.Application.Solvers.AlgorithmSet2;

/// <summary>
/// Interface for the Algorithm.
/// </summary>
public interface IB_B_Simplex_Algorithm
{
    Task<(bool Success, string Message, ExportReport exportTableData)> Execute(
        LinearProgram linearProgram
    );
}

public class B_B_Simplex_Algorithm : IB_B_Simplex_Algorithm
{
    // Safety net against runaway trees (bad data, cycling, etc.) - not a mathematical limit.
    private const int MaxNodesExplored = 500;

    /// <summary>
    /// Class constructor for the Application class.
    /// </summary>
    public B_B_Simplex_Algorithm()
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
        var (simplexTables, tableColumnNames, pivotHistory, solutionSummary) = SolveBBSimplex(
            linearProgram
        );

        return await ExportBBSimplex(
            simplexTables,
            tableColumnNames,
            pivotHistory,
            solutionSummary
        );
    }

    /// <summary>
    /// One completed pivot step, along with the column-name set that was active when it
    /// was performed (column names change whenever a branch adds a new S/E column).
    /// </summary>
    private sealed record PivotStep(int PivotColumn, int PivotRow, List<string> ColumnNames);

    /// <summary>
    /// One open node in the branch-and-bound search tree: an LP-optimal tableau
    /// (relaxation or branch-restricted) together with the column names that match it.
    /// </summary>
    private sealed record BbNode(double[,] Tableau, List<string> ColumnNames);

    /// <summary>
    /// Solves the linear program using the branch and bound simplex algorithm.
    /// </summary>
    private (
        List<double[,]> Tables,
        List<List<string>> TableColumnNames,
        List<PivotStep> PivotHistory,
        List<string> ColumnNames
    ) SolveBBSimplex(LinearProgram linearProgram)
    {
        if (linearProgram == null)
        {
            throw new ArgumentNullException(
                nameof(linearProgram),
                "Linear program cannot be null."
            );
        }

        // Add binary bounds to a copy so the imported model remains unchanged.
        var workingCopy = linearProgram.DeepCopy();

        // The tableau already assumes x >= 0, so each binary variable only needs
        // an explicit x <= 1 constraint to keep the LP relaxation within its bounds.
        for (
            var variableIndex = 0;
            variableIndex < workingCopy.Restriction.Restrictions.Count;
            variableIndex++
        )
        {
            if (workingCopy.Restriction.Restrictions[variableIndex] != VariableRestriction.Binary)
            {
                continue;
            }

            // Use a unit-vector row so the new constraint targets this variable only.
            var coefficients = new List<double>();
            for (var i = 0; i < workingCopy.Objective.Objectives.Count; i++)
            {
                coefficients.Add(i == variableIndex ? 1.0 : 0.0);
            }

            workingCopy.Constraints.Add(
                new Constraint
                {
                    Coefficients = coefficients,
                    Relation = ConstraintRelation.LessOrEqual,
                    RightHandSide = 1.0,
                }
            );
        }

        var (initialTableau, initialColumnNames) = PrimalSimplexUtils.BuildInitialTableau(
            workingCopy
        );

        var tables = new List<double[,]> { (double[,])initialTableau.Clone() };
        var tableColumnNames = new List<List<string>> { new List<string>(initialColumnNames) };
        var pivotHistory = new List<PivotStep>();

        // Solve the LP relaxation (the B&B root node) first.
        var rootTableau = RunPrimalSimplex(
            initialTableau,
            initialColumnNames,
            tables,
            tableColumnNames,
            pivotHistory
        );

        // Depth-first branch-and-bound search using an explicit stack.
        var openNodes = new Stack<BbNode>();
        openNodes.Push(new BbNode(rootTableau, initialColumnNames));

        double? bestObjectiveValue = null;
        List<string>? bestColumnNames = null;
        double[,]? bestTableau = null;

        var nodesExplored = 0;

        while (openNodes.Count > 0 && nodesExplored++ < MaxNodesExplored)
        {
            var node = openNodes.Pop();
            var nodeTableau = node.Tableau;
            var nodeColumnNames = node.ColumnNames;
            var rhsColumnIndex = nodeTableau.GetLength(1) - 1;

            // The solved relaxation gives the best objective this node can reach.
            var nodeObjectiveValue = nodeTableau[0, rhsColumnIndex];
            var isMaximization =
                linearProgram.Objective.Direction == OptimizationDirection.Maximize;

            // Prune the node when its bound cannot beat the best integer solution.
            var cannotImprove =
                bestObjectiveValue.HasValue
                && (
                    isMaximization
                        ? nodeObjectiveValue <= bestObjectiveValue.Value + 1e-9
                        : nodeObjectiveValue >= bestObjectiveValue.Value - 1e-9
                );

            if (cannotImprove)
            {
                continue;
            }

            var branchRow = BBSimplexUtils.FindBranchRow(nodeTableau);

            if (branchRow < 0)
            {
                // Every basic variable is integer - this is a candidate solution.
                var objectiveValue = nodeTableau[0, rhsColumnIndex];

                // Keep the candidate if it improves the chosen objective direction.
                var isBetter =
                    !bestObjectiveValue.HasValue
                    || (
                        isMaximization
                            ? objectiveValue > bestObjectiveValue.Value + 1e-9
                            : objectiveValue < bestObjectiveValue.Value - 1e-9
                    );

                if (isBetter)
                {
                    bestObjectiveValue = objectiveValue;
                    bestColumnNames = nodeColumnNames;
                    bestTableau = (double[,])nodeTableau.Clone();

                    tables.Add((double[,])nodeTableau.Clone());
                    tableColumnNames.Add(new List<string>(nodeColumnNames));
                }

                continue;
            }

            var branchValue = nodeTableau[branchRow, rhsColumnIndex];
            var floorBound = Math.Floor(branchValue);
            var ceilBound = Math.Ceiling(branchValue);

            // Split around the fractional value. Both constraints use the original
            // parent tableau so they branch on the same variable.
            var branchPlan = new[]
            {
                (Relation: ConstraintRelation.GreaterOrEqual, Bound: ceilBound),
                (Relation: ConstraintRelation.LessOrEqual, Bound: floorBound),
            };

            foreach (var (relation, bound) in branchPlan)
            {
                var (branchedTableau, branchedColumnNames) = BBSimplexUtils.AddBBConstraint(
                    nodeTableau,
                    nodeColumnNames,
                    relation,
                    bound
                );

                tables.Add((double[,])branchedTableau.Clone());
                tableColumnNames.Add(new List<string>(branchedColumnNames));

                // The new row can have a negative RHS (the parent's relaxed solution
                // violates the branch bound) - dual simplex restores feasibility
                // while keeping the objective row optimal, so no further primal
                // pivoting is needed once it finishes.
                var (repairedTableau, feasible) = RunDualSimplex(
                    branchedTableau,
                    branchedColumnNames,
                    tables,
                    tableColumnNames,
                    pivotHistory
                );

                if (!feasible)
                {
                    // No entering column could clear the negative RHS - this branch
                    // has no feasible solutions, so it's dropped from the search.
                    continue;
                }

                openNodes.Push(new BbNode(repairedTableau, branchedColumnNames));
            }
        }

        if (bestColumnNames is null)
        {
            throw new InvalidOperationException(
                "Branch and bound search did not find an integer-feasible solution."
            );
        }

        // The tableau already stores the objective with its final sign.
        var solutionSummary = BuildSolutionSummary(
            bestTableau!,
            bestColumnNames!,
            bestObjectiveValue!.Value
        );

        return (tables, tableColumnNames, pivotHistory, solutionSummary);
    }

    private List<string> BuildSolutionSummary(
        double[,] tableau,
        List<string> columnNames,
        double objectiveValue
    )
    {
        var summary = new List<string>
        {
            "Status: Optimal integer solution found",
            $"Optimal value (Z): {objectiveValue.ToString("0.####", CultureInfo.InvariantCulture)}",
        };

        var rowCount = tableau.GetLength(0);
        var rhsColumnIndex = tableau.GetLength(1) - 1;

        for (var column = 0; column < rhsColumnIndex; column++)
        {
            if (!columnNames[column].StartsWith("X", StringComparison.Ordinal))
            {
                continue;
            }

            var value = 0.0;
            for (var row = 1; row < rowCount; row++)
            {
                if (
                    Math.Abs(tableau[row, column] - 1.0) < 1e-9
                    && IsUnitColumn(tableau, row, column)
                )
                {
                    value = tableau[row, rhsColumnIndex];
                    break;
                }
            }

            summary.Add(
                $"{columnNames[column]} = {value.ToString("0.####", CultureInfo.InvariantCulture)}"
            );
        }

        return summary;
    }

    private static bool IsUnitColumn(double[,] tableau, int row, int column)
    {
        var rowCount = tableau.GetLength(0);
        for (var r = 0; r < rowCount; r++)
        {
            if (r != row && Math.Abs(tableau[r, column]) > 1e-9)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Repeatedly applies primal simplex pivots until no negative coefficient remains
    /// in the objective row (optimal) or the problem is found to be unbounded.
    /// </summary>
    private double[,] RunPrimalSimplex(
        double[,] tableau,
        List<string> columnNames,
        List<double[,]> tables,
        List<List<string>> tableColumnNames,
        List<PivotStep> pivotHistory
    )
    {
        var currentTableau = tableau;

        while (true)
        {
            var pivotColumn = PrimalSimplexUtils.FindPivotColumn(currentTableau);

            if (pivotColumn < 0)
            {
                // No negative coefficients remain in the objective row - optimal.
                return currentTableau;
            }

            var pivotRow = PrimalSimplexUtils.FindPivotRow(currentTableau, pivotColumn);

            if (pivotRow < 0)
            {
                throw new InvalidOperationException(
                    "The linear program is unbounded - no valid pivot row was found."
                );
            }

            currentTableau = PrimalSimplexUtils.Pivot(currentTableau, pivotRow, pivotColumn);

            pivotHistory.Add(new PivotStep(pivotColumn, pivotRow, columnNames));
            tables.Add((double[,])currentTableau.Clone());
            tableColumnNames.Add(new List<string>(columnNames));
        }
    }

    /// <summary>
    /// Repeatedly applies dual simplex pivots until every RHS is non-negative (feasible)
    /// or no entering column can be found for an infeasible row (branch is infeasible).
    /// </summary>
    private (double[,] Tableau, bool Feasible) RunDualSimplex(
        double[,] tableau,
        List<string> columnNames,
        List<double[,]> tables,
        List<List<string>> tableColumnNames,
        List<PivotStep> pivotHistory
    )
    {
        var currentTableau = tableau;

        while (true)
        {
            var pivotRow = BBSimplexUtils.FindDualPivotRow(currentTableau);

            if (pivotRow < 0)
            {
                // Every RHS is non-negative - the branch constraint is satisfied.
                return (currentTableau, true);
            }

            var pivotColumn = BBSimplexUtils.FindDualPivotColumn(currentTableau, pivotRow);

            if (pivotColumn < 0)
            {
                // No entering variable can clear the negative RHS - infeasible branch.
                return (currentTableau, false);
            }

            currentTableau = PrimalSimplexUtils.Pivot(currentTableau, pivotRow, pivotColumn);

            pivotHistory.Add(new PivotStep(pivotColumn, pivotRow, columnNames));
            tables.Add((double[,])currentTableau.Clone());
            tableColumnNames.Add(new List<string>(columnNames));
        }
    }

    private Task<(bool Success, string Message, ExportReport exportTableData)> ExportBBSimplex(
        List<double[,]> simplexTables,
        List<List<string>> tableColumnNames,
        List<PivotStep> pivotHistory,
        List<string> solutionSummary
    )
    {
        ArgumentNullException.ThrowIfNull(simplexTables);
        ArgumentNullException.ThrowIfNull(tableColumnNames);
        ArgumentNullException.ThrowIfNull(pivotHistory);
        ArgumentNullException.ThrowIfNull(solutionSummary);

        var exportReport = new ExportReport
        {
            AdditionalData = Export_AdditionalData(pivotHistory),
            ImportantDetails = Export_ImportantDetails(pivotHistory, solutionSummary),
            SensitivityAnalysis = Export_SensitivityAnalysis(),
            Tables = Export_ExportTable(simplexTables, tableColumnNames),
        };

        return Task.FromResult(
            (true, "Branch and bound simplex tables created successfully.", exportReport)
        );
    }

    private AdditionalData Export_AdditionalData(List<PivotStep> pivotHistory)
    {
        var additionalData = new AdditionalData
        {
            Title = "Additional Data for Branch And Bound Simplex Solver",
        };

        additionalData.Rows.Add($"Total Pivots Performed: {pivotHistory.Count}");

        var columns = "";
        foreach (var step in pivotHistory)
        {
            columns += $"{step.ColumnNames[step.PivotColumn]} (Index: {step.PivotColumn}), ";
        }
        additionalData.Rows.Add(columns);

        return additionalData;
    }

    private ImportantDetails Export_ImportantDetails(
        List<PivotStep> pivotHistory,
        List<string> solutionSummary
    )
    {
        var importantDetails = new ImportantDetails
        {
            Title = "Important Details for Branch And Bound Simplex Solver",
        };

        importantDetails.Rows.AddRange(solutionSummary);
        importantDetails.Rows.Add(string.Empty);
        importantDetails.Rows.Add("Pivot history:");

        for (int i = 0; i < pivotHistory.Count; i++)
        {
            var step = pivotHistory[i];
            var columnName = step.ColumnNames[step.PivotColumn];
            importantDetails.Rows.Add(
                $"Iteration {i + 1}: Pivot Column = {columnName} (Index: {step.PivotColumn}), Pivot Row = {step.PivotRow}"
            );
        }

        return importantDetails;
    }

    private SensitivityAnalysis Export_SensitivityAnalysis()
    {
        var sensitivityAnalysis = new SensitivityAnalysis
        {
            Title = "Sensitivity Analysis for Branch And Bound Simplex Solver",
            Rows = new List<string>() { "None available" },
        };

        return sensitivityAnalysis;
    }

    private ExportTable Export_ExportTable(
        List<double[,]> simplexTables,
        List<List<string>> tableColumnNames
    )
    {
        var result = new ExportTable
        {
            Title = "Export Tables for Branch And Bound Simplex Solver",
        };

        for (var tableIndex = 0; tableIndex < simplexTables.Count; tableIndex++)
        {
            var table = simplexTables[tableIndex];
            var columnNames = tableColumnNames[tableIndex];
            var rows = new List<List<string>>();

            var headings = new List<string> { "Row" };
            headings.AddRange(columnNames);
            headings.Add("RHS");
            rows.Add(headings);

            for (var row = 0; row < table.GetLength(0); row++)
            {
                var values = new List<string> { row.ToString() };

                for (var column = 0; column < table.GetLength(1); column++)
                {
                    values.Add(table[row, column].ToString("0.##", CultureInfo.InvariantCulture));
                }

                rows.Add(values);
            }

            result.Tables.Add(tableIndex == 0 ? "Table 0 (Initial)" : $"\nTable {tableIndex}");
            result.Tables.Add(rows);
        }

        return result;
    }
}
