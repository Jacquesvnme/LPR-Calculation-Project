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
}
