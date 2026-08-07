using System.Collections;
using System.Text;
using LprSolver.Models;

namespace LprSolver.Services;

public interface IExporter
{
    Task<(bool IsSuccess, string Message)> ExportDataToTextFile(ExportReport exportReport);
    Task<(bool IsSuccess, string Message, string Text)> ExportTablesToText(List<object> tables);
}

public class Exporter : IExporter
{
    public async Task<(bool IsSuccess, string Message)> ExportDataToTextFile(
        ExportReport exportReport
    )
    {
        return (true, "Data exported successfully.");
    }
}
