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

    /// <summary>
    /// Imports the default example model from the configured location and runs it
    /// through the primal simplex solver without using the interactive menu.
    /// </summary>
    [TestMethod]
    public async Task PrimalSimplex()
    {
        var importFilePath = _configuration["ImportLocation"];
        var exportFilePath = _configuration["ExportLocation"];
        Assert.IsFalse(string.IsNullOrWhiteSpace(importFilePath));
        Assert.IsFalse(string.IsNullOrWhiteSpace(exportFilePath));

        var inputPath = Path.GetFullPath(importFilePath, AppContext.BaseDirectory);
        var importResult = await _importer.ImportDataFromTextFile(inputPath);

        Assert.IsTrue(importResult.IsSuccess, importResult.Message);
        Assert.IsNotNull(importResult.LinearProgram);

        var solverResult = await _solver.StartSolver(
            SolverAlgorithm.PrimalSimplex,
            importResult.LinearProgram
        );

        Assert.IsTrue(solverResult.IsSuccess, solverResult.Message);

        var exportResult = await _exporter.ExportDataToTextFile(
            solverResult.exportReport,
            exportFilePath
        );

        Assert.IsTrue(solverResult.IsSuccess, solverResult.Message);
    }

    /// <summary>
    /// Imports the default example model from the configured location and runs it
    /// through the primal simplex solver without using the interactive menu.
    /// </summary>
    [TestMethod]
    public async Task PrimalSimplexRevised()
    {
        throw new NotImplementedException("Still need to implement this");
    }
}
