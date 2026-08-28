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
        var (simplexTables, tableColumnNames, pivotHistory, columnNames) = SolveBBSimplex(
            linearProgram
        );

        return await ExportBBSimplex(simplexTables, tableColumnNames, pivotHistory, columnNames);
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

        var (initialTableau, initialColumnNames) = PrimalSimplexUtils.BuildInitialTableau(
            linearProgram.DeepCopy()
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

        var nodesExplored = 0;

        while (openNodes.Count > 0 && nodesExplored++ < MaxNodesExplored)
        {
            var node = openNodes.Pop();
            var nodeTableau = node.Tableau;
            var nodeColumnNames = node.ColumnNames;
            var rhsColumnIndex = nodeTableau.GetLength(1) - 1;

            // Bound: this node's relaxation can only get worse (or equal) as more
            // constraints are added below it, so if it's already no better than the
            // best integer solution found so far, there's no point exploring it.
            if (
                bestObjectiveValue.HasValue
                && nodeTableau[0, rhsColumnIndex] >= bestObjectiveValue.Value - 1e-9
            )
            {
                continue;
            }

            var branchRow = BBSimplexUtils.FindBranchRow(nodeTableau);

            if (branchRow < 0)
            {
                // Every basic variable is integer - this is a candidate solution.
                var objectiveValue = nodeTableau[0, rhsColumnIndex];

                if (!bestObjectiveValue.HasValue || objectiveValue < bestObjectiveValue.Value)
                {
                    bestObjectiveValue = objectiveValue;
                    bestColumnNames = nodeColumnNames;

                    tables.Add((double[,])nodeTableau.Clone());
                    tableColumnNames.Add(new List<string>(nodeColumnNames));
                }

                continue;
            }

            var branchValue = nodeTableau[branchRow, rhsColumnIndex];
            var floorBound = Math.Floor(branchValue);
            var ceilBound = Math.Ceiling(branchValue);

            // Branch on the same fractional row in both directions. AddBBConstraint
            // re-derives the branch row itself, so passing the unmodified parent
            // tableau to both calls is what keeps the branching variable consistent.
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

        return (tables, tableColumnNames, pivotHistory, bestColumnNames);
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
        List<string> columnNames
    )
    {
        ArgumentNullException.ThrowIfNull(simplexTables);
        ArgumentNullException.ThrowIfNull(tableColumnNames);
        ArgumentNullException.ThrowIfNull(pivotHistory);
        ArgumentNullException.ThrowIfNull(columnNames);

        var exportReport = new ExportReport
        {
            AdditionalData = Export_AdditionalData(pivotHistory),
            ImportantDetails = Export_ImportantDetails(pivotHistory),
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

    private ImportantDetails Export_ImportantDetails(List<PivotStep> pivotHistory)
    {
        var importantDetails = new ImportantDetails
        {
            Title = "Important Details for Branch And Bound Simplex Solver",
        };

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
