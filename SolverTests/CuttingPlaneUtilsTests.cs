using LprSolver.Application.SolverUtils;

namespace SolverTests;

[TestClass]
public sealed class CuttingPlaneUtilsTests
{
    [TestMethod]
    public void AddGomoryFractionalCut_AddsCutVariableAndFractionalConstraint()
    {
        double[,] tableau =
        {
            { 0, 0, 0, 0 },
            { 1, 0, -1.25, 3.75 },
            { 0, 1, 2.25, 2.25 },
        };
        var columnNames = new List<string> { "X1", "X2", "S1" };

        var result = CuttingPlaneUtils.AddGomoryFractionalCut(
            tableau,
            columnNames,
            cuttingRow: 1,
            cutNumber: 1
        );

        Assert.IsTrue(result.Success, result.Message);
        Assert.AreEqual(4, result.Tableau.GetLength(0));
        Assert.AreEqual(5, result.Tableau.GetLength(1));
        CollectionAssert.AreEqual(
            new List<string> { "X1", "X2", "S1", "G1" },
            result.ColumnNames
        );
        Assert.AreEqual(3.75, result.Tableau[1, 4], 0.0000001);
        Assert.AreEqual(-0.75, result.Tableau[3, 2], 0.0000001);
        Assert.AreEqual(1, result.Tableau[3, 3], 0.0000001);
        Assert.AreEqual(-0.75, result.Tableau[3, 4], 0.0000001);
    }

    [TestMethod]
    public void AddGomoryFractionalCut_RejectsAnIntegralCuttingRow()
    {
        double[,] tableau =
        {
            { 0, 0, 0 },
            { 1, 0, 2 },
        };
        var columnNames = new List<string> { "X1", "S1" };

        var result = CuttingPlaneUtils.AddGomoryFractionalCut(
            tableau,
            columnNames,
            cuttingRow: 1,
            cutNumber: 1
        );

        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Message, "integral RHS");
    }
}
