namespace LprSolver.Application.SolverUtils;

public static class PrimalSimplexUtils {
    public static List<double> NormalizeObjective(List<double> values)
    {
        if (values == null || values.Count == 0)
        {
            throw new ArgumentException("Values cannot be null or empty.", nameof(values));
        }

        return values.Select(value => -value).ToList();
    }
}
