using System.Collections;
using LprSolver.Models;
using Spectre.Console;

namespace LprSolver.Application.SolverUtils;

public static class ConsolePrinter
{
    /// <summary>
    /// Generic console exporter setup for Primal Simplex and Cutting Plane algorithms.
    /// </summary>
    /// <param name="exportTableData"></param>
    public static void PrintOutputData(ExportReport exportTableData)
    {
        PrintSection(
            exportTableData.ImportantDetails.Title,
            exportTableData.ImportantDetails.Rows
        );
        PrintTableSection(exportTableData.Tables);
        PrintSection(
            exportTableData.SensitivityAnalysis.Title,
            exportTableData.SensitivityAnalysis.Rows
        );
        PrintSection(
            exportTableData.AdditionalData.Title,
            exportTableData.AdditionalData.Rows
        );
    }

    private static void PrintSection(string title, List<string> rows)
    {
        PrintTitle(title);

        foreach (var row in rows)
        {
            AnsiConsole.MarkupLine($"[blue]>[/] {Markup.Escape(row)}");
        }

        AnsiConsole.WriteLine();
    }

    private static void PrintTableSection(ExportTable exportTable)
    {
        PrintTitle(exportTable.Title);

        foreach (var tableContent in exportTable.Tables)
        {
            if (tableContent is List<List<string>> table)
            {
                foreach (var row in table)
                {
                    AnsiConsole.WriteLine(string.Join(" | ", row));
                }
            }

            AnsiConsole.WriteLine();
        }

        AnsiConsole.WriteLine();
    }

    private static void PrintTitle(string title)
    {
        var displayTitle = string.IsNullOrWhiteSpace(title) ? "Untitled Section" : title;
        AnsiConsole.Write(
            new Rule($"[bold cornflowerblue]{Markup.Escape(displayTitle)}[/]").LeftJustified()
        );
    }
}
