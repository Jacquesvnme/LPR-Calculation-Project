namespace LprSolver.Application.SolverUtils;

public static class CuttingPlaneUtils
{
    // Floating-point calculations can produce very tiny negative values.
    // These very small values should just be 0.
    // The tolerance prevents those rounding errors from making the tableau appear infeasible.
    private const double TOLERANCE = 0.0000001;

    // The target fractional part for selecting a cutting row is 0.5.
    // The target closer to this value will be used and selected.
    private const double TARGET_FRACTIONAL = 0.5;

    public static (bool Success, string Message) InitialCuttingPlaneValidation(
        double[,] initialTableau,
        List<string> columnNames
    )
    {
        // Validate inputs
        if (initialTableau == null)
        {
            return (false, "The tableau cannot be null.");
        }

        if (columnNames == null)
        {
            return (false, "The column names cannot be null.");
        }

        // Get column & row counts
        var rowCount = initialTableau.GetLength(0);
        var columnCount = initialTableau.GetLength(1);

        if (rowCount < 2 || columnCount < 2)
        {
            return (
                false,
                "Objective row, constrain missing or RHS column missing."
            );
        }

        var rightHandSideColumn = columnCount - 1;
        if (columnNames.Count != rightHandSideColumn)
        {
            return (
                false,
                "Column name count does not match column count."
            );
        }

        return (true, "No issues.");
    }

    public static (
        bool Success,
        string Message,
        string CuttingEntry,
        int CuttingRow,
        int RightHandSideColumn
    ) DetermineCuttingIndex(double[,] initialTableau, List<string> columnNames)
    {
        // Validation
        var validation = InitialCuttingPlaneValidation(initialTableau, columnNames);
        if (!validation.Success)
        {
            return new(false, validation.Message, string.Empty, -1, -1);
        }

        // Get basic information again
        var rowCount = initialTableau.GetLength(0);
        var columnCount = initialTableau.GetLength(1);
        var rightHandSideColumn = columnCount - 1;

        // Determine the cutting row
        var cuttingRow = -1;
        // Infinite is assigned and then overriden when an actual ratio is found
        var shortestDistance = double.PositiveInfinity;

        // Find the smallest ratio closes to default 0.5
        for (var row = 1; row < rowCount; row++)
        {
            var rightHandSide = initialTableau[row, rightHandSideColumn];

            var fractionalPart = GetFractionalPart(rightHandSide);
            if (fractionalPart == 0)
            {
                continue;
            }

            var distance = Math.Abs(fractionalPart - TARGET_FRACTIONAL);
            if (distance < shortestDistance - TOLERANCE)
            {
                shortestDistance = distance;
                cuttingRow = row;
            }
        }

        if (cuttingRow == -1)
        {
            return (
                true,
                "All RHS values are integral; No cutting is required.",
                string.Empty,
                -1,
                rightHandSideColumn
            );
        }

        // Identify the column for the selected cutting row
        var basicColumn = FindBasicColumn(initialTableau, cuttingRow, rightHandSideColumn);
        if (basicColumn == -1)
        {
            return (
                false,
                $"No basic-variable column could be found for cutting row {cuttingRow}.",
                string.Empty,
                cuttingRow,
                rightHandSideColumn
            );
        }

        return (
            true,
            $"Cutting row {cuttingRow} was selected using {columnNames[basicColumn]}.",
            columnNames[basicColumn],
            cuttingRow,
            rightHandSideColumn
        );
    }

    /// <summary>
    /// Finds the basic-variable column for the given cutting row.
    /// </summary>
    private static int FindBasicColumn(double[,] tableau, int cuttingRow, int rightHandSideColumn)
    {
        for (var column = 0; column < rightHandSideColumn; column++)
        {
            if (!IsApproximately(tableau[cuttingRow, column], 1))
            {
                continue;
            }

            var isBasicColumn = true;
            for (var row = 0; row < tableau.GetLength(0); row++)
            {
                if (row != cuttingRow && !IsApproximately(tableau[row, column], 0))
                {
                    isBasicColumn = false;
                    break;
                }
            }

            if (isBasicColumn)
            {
                return column;
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns the decimal part of the whole number, while treating values close to integers as whole numbers.
    /// </summary>
    private static double GetFractionalPart(double value)
    {
        if (Math.Abs(value - Math.Round(value)) <= TOLERANCE)
        {
            return 0;
        }

        var fractionalPart = value - Math.Floor(value);
        if (fractionalPart <= TOLERANCE || 1 - fractionalPart <= TOLERANCE)
        {
            return 0;
        }
        else
        {
            return fractionalPart;
        }
    }

    /// <summary>
    /// Checks if two values are close enought to be equal while accouting for the floating-point rounding error.
    /// </summary>
    private static bool IsApproximately(double value, double expected)
    {
        return Math.Abs(value - expected) <= TOLERANCE;
    }
}
