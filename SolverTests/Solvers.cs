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
    private LprSolver.Services.SessionInformation _sessionInformation = null!;

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
        _sessionInformation =
            _serviceProvider.GetRequiredService<LprSolver.Services.SessionInformation>();
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
    [Ignore("Not implemented")]
    public async Task PrimalSimplexRevised()
    {
        return;
    }

    [TestMethod]
    [Ignore("Not implemented")]
    public async Task BranchAndBoundRevised()
    {
        return;
    }

    [TestMethod]
    [Ignore("Not implemented")]
    public async Task CuttingPlaneRevised()
    {
        return;
    }

    [TestMethod]
    [Ignore("Not implemented")]
    public async Task NonLinear()
    {
        return;
    }
    #endregion

    #region Solver Method
    private async Task DefaultSolver(SolverAlgorithm solverAlgorithm)
    {
        var (importFilePath, exportFilePath) = await GetAbsoluteFilePaths();
        SessionInformation(solverAlgorithm, importFilePath, exportFilePath);

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
    private void SessionInformation(
        SolverAlgorithm solverAlgorithm,
        string importFilePath,
        string exportFilePath
    )
    {
        // Defaulted session information
        var completedEvents = new List<string>()
        {
            "SolverTests.Solvers.cs executed"
        };

        _sessionInformation.CurrentSession = new()
        {
            SessionId = Guid.NewGuid(),
            ImportFilePath = importFilePath,
            ExportFilePath = exportFilePath,
            SelectedAlgorithm = solverAlgorithm,
            AlgorithmOptions = new List<AlgorithmAnalysisOptions>(),
            CompletedEvents = completedEvents,
        };
    }

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
