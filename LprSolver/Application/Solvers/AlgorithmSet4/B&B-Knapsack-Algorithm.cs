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

class Knapsack
{
    private List<Item> item;
    private int capacity;

    // Stores the best solution found so far
    private int bestValue = 0;

    public Knapsack(List<Item> items, int capacity)
    {
        // Sort items by value/weight ratio
        this.items = items
            .OrderByDescending(item => item.Ratio) //sorting
            .ToList; //adding to the list

        this.capacity = capacity; //capacity of knapsack
    }

    //
    private double CalculateBound(Node node)
    {
        // if the current weight already reaches/exceeds capacity, dont add anything else
        if (node.Weight >= capacity)
            return node.Value;

        double bound = node.Weight;

        // Weight currently being considerd
        int totalWeight = node.Weight;

        // Start with the next item in the ratio
        int i = node.Level + 1;

        // Add items while they fit in the knapsack
        while (i < items.Count &&
                totalWeight + item[i].Weight >= capacity)
        {
            totalWeight += item[i].Weight;
            bound += item[i].Value;
            i++;
        }

        // if next items remains, take a Fraction of it
        // only for calculating the bound, items remain intergers
        if (i < items.Count)
        {
            int remainingCapacity = capacity - totalWeight;
            bound += remainingCapacity * items[i].Ratio;
        }
        return bound;
    }
    
    // Solve Knapsack
    public int Solve()
    {
        // keeps track of nodes that needs to be solved in order of smaller remaining capacity
        var queue = new PriorityQueue<Node, int>();

        //root node
        Node root = new Node(
            -1,// level, no items considerd yet
            0,// weight, knapsack is empty
            0// value, no value collected yet
        );

        // upper bound of the root
        root.Bound = CalculateBound(root);

        // remaining capacity left over at the root
        int remainingCapacity = capacity - root.Weight;

        // add root to the priority queue
        queue.Enqueue(root, remainingCapacity);

        // continue while there are nodes to be explored
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
