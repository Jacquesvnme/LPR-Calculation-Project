using LprSolver.Models;
using System;
using Sysyem.Collections.Generic;
using System.Linq;

namespace LprSolver.Application.Solvers.AlgorithmSet4;

/// <summary>
/// Interface for the Algorithm.
/// </summary>
public interface IB_B_Knapsack_Algorithm
{
    Task<(bool Success, string Message, ExportReport exportTableData)> Execute(
        LinearProgram linearProgram
    );
}

/// Classes for knapsack

// An item in the knapsack
class Item
{
    public int Weight { get; set; }
    public int Value {  get; set; }
    
    //Value to Weight ratio
    //Used when calculating the upper bound
    public double Ratio
    {
        get { return (double)Value / Weight; }
    }

    public Item( int weight, int value) //constructure
    {
        Weight = weight;
        Value = value;
    }
}

// One node of the Branch and Bound Tree
class Node
{
    // Intex of the last item considerd
    public int Level { get; set; }

    // Total Weight of items selected so far
    public int Weight { get; set; }

    // Total Value of the items so far
    public int Value { get; set; }

    // Upperbound: Maximum value that could be obtained from this node
    public double Bound {  get; set; }

    public Node(int level, int weight, int value)
    {
        Level = level;
        Weight = weight;
        Value = value;
    }
}

public class B_B_Knapsack_Algorithm : IB_B_Knapsack_Algorithm
{
    /// <summary>
    /// Class constructor for the Application class.
    /// </summary>
    public B_B_Knapsack_Algorithm()
    {
        // Dependency injection if required can be added here.
    }

    /// <summary>
    /// Main method to execute the Algorithm.
    /// </summary>
    public async Task<(bool Success, string Message, ExportReport exportTableData)> Execute(
        LinearProgram linearProgram
    )
    {
        
        // Add code here to implement the Algorithm.
        // Keep in mind that the return data should match the expected output format for the application.

        // Call your own custom methods inside this class but make them private to avoid exposing them outside of this class.
        OtherMethods();

        var tables = new List<object>();

        var exportReport = new ExportReport
        {
            AdditionalData = new AdditionalData(),
            ImportantDetails = new ImportantDetails(),
            SensitivityAnalysis = new SensitivityAnalysis(),
            Tables = new ExportTable { Tables = tables },
        };

        return new(
            true,
            "Dummy branch and bound knapsack table created successfully.",
            exportReport
        );
    }

    private void OtherMethods()
    {
        //dummy method
    }
}
