namespace LprSolver.Models;

/// <summary>
/// Represents a response that includes a file path, inheriting the GeneralResponse class.
/// </summary>
public class ImporterFilePathResponse : GeneralResponse
{
    public string FilePath { get; set; } = string.Empty;

    public ImporterFilePathResponse(string message, bool isSuccess, string filePath)
        : base(message, isSuccess)
    {
        FilePath = filePath;
    }
}
