using LprSolver.Application.SolverUtils;

namespace SolverTests;

[TestClass]
public sealed class DualSimplexUtilsTests
{
    [TestMethod]
    public void FindPivotRow_SelectsTheMostNegativeRightHandSide()
    {
        double[,] tableau =
        {
            { 1, 2, 0, 0 },
            { -1, -1, 1, -0.5 },
            { -1, -2, 0, -1.5 },
        };

        var pivotRow = DualSimplexUtils.FindPivotRow(tableau);

        Assert.AreEqual(2, pivotRow);
    }

    [TestMethod]
    public void FindPivotColumn_UsesTheSmallestDualRatio()
    {
        double[,] tableau =
        {
            { 4, 1, 0, 0 },
            { -2, -1, 1, -0.5 },
        };

        var pivotColumn = DualSimplexUtils.FindPivotColumn(tableau, pivotRow: 1);

        Assert.AreEqual(1, pivotColumn);
    }

    [TestMethod]
    public void Solve_RestoresPrimalFeasibilityAndRecordsThePivot()
    {
        double[,] tableau =
        {
            { 1, 2, 0, 0 },
            { 1, 0, 1, 3 },
            { -1, -2, 0, -1 },
        };

        var result = DualSimplexUtils.Solve(tableau);

        Assert.IsTrue(result.Success, result.Message);
        CollectionAssert.AreEqual(new List<int> { 0 }, result.PivotColumns);
        CollectionAssert.AreEqual(new List<int> { 2 }, result.PivotRows);
        Assert.HasCount(1, result.Tables);
        Assert.IsFalse(PrimalSimplexUtils.HasNegativeRightHandSide(result.Tableau));
    }

    [TestMethod]
    public void Solve_ReturnsFailureWhenNoEnteringColumnExists()
    {
        double[,] tableau =
        {
            { 1, 2, 0 },
            { 1, 1, -1 },
        };

        var result = DualSimplexUtils.Solve(tableau);

        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Message, "No entering column");
    }
}
