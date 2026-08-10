namespace LprSolver.Models;

public class ExportReport
{
    public ImportantDetails ImportantDetails { get; set; } = new();
    public ExportTable Tables { get; set; } = new();
    public SensitivityAnalysis SensitivityAnalysis { get; set; } = new();
    public AdditionalData AdditionalData { get; set; } = new();
}

public class ExportTable
{
    public string Title { get; set; } = "Table Information";
    public List<Object> Tables { get; set; } = new();
}

public class ImportantDetails
{
    public string Title { get; set; } = "Important Details";
    public List<string> Rows { get; set; } = new();
}

public class SensitivityAnalysis
{
    public string Title { get; set; } = "Sensitivity Analysis";
    public List<string> Rows { get; set; } = new();
}

public class AdditionalData
{
    public string Title { get; set; } = "Additional Information";
    public List<string> Rows { get; set; } = new();
}
