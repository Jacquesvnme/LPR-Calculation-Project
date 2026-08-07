using LprSolver.Models;

namespace LprSolver.Services;

public interface IExporter
{
    Task<(bool IsSuccess, string Message)> ExportDataToTextFile();
}

public class Exporter : IExporter
{
    public async Task<(bool IsSuccess, string Message)> ExportDataToTextFile()
    {
        return (true, "Data exported successfully.");
    }
}
