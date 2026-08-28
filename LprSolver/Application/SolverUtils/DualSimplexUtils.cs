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

}
