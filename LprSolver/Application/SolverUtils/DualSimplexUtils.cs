namespace LprSolver.Application.SolverUtils;

public static class DualSimplexUtils
{
    // Floating-point calculations can produce very tiny negative values.
    // These very small values should just be 0.
    // The tolerance prevents those rounding errors from making the tableau appear infeasible.
    private const double TOLERANCE = 0.0000001;
    private const int MAXIMUM_ITERATIONS = 1000;

    #region Validation

    /// <summary>
    /// Checks that the tableau has the required dimensions, contains only finite values,
    /// and has a dual-feasible objective row.
    /// </summary>
    public static (bool Success, string Message) ValidateTableau(double[,]? tableau)
    {
        // No data to process
        if (tableau == null)
        {
            return (false, "Required data cannot be null.");
        }

        // The tableau must contain at least one objective row, one constraint row, and one RHS column.
        if (tableau.GetLength(0) < 2 || tableau.GetLength(1) < 2)
        {
            return (
                false,
                "The dual-simplex tableau must contain an objective row, a constraint row, and an RHS column."
            );
        }

        // Checking for NaN and infinity values in the tableau
        for (var row = 0; row < tableau.GetLength(0); row++)
        {
            for (var column = 0; column < tableau.GetLength(1); column++)
            {
                if (!double.IsFinite(tableau[row, column]))
                {
                    return (false, "Dual simplex can't contain infinite values or NaN.");
                }
            }
        }

        var rightHandSideColumn = tableau.GetLength(1) - 1;
        // This tableau convention is dual feasible when the objective coefficients
        // before the RHS are nonnegative within tolerance.
        for (var column = 0; column < rightHandSideColumn; column++)
        {
            if (tableau[0, column] < -TOLERANCE)
            {
                return (
                    false,
                    "The tableau is not feasible because its objective row contains a negative coefficient."
                );
            }
        }

        return (true, "No issues.");
    }

    #endregion

    #region Pivot Selection

    /// <summary>
    /// Selects the constraint row with the most negative RHS value.
    /// Returns -1 when every RHS value is nonnegative within tolerance.
    /// </summary>
    public static (bool Success, string Message, int PivotRow) FindPivotRow(double[,] tableau)
    {
        // Basic validation
        if (tableau == null)
        {
            return (false, "Required data cannot be null.", -1);
        }

        var rightHandSideColumn = tableau.GetLength(1) - 1;
        var pivotRow = -1;

        // The most negative RHS value is assigned to the small tolerance value.
        // This gets overridden by any negative RHS value in the tableau.
        var mostNegativeRightHandSide = -TOLERANCE;

        for (var row = 1; row < tableau.GetLength(0); row++)
        {
            // Finds the most negative RHS value and its row index.
            var rightHandSide = tableau[row, rightHandSideColumn];
            if (rightHandSide < mostNegativeRightHandSide)
            {
                mostNegativeRightHandSide = rightHandSide;
                pivotRow = row;
            }
        }

        return new(true, string.Empty, pivotRow);
    }

    /// <summary>
    /// Selects an entering column from the negative coefficients in the pivot row using
    /// the smallest dual-simplex ratio. Returns -1 when no column can enter.
    /// </summary>
    public static (bool Success, string Message, int PivotColumn) FindPivotColumn(
        double[,] tableau,
        int pivotRow
    )
    {
        // Basic validation
        if (tableau == null)
        {
            return new(false, "The tableau cannot be null.", -1);
        }

        if (pivotRow < 1 || pivotRow >= tableau.GetLength(0))
        {
            return new(false, "The pivot row must contain a constraint row.", -1);
        }

        var rightHandSideColumn = tableau.GetLength(1) - 1;
        var pivotColumn = -1;
        var smallestRatio = double.PositiveInfinity;

        for (var column = 0; column < rightHandSideColumn; column++)
        {
            var rowCoefficient = tableau[pivotRow, column];
            // Only a negative coefficient can increase the leaving row's RHS after pivoting.
            if (rowCoefficient >= -TOLERANCE)
            {
                continue;
            }

            // Validation permits tiny negative objective coefficients within tolerance.
            // Treat them as zero before calculating the ratio.
            var objectiveCoefficient = Math.Max(0, tableau[0, column]);
            var ratio = objectiveCoefficient / -rowCoefficient;

            // Find the smallest ratio and its column index.
            if (ratio < smallestRatio - TOLERANCE)
            {
                smallestRatio = ratio;
                pivotColumn = column;
            }
        }

        return new(true, string.Empty, pivotColumn);
    }

    #endregion

}
