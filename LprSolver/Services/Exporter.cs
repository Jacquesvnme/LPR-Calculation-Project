using System.Collections;
using System.Linq;
using System.Text;
using LprSolver.Extensions;
using LprSolver.Models;

namespace LprSolver.Services;

public interface IExporter
{
    Task<(bool IsSuccess, string Message)> ExportDataToTextFile(
        ExportReport exportReport,
        string exportFilePath
    );
}

public class Exporter : IExporter
{
    private readonly SessionInformation _session;

    public Exporter(SessionInformation session)
    {
        _session = session;
    }

    public async Task<(bool IsSuccess, string Message)> ExportDataToTextFile(
        ExportReport exportReport,
        string exportFilePath
    )
    {
        var validation = await ValidateExportData(exportReport);
        if (!validation.IsSuccess)
        {
            return (false, "Data validation failed.");
        }

        try
        {
            var sessionDetails = await _session.GetCurrentSession();
            var sessionInformation = new StringBuilder()
                .AppendLine($"Session Id: {sessionDetails.SessionId}")
                .AppendLine($"Import File Path: {sessionDetails.ImportFilePath}")
                .AppendLine($"Export File Path: {sessionDetails.ExportFilePath}")
                .AppendLine($"Algorithm: {sessionDetails.SelectedAlgorithm.ToString()}")
                .AppendLine("Algorithm Options")
                .AppendEnums(sessionDetails.AlgorithmOptions.Cast<Enum>().ToList(), "> ")
                .AppendLine("Completed Events")
                .AppendListLines(sessionDetails.CompletedEvents)
                .ToString();

            var text = new StringBuilder()
                .AppendTitle("Session Information")
                .AppendLine(sessionInformation)
                .AppendTitle(exportReport.ImportantDetails.Title)
                .AppendListLines(exportReport.ImportantDetails.Rows, "> ")
                .AppendTitle(exportReport.Tables.Title)
                .Append(ExportDataTables(exportReport.Tables.Tables))
                .AppendTitle(exportReport.SensitivityAnalysis.Title)
                .AppendListLines(exportReport.SensitivityAnalysis.Rows, "> ")
                .AppendTitle(exportReport.AdditionalData.Title)
                .AppendListLines(exportReport.AdditionalData.Rows, "> ");

            var saveResults = await SaveToTextFile(text.ToString(), exportFilePath);
            if (!saveResults.IsSuccess)
            {
                return new(false, "Exporting failed");
            }

            return new(true, "Exported successfully");
        }
        catch
        {
            return (false, "Data exported unsuccessfully.");
        }
    }

    private static string ExportDataTables(object? value)
    {
        var text = new StringBuilder();

        switch (value)
        {
            case null:
                break;

            case string stringValue:
                text.AppendLine(stringValue);
                break;

            case ExportTable exportTable:
                if (!string.IsNullOrWhiteSpace(exportTable.Title))
                {
                    text.AppendLine(exportTable.Title);
                }

                text.Append(ExportDataTables(exportTable.Tables));
                break;

            case IEnumerable<IEnumerable<string>> tableRows:
                text.Append(AppendTable(tableRows));
                break;

            case IEnumerable collection:
                foreach (var item in collection)
                {
                    text.Append(ExportDataTables(item));
                }

                break;

            default:
                text.AppendLine(value.ToString());
                break;
        }

        return text.ToString();
    }

    private static string AppendTable(IEnumerable<IEnumerable<string>> tableRows)
    {
        var text = new StringBuilder();
        var rows = tableRows.Select(row => row.ToList()).ToList();
        if (rows.Count == 0)
        {
            return string.Empty;
        }

        var columnCount = rows.Max(row => row.Count);
        var columnWidths = Enumerable
            .Range(0, columnCount)
            .Select(column => rows.Max(row => column < row.Count ? row[column].Length : 0))
            .ToList();

        foreach (var row in rows)
        {
            for (var column = 0; column < columnCount; column++)
            {
                var cell = column < row.Count ? row[column] : string.Empty;
                text.Append(cell.PadRight(columnWidths[column]));

                if (column < columnCount - 1)
                {
                    text.Append(" | ");
                }
            }

            text.AppendLine();
        }

        return text.ToString();
    }

    public async Task<(bool IsSuccess, string Message)> SaveToTextFile(
        string exportText,
        string exportFilePath
    )
    {
        if (string.IsNullOrWhiteSpace(exportFilePath))
        {
            return (false, "An export file path is required.");
        }

        if (exportText == null)
        {
            return (false, "There is no text to export.");
        }

        try
        {
            string fullPath = Path.GetFullPath(exportFilePath);
            string? directoryPath = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await File.WriteAllTextAsync(fullPath, exportText);

            return (true, "Saved successfully.");
        }
        catch (Exception exception)
        {
            return (false, $"The file could not be saved: {exception.Message}");
        }
    }

    /// <summary>
    /// Validates the data so that we know there is data to save
    /// </summary>
    /// <param name="exportReport"></param>
    /// <returns></returns>
    public async Task<(bool IsSuccess, string Message)> ValidateExportData(
        ExportReport exportReport
    )
    {
        if (exportReport == null)
        {
            return new(false, "Data is empty");
        }

        if (
            exportReport.ImportantDetails == null
            || exportReport.SensitivityAnalysis == null
            || exportReport.Tables == null
            || exportReport.AdditionalData == null
        )
        {
            return new(false, "Data is empty or missing");
        }

        if (
            exportReport.ImportantDetails.Rows.Count() < 0
            || exportReport.SensitivityAnalysis.Rows.Count() < 0
            || exportReport.AdditionalData.Rows.Count() < 0
            || exportReport.Tables.Tables.Count < 0
        )
        {
            return new(false, "Data contains no rows");
        }

        return new(true, "Data is acccetable");
    }
}
