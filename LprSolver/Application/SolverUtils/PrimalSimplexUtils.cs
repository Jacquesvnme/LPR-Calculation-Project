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

    {
        if (values == null || values.Count == 0)
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
}
