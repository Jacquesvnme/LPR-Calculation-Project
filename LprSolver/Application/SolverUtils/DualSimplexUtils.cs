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

    #region Solver

    /// <summary>
    /// Applies dual-simplex pivots until the tableau becomes primal feasible or reaches
    /// a terminal failure. Returns the final tableau and the recorded pivot history.
    /// </summary>
    public static (
        bool Success,
        string Message,
        double[,] Tableau,
        List<double[,]> Tables,
        List<int> PivotColumns,
        List<int> PivotRows
    ) Solve(double[,] initialTableau, int maximumIterations = MAXIMUM_ITERATIONS)
    {
        var validation = ValidateTableau(initialTableau);
        if (!validation.Success)
        {
            return (
                false,
                validation.Message,
                new double[0, 0],
                new List<double[,]>(),
                new List<int>(),
                new List<int>()
            );
        }

        if (maximumIterations < 1)
        {
            return (
                false,
                "The maximum number of dual-simplex iterations must be greater than zero.",
                new double[0, 0],
                new List<double[,]>(),
                new List<int>(),
                new List<int>()
            );
        }

        // Work on a copy so solving does not modify the caller's tableau.
        var currentTableau = (double[,])initialTableau.Clone();
        var tables = new List<double[,]>();
        var pivotColumns = new List<int>();
        var pivotRows = new List<int>();
        var iteration = 0;

        while (true)
        {
            // Gets the pivot row
            var pivotRowResult = FindPivotRow(currentTableau);
            if (!pivotRowResult.Success)
            {
                // No negative constraint RHS remains, so primal feasibility is restored.
                return (
                    true,
                    "The dual-simplex tableau is feasible.",
                    currentTableau,
                    tables,
                    pivotColumns,
                    pivotRows
                );
            }

            // Max iterations reached
            if (iteration >= maximumIterations)
            {
                return (
                    false,
                    $"The dual-simplex algorithm exceeded {maximumIterations} iterations.",
                    currentTableau,
                    tables,
                    pivotColumns,
                    pivotRows
                );
            }

            // Gets the pivot column
            var pivotColumnResult = FindPivotColumn(currentTableau, pivotRowResult.PivotRow);
            if (!pivotColumnResult.Success)
            {
                // A negative RHS with no eligible entering column cannot be repaired.
                return (
                    false,
                    $"No pivot column has been found for dual-simplex row {pivotRowResult.PivotRow} & {pivotRowResult.Message}.",
                    currentTableau,
                    tables,
                    pivotColumns,
                    pivotRows
                );
            }

            // Both simplex methods use the same row-operation implementation.
            currentTableau = PrimalSimplexUtils.Pivot(
                currentTableau,
                pivotRowResult.PivotRow,
                pivotColumnResult.PivotColumn
            );

            var iterationValidation = ValidateTableau(currentTableau);
            if (!iterationValidation.Success)
            {
                return (
                    false,
                    iterationValidation.Message,
                    currentTableau,
                    tables,
                    pivotColumns,
                    pivotRows
                );
            }

            // Keep track of the pivot history
            pivotColumns.Add(pivotColumnResult.PivotColumn);
            pivotRows.Add(pivotRowResult.PivotRow);

            // Keep an independent snapshot for history tracking
            tables.Add((double[,])currentTableau.Clone());
            iteration++;
        }
    }

    #endregion
}
