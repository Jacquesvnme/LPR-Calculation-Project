namespace LprSolver.Application.SolverUtils;

public static class DualSimplexUtils
{
    // Floating-point calculations can produce very tiny negative values.
    // These very small values should just be 0.
    // The tolerance prevents those rounding errors from making the tableau appear infeasible.
    private const double TOLERANCE = 0.0000001;
    private const int MAXIMUM_ITERATIONS = 1000;
}
