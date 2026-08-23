using LprSolver.Enums;
using LprSolver.Models;

namespace LprSolver.Application.SolverUtils;

public static class PrimalSimplexUtils
{
    /// <summary>
    /// Used for creating a deep copy of a LinearProgram object to avoid modifying the original.
    /// </summary>
    public static LinearProgram DeepCopy(this LinearProgram linearProgram)
    {
        ArgumentNullException.ThrowIfNull(linearProgram);

        return new LinearProgram(
            linearProgram.Message,
            linearProgram.IsSuccess,
            new Objective
            {
                Direction = linearProgram.Objective.Direction,
                Objectives = new List<double>(linearProgram.Objective.Objectives),
            },
            linearProgram
                .Constraints.Select(constraint => new Constraint
                {
                    Coefficients = new List<double>(constraint.Coefficients),
                    Relation = constraint.Relation,
                    RightHandSide = constraint.RightHandSide,
                })
                .ToList(),
            new Restriction
            {
                Restrictions = new List<VariableRestriction>(
                    linearProgram.Restriction.Restrictions
                ),
            }
        );
    }

    /// <summary>
    /// Adds the slack or excess values
    /// Adds a slack-variable column for each less-than-or-equal constraint and an
    /// excess-variable column for each greater-than-or-equal constraint.
    /// Slack variables receive a value of one and excess variables receive minus one.
    /// </summary>
    public static (List<Constraint> Constraints, List<string> ColumnNames) AddSlackOrExcess(
        List<Constraint> constraints
    )
    {
        if (constraints is null || constraints.Count == 0)
        {
            throw new ArgumentException(
                "Constraints cannot be null or empty.",
                nameof(constraints)
            );
        }

        var updatedConstraints = new List<Constraint>();

        // Equality constraints do not receive a slack or excess column.
        var slackOrExcessCount = constraints.Count(constraint =>
            constraint.Relation != ConstraintRelation.Equal
        );
        var currentSlackOrExcessIndex = 0;

        for (var constraintIndex = 0; constraintIndex < constraints.Count; constraintIndex++)
        {
            var currentConstraint = constraints[constraintIndex];

            // Keep the original decision-variable coefficients and append new columns to the copy.
            var updatedCoefficients = new List<double>(currentConstraint.Coefficients);

            // A value of -1 means this equality row has no assigned auxiliary column.
            var assignedColumnIndex = -1;

            if (currentConstraint.Relation != ConstraintRelation.Equal)
            {
                assignedColumnIndex = currentSlackOrExcessIndex;
                currentSlackOrExcessIndex++;
            }

            for (var columnIndex = 0; columnIndex < slackOrExcessCount; columnIndex++)
            {
                var addedValue = 0.0;

                // Only the auxiliary column belonging to this row receives a non-zero value.
                if (columnIndex == assignedColumnIndex)
                {
                    if (currentConstraint.Relation == ConstraintRelation.LessOrEqual)
                    {
                        addedValue = 1.0;
                    }
                    else if (currentConstraint.Relation == ConstraintRelation.GreaterOrEqual)
                    {
                        addedValue = -1.0;
                    }
                }

                updatedCoefficients.Add(addedValue);
            }

            updatedConstraints.Add(
                new Constraint
                {
                    Coefficients = updatedCoefficients,
                    Relation = currentConstraint.Relation,
                    RightHandSide = currentConstraint.RightHandSide,
                }
            );
        }

        var columnNames = AddColumnNames(constraints);

        return (updatedConstraints, columnNames);
    }

    /// <summary>
    /// Creates the column names for the tableau for later tracking.
    /// E.g. X1, S1, E1.
    /// </summary>
    public static List<string> AddColumnNames(List<Constraint> constraints)
    {
        if (constraints is null || constraints.Count == 0)
        {
            throw new ArgumentException(
                "Constraints cannot be null or empty.",
                nameof(constraints)
            );
        }

        var columnNames = new List<string>();

        // The importer guarantees that every constraint has the same decision-variable count.
        var decisionVariableCount = constraints[0].Coefficients.Count;

        // Decision-variable columns always appear first in the tableau.
        for (var variableIndex = 0; variableIndex < decisionVariableCount; variableIndex++)
        {
            columnNames.Add($"X{variableIndex + 1}");
        }

        // Use the original constraint number so each S or E name maps back to its source row.
        for (var constraintIndex = 0; constraintIndex < constraints.Count; constraintIndex++)
        {
            var relation = constraints[constraintIndex].Relation;
            var constraintNumber = constraintIndex + 1;

            if (relation == ConstraintRelation.LessOrEqual)
            {
                columnNames.Add($"S{constraintNumber}");
            }
            else if (relation == ConstraintRelation.GreaterOrEqual)
            {
                columnNames.Add($"E{constraintNumber}");
            }
        }

        return columnNames;
    }

    /// <summary>
    /// Finds the index of the most negative coefficient in the normalized objective row.
    /// Returns -1 when no negative coefficient remains and the current tableau is optimal.
    /// </summary>
    public static int FindPivotColumn(double[,] tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        if (tableau.GetLength(0) == 0 || tableau.GetLength(1) < 2)
        {
            throw new ArgumentException(
                "The tableau must contain an objective row and at least one value column.",
                nameof(tableau)
            );
        }

        var pivotColumnIndex = -1;
        var mostNegativeValue = 0.0;

        // Row zero is the objective row. The final RHS column is not a pivot candidate.
        for (var columnIndex = 0; columnIndex < tableau.GetLength(1) - 1; columnIndex++)
        {
            var currentValue = tableau[0, columnIndex];

            // Using < keeps the left-most column when values are tied.
            if (currentValue < mostNegativeValue)
            {
                mostNegativeValue = currentValue;
                pivotColumnIndex = columnIndex;
            }
        }

        return pivotColumnIndex;
    }

    /// <summary>
    /// Finds the constraint row with the smallest non-negative RHS-to-column ratio.
    /// Returns -1 when the pivot column has no valid positive entry, meaning unboundedness.
    /// </summary>
    public static int FindPivotRow(double[,] tableau, int pivotColumnIndex)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        if (tableau.GetLength(0) < 2 || tableau.GetLength(1) < 2)
        {
            throw new ArgumentException(
                "The tableau must contain an objective row and at least one constraint row.",
                nameof(tableau)
            );
        }

        var rightHandSideColumnIndex = tableau.GetLength(1) - 1;

        if (pivotColumnIndex < 0 || pivotColumnIndex >= rightHandSideColumnIndex)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pivotColumnIndex),
                "The pivot column must refer to a value column before the RHS."
            );
        }

        var pivotRowIndex = -1;
        var smallestRatio = double.PositiveInfinity;

        // Constraint rows begin at row one because row zero contains the objective.
        for (var rowIndex = 1; rowIndex < tableau.GetLength(0); rowIndex++)
        {
            var pivotColumnValue = tableau[rowIndex, pivotColumnIndex];

            // Zero and negative column values cannot participate in the minimum-ratio test.
            if (pivotColumnValue <= 0.0)
            {
                continue;
            }

            var rightHandSide = tableau[rowIndex, rightHandSideColumnIndex];
            var ratio = rightHandSide / pivotColumnValue;

            // Using < keeps the first valid row when two ratios are equal.
            if (ratio >= 0.0 && ratio < smallestRatio)
            {
                smallestRatio = ratio;
                pivotRowIndex = rowIndex;
            }
        }

        return pivotRowIndex;
    }

    /// <summary>
    /// Checks whether any constraint row has a negative right-hand-side value.
    /// A negative RHS means the starting tableau is not primal feasible.
    /// </summary>
    public static bool HasNegativeRightHandSide(double[,] tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        if (tableau.GetLength(0) < 2 || tableau.GetLength(1) < 2)
        {
            throw new ArgumentException(
                "The tableau must contain an objective row and at least one constraint row.",
                nameof(tableau)
            );
        }

        // Floating-point calculations can produce tiny negative values that should be zero.
        // The tolerance prevents those rounding errors from making the tableau appear infeasible.
        const double tolerance = 0.0000001;
        var rightHandSideColumnIndex = tableau.GetLength(1) - 1;

        for (var rowIndex = 1; rowIndex < tableau.GetLength(0); rowIndex++)
        {
            if (tableau[rowIndex, rightHandSideColumnIndex] < -tolerance)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns back negative values for maximization problems and positive values for minimization problems.
    /// </summary>
    public static List<double> NormalizeObjective(
        List<double> values,
        OptimizationDirection direction
    )
    {
        if (values is null || values.Count == 0)
        {
            throw new ArgumentException("Values cannot be null or empty.", nameof(values));
        }

        var multiplier = direction == OptimizationDirection.Maximize ? -1 : 1;

        return values.Select(value => value * multiplier).ToList();
    }

    public static double[,] Pivot(
        double[,] table,
        int pivotRowIndex,
        int pivotColumnIndex
    )
    {
        // Replace
        return null;
    }

    /// <summary>
    /// Creates the initial tableau in a format that is easier to manipulate
    /// </summary>
    public static (double[,] tableau, List<string> columnNames) BuildInitialTableau(
        LinearProgram workingCopy
    )
    {
        (workingCopy, var columnNames) = GetRequiredInformation(workingCopy);

        // Number of rows + z row
        var rowCount = workingCopy.Constraints.Count + 1;

        // Number of columns + RHS
        var columnCount = workingCopy.Constraints[0].Coefficients.Count + 1;

        // Width x Height
        double[,] tableau = new double[rowCount, columnCount];
        var rightHandSideColumnIndex = columnCount - 1;

        // Adds the objectives and then 0 for all other values
        for (var columnIndex = 0; columnIndex < rightHandSideColumnIndex; columnIndex++)
        {
            if (columnIndex < workingCopy.Objective.Objectives.Count)
            {
                tableau[0, columnIndex] = workingCopy.Objective.Objectives[columnIndex];
            }
            else
            {
                tableau[0, columnIndex] = 0.0;
            }
        }

        // Add the constraints
        for (var rowIndex = 1; rowIndex < rowCount; rowIndex++)
        {
            // Constraint zero maps to tableau row one, constraint one to row two, and so on.
            var constraint = workingCopy.Constraints[rowIndex - 1];

            for (var columnIndex = 0; columnIndex < constraint.Coefficients.Count; columnIndex++)
            {
                tableau[rowIndex, columnIndex] = constraint.Coefficients[columnIndex];
            }

            // The RHS is stored separately on the constraint and belongs in the final column.
            tableau[rowIndex, rightHandSideColumnIndex] = constraint.RightHandSide;
        }

        return new(tableau, columnNames);
    }

    /// <summary>
    /// Calls the normalization and slack/excess methods to prepare the working copy
    /// </summary>
    private static (LinearProgram workingCopy, List<string> columnNames) GetRequiredInformation(
        LinearProgram workingCopy
    )
    {
        if (workingCopy.Objective.Direction == OptimizationDirection.Maximize)
        {
            workingCopy.Objective.Objectives = PrimalSimplexUtils.NormalizeObjective(
                workingCopy.Objective.Objectives,
                workingCopy.Objective.Direction
            );
        }

        var slackOrExcessResult = PrimalSimplexUtils.AddSlackOrExcess(workingCopy.Constraints);
        workingCopy.Constraints = slackOrExcessResult.Constraints;

        return new(workingCopy, slackOrExcessResult.ColumnNames);
    }
}
