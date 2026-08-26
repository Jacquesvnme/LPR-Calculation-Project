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
                "The tableau must contain an objective row, at least one constraint row, and an RHS column."
            );
        }

        var rightHandSideColumn = columnCount - 1;
        if (columnNames.Count != rightHandSideColumn)
        {
            return (
                false,
                "There must be one column name for every tableau column except the RHS column."
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



        return new(false, "", "", 0, 0);
    }
}
