using LprSolver.Enums;

namespace LprSolver.Models;

public class SessionDetails
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public string ImportFilePath { get; set; } = string.Empty;
    public string ExportFilePath { get; set; } = string.Empty;
    public SolverAlgorithm SelectedAlgorithm { get; set; }
    public List<AlgorithmAnalysisOptions> AlgorithmOptions { get; set; } = new();
    public List<string> CompletedEvents { get; set; } = new();
}
