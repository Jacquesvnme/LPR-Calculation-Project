using LprSolver;
using LprSolver.Enums;
using LprSolver.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SolverTests;

[TestClass]
public sealed class Solvers
{
    private ServiceProvider _serviceProvider = null!;
    private IConfiguration _configuration = null!;
    private IImporter _importer = null!;
    private ISolverSelection _solver = null!;
    private IExporter _exporter = null!;

    #region Required Services & Methods
    /// <summary>
    /// Runs before each test method to create the dependency-injection container
    /// and resolve the services required by the test.
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _serviceProvider = new ServiceCollection()
            .AddConfigureServices()
            .BuildServiceProvider();

        _configuration = _serviceProvider.GetRequiredService<IConfiguration>();
        _importer = _serviceProvider.GetRequiredService<IImporter>();
        _solver = _serviceProvider.GetRequiredService<ISolverSelection>();
        _exporter = _serviceProvider.GetRequiredService<IExporter>();
    }

    /// <summary>
    /// Runs after each test method to dispose the dependency-injection container
    /// and release any disposable services it created.
    /// </summary>
    [TestCleanup]
    public void CleanUp() => _serviceProvider.Dispose();
    #endregion

    /// <summary>
    /// Imports the default example model from the configured location and runs it
    /// through the specified solver without using the interactive menu's.
    /// </summary>
    #region Test Methods
    [TestMethod]
    public async Task PrimalSimplex()
    {
        await DefaultSolver(SolverAlgorithm.PrimalSimplex);
    }

    [TestMethod]
    public async Task CuttingPlane()
    {
        await DefaultSolver(SolverAlgorithm.CuttingPlane);
    }

    [TestMethod]
    public async Task BranchAndBound()
    {
        await DefaultSolver(SolverAlgorithm.BranchAndBound);
    }

    [TestMethod]
    public async Task BranchAndBoundKnapsack()
    {
        await DefaultSolver(SolverAlgorithm.BranchAndBoundKnapsack);
    }
    #endregion

    /// <summary>
    /// Unimplemented methods signifying that the below algorithms have not beed developed yet.
    /// </summary>
    #region Unimplemented Test Methods
    [TestMethod]
    public async Task PrimalSimplexRevised()
    {
        throw new NotImplementedException("Not implemented");
    }

    [TestMethod]
    public async Task BranchAndBoundRevised()
    {
        throw new NotImplementedException("Not implemented");
    }

    [TestMethod]
    public async Task CuttingPlaneRevised()
    {
        throw new NotImplementedException("Not implemented");
    }

    [TestMethod]
    public async Task NonLinear()
    {
        throw new NotImplementedException("Not implemented");
    }
    #endregion

    #region Solver Method
    private async Task DefaultSolver(SolverAlgorithm solverAlgorithm)
    {
        var (importFilePath, exportFilePath) = await GetAbsoluteFilePaths();
        var importResult = await _importer.ImportDataFromTextFile(importFilePath);

        Assert.IsTrue(importResult.IsSuccess, importResult.Message);
        Assert.IsNotNull(importResult.LinearProgram);

        var solverResult = await _solver.StartSolver(
            solverAlgorithm,
            importResult.LinearProgram
        );

        Assert.IsTrue(solverResult.IsSuccess, solverResult.Message);

        var exportResult = await _exporter.ExportDataToTextFile(
            solverResult.exportReport,
            exportFilePath
        );

        Assert.IsTrue(exportResult.IsSuccess, exportResult.Message);
    }
    #endregion

    #region Utility Methods
    private async Task<(string ImportFilePath, string ExportFilePath)> GetAbsoluteFilePaths()
    {
        var importFilePath = _configuration["ImportLocation"];
        var exportFilePath = _configuration["ExportLocation"];
        Assert.IsFalse(string.IsNullOrWhiteSpace(importFilePath));
        Assert.IsFalse(string.IsNullOrWhiteSpace(exportFilePath));

        var inputPath = Path.GetFullPath(importFilePath, AppContext.BaseDirectory);
        var exportPath = Path.GetFullPath(exportFilePath, AppContext.BaseDirectory);

        return (inputPath, exportPath);
    }
    #endregion
}
