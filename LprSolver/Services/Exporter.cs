namespace LprSolver.Services;

public interface IExporter
{
    Task ExportDataToTextFile(string data);
}

public class Exporter : IExporter
{
    public async Task ExportDataToTextFile(string data)
    {
        // TODO: Implement the logic to export data to a text file

        Console.WriteLine($"Exporting data: {data}");
    }
}