using LprSolver.Enums;
using LprSolver.Models;

namespace LprSolver.Services;

public class SessionInformation
{
    public SessionDetails CurrentSession { get; set; } = new();

    public async Task StartSession()
    {
        CurrentSession = new SessionDetails();
    }

    public async Task UpdateFilePath(MenuType menuType, string filePath)
    {
        switch (menuType)
        {
            case MenuType.Exporter:
                CurrentSession.ExportFilePath = filePath;
                break;
            case MenuType.Importer:
                CurrentSession.ImportFilePath = filePath;
                break;
        }
        return;
    }

    public async Task UpdateAlgorithmType(SolverAlgorithm solverAlgorithm)
    {
        CurrentSession.SelectedAlgorithm = solverAlgorithm;
        return;
    }

    public async Task AddSelectedOptions(List<AlgorithmAnalysisOptions> algorithmOptions)
    {
        CurrentSession.AlgorithmOptions.AddRange(algorithmOptions);
        return;
    }

    public async Task AddCompletedEvent(string CompletedEvent)
    {
        CurrentSession.CompletedEvents.Add(CompletedEvent);
        return;
    }

    public async Task<SessionDetails> GetCurrentSession()
    {
        return CurrentSession;
    }
}
