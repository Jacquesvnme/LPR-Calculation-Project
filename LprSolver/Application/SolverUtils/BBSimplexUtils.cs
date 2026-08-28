using LprSolver.Enums;

namespace LprSolver.Application.SolverUtils;

public static class BBSimplexUtils
{
    public static (double[,] Tableau, List<string> ColumnNames) AddBBConstraint(
        double[,] inputTable,
        List<string> columnNames,
        ConstraintRelation relation,
        double rightHandSide
    )
    {
        ArgumentNullException.ThrowIfNull(inputTable);
        ArgumentNullException.ThrowIfNull(columnNames);

        if (relation == ConstraintRelation.Equal)
        {
            throw new NotSupportedException(
                "Equality branch constraints aren't supported by this tableau – "
                    + "add an artificial-variable/Big-M row instead."
            );
        }

        int rowCount = inputTable.GetLength(0);
        int columnCount = inputTable.GetLength(1);
        int rhsCol = columnCount - 1;

        int ibranch = FindBranchRow(inputTable);
        if (ibranch < 0)
            throw new InvalidOperationException(
                "No fractional basic variable found – cannot create a branch constraint."
            );

        int basicCol = FindBasicColumn(inputTable, ibranch);
        if (basicCol < 0)
            throw new InvalidOperationException(
                $"Row {ibranch} has no identifiable basic variable – the tableau isn't in canonical form."
            );

        // +1 row, +1 column (new slack inserted immediately before RHS)
        int newRowCount = rowCount + 1;
        int newColumnCount = columnCount + 1;
        var newTableau = new double[newRowCount, newColumnCount];

        // Copy existing rows unchanged; the new slack column defaults to 0 for all of them.
        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 0; c < rhsCol; c++)
                newTableau[r, c] = inputTable[r, c];

            newTableau[r, newColumnCount - 1] = inputTable[r, rhsCol]; // RHS moves right
        }

        // Substitute the basic variable out of the bound using the branch row's own equation:
        // Either way x_j's own column must be exactly 0 - it's already accounted for via
        // substitution. Leaving it at 1 (a raw copy) would give two rows both claiming to
        // define the same basic variable, which isn't a valid tableau.
        var branchValue = inputTable[ibranch, rhsCol];
        var isFloorBranch = relation == ConstraintRelation.LessOrEqual;
        var sign = isFloorBranch ? -1.0 : 1.0;

        int newRowIdx = newRowCount - 1;
        int slackCol = newColumnCount - 2;

        for (int c = 0; c < rhsCol; c++)
            newTableau[newRowIdx, c] = c == basicCol ? 0.0 : sign * inputTable[ibranch, c];

        // Both branches genuinely need a +1 slack here - the sign flip is already baked into
        // the row above, so (unlike a normal <=/>= constraint) this is never -1.
        newTableau[newRowIdx, slackCol] = 1.0;

        newTableau[newRowIdx, newColumnCount - 1] = isFloorBranch
            ? rightHandSide - branchValue
            : branchValue - rightHandSide;

        // Build column names with the new slack inserted before RHS.
        var newColumnNames = new List<string>(columnCount);
        for (int i = 0; i < rhsCol; i++)
            newColumnNames.Add(columnNames[i]);

        newColumnNames.Add($"S{slackCol}");

        return (newTableau, newColumnNames);
    }

    /// <summary>
    /// Finds the column that is basic in the given row: the column with a 1 in this row
    /// and 0 in every other row. Returns -1 if the row isn't in valid canonical form.
    /// </summary>
    public static int FindBasicColumn(double[,] tableau, int row)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        int rowCount = tableau.GetLength(0);
        int columnCount = tableau.GetLength(1) - 1; // exclude RHS
        const double tolerance = 1e-9;

        for (int c = 0; c < columnCount; c++)
        {
            if (Math.Abs(tableau[row, c] - 1.0) > tolerance)
                continue;

            bool isUnitColumn = true;
            for (int r = 0; r < rowCount; r++)
            {
                if (r != row && Math.Abs(tableau[r, c]) > tolerance)
                {
                    isUnitColumn = false;
                    break;
                }
            }

            if (isUnitColumn)
                return c;
        }

        return -1;
    }

    public static int FindBranchRow(double[,] tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        int rowCount = tableau.GetLength(0);
        int rhsCol = tableau.GetLength(1) - 1;
        int branchRow = -1;
        double maxFrac = 0.0;

        // Skip objective row (assumed to be row 0)
        for (int r = 1; r < rowCount; r++)
        {
            double rhs = tableau[r, rhsCol];
            double frac = rhs - Math.Floor(rhs);

            // Prefer the largest fractional part; ignore pure integers
            if (frac > maxFrac + 1e-12) // small tolerance for floating-point
            {
                maxFrac = frac;
                branchRow = r;
            }
        }

        return branchRow; // –1 when the basis is already integer
    }

    /// <summary>
    /// Finds the constraint row with the most negative RHS – the row that is
    /// currently violating primal feasibility after a branch constraint was added.
    /// Returns -1 once every RHS value is non-negative (primal feasible).
    /// </summary>
    public static int FindDualPivotRow(double[,] tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        int rowCount = tableau.GetLength(0);
        int rhsColumnIndex = tableau.GetLength(1) - 1;
        int pivotRowIndex = -1;
        double mostNegativeRhs = 0.0;

        // Skip the objective row (row 0); only constraint rows can be primal-infeasible.
        for (int r = 1; r < rowCount; r++)
        {
            double rhs = tableau[r, rhsColumnIndex];

            if (rhs < mostNegativeRhs)
            {
                mostNegativeRhs = rhs;
                pivotRowIndex = r;
            }
        }

        return pivotRowIndex;
    }

    /// <summary>
    /// Finds the entering column for a dual-simplex pivot on the given (infeasible) row.
    /// Only negative entries in that row are valid candidates; among them, the one with
    /// the smallest ratio of |objective coefficient / row coefficient| is chosen so the
    /// pivot keeps the tableau dual-feasible (i.e. keeps the objective row optimal).
    /// Returns -1 when no negative entry exists in the row, meaning the branch is infeasible.
    /// </summary>
    public static int FindDualPivotColumn(double[,] tableau, int pivotRowIndex)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        int columnCount = tableau.GetLength(1);
        int rhsColumnIndex = columnCount - 1;
        int pivotColumnIndex = -1;
        double smallestRatio = double.PositiveInfinity;

        for (int c = 0; c < rhsColumnIndex; c++)
        {
            double rowValue = tableau[pivotRowIndex, c];

            // Only negative entries can serve as the leaving-variable's replacement.
            if (rowValue >= 0.0)
            {
                continue;
            }

            double ratio = Math.Abs(tableau[0, c] / rowValue);

            if (ratio < smallestRatio)
            {
                smallestRatio = ratio;
                pivotColumnIndex = c;
            }
        }

        return pivotColumnIndex;
    }
}
