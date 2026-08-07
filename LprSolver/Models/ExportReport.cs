namespace LprSolver.Models;

public class ExportReport
{
    public List<string> ImportantDetails { get; set; } = new();
    public List<Object> Tables { get; set; } = new();
    public List<string> SensitivityAnalysis { get; set; } = new();
    public List<string> AdditionalData { get; set; } = new();
}

public class ExportTable
{
    public string Title { get; set; } = string.Empty;
    public List<List<string>> Rows { get; set; } = new();
}
