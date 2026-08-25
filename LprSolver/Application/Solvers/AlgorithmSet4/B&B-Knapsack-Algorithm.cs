using LprSolver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    public string Name { get; set; }    //name of the item e.g. xi = where i = 1,2,...
    public int Weight { get; set; }     //weight of the item
    public int Value {  get; set; }     //value or profit of the item
    
    //Value-to-Weight ratio
    //Used when calculating the upper bound
    public double Ratio
    {
        get { return (double)Value / Weight; }
    }

    public Item( string name, int weight, int value) //constructure
    {
        Name = name;
        Weight = weight;
        Value = value;
    }
}

// One node of the Branch and Bound Tree
class Node
{
    public string Number { get; set; }//node number e.g. 1.1 , 1.2
    public int Level { get; set; }// Intex of the last item considerd
    public string ItemName { get; set; }// Name of item being condiderd at this node
    public int Weight { get; set; }// Total Weight of items selected so far
    public int Value { get; set; }// Total Value of the items so far
    public double Bound {  get; set; }// Upperbound: Maximum value that could be obtained from this node
    public string Decision { get; set; } //Include / Exclude item

    public Node(
        string number,
        int level, 
        string itemName,
        int weight, 
        int value,
        string decision)
    {
        Number = number;
        Level = level;
        ItemName = itemName;
        Weight = weight;
        Value = value;
        Decision = decision;
    }
}

class Knapsack
{
    private readonly List<Item> items;
    private readonly int capacity;

    // Stores the best solution found so far
    private int bestValue = 0;

    //Stores every node created
    private readonly List<Node> allNodes = new();

    

    public Knapsack(List<Item> items, int capacity)
    {
        // Sort items by value/weight ratio, highest ratio first
        this.items = items
            .OrderByDescending(item => item.Ratio) //sorting
            .ToList; //adding to the list

        this.capacity = capacity; //capacity of knapsack
    }


    private double CalculateBound(Node node)
    {
        // if the current weight already reaches/exceeds capacity, dont add anything else
        if (node.Weight >= capacity)
            return node.Value;

        //Bound starts with value, not weight
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
    public int SolveKnapsack()
    {
        //resets values incase the solver is used more than once
        bestValue = 0;
        allNodes.Clear();

        // keeps track of nodes that needs to be solved in order of smaller remaining capacity
        var queue = new PriorityQueue<Node, int>();

        //root node
        Node root = new Node(
            "",//Root has no number
            -1,// level, no items considerd yet
            0,// weight, knapsack is empty
            0,// value, no value collected yet
            "ROOT"
        );

        // upper bound of the root
        root.Bound = CalculateBound(root);

        //store root
        allNodes.Add(root);

        //Priority queue, smaller remaining capacity processed first
        var queue = new PriorityQueue<Node, int>();

        // remaining capacity left over at the root
        int remainingCapacity = capacity - root.Weight;

        // add root to the priority queue
        queue.Enqueue(root, remainingCapacity);

        /*
         BRANCH AND BOUND
         */
        // continue while there are nodes to be explored
        while (queue.Count > 0)
        {
            //Remove the node with the least capacity remaining
            Node current = queue.Dequeue();

            //If upper-bound is not better, then stop exploring this node
            if (current.Bound <= bestValue)
                continue;

            //move to the next item
            int nextLevel = current.Level + 1;

            //no more items means candidate
            if (nextLevel >= items.Count)
                continue;

            //get next item
            Item item = item[nextLevel];

            /*
                Problem 1: exclude item e.g. xi=0
            */
            string excludeNumber;
            if (current.Number == "") //if it is the root
            {
                excludeNumber = "1";
            }
            else
            {
                excludeNumber = current.Number + ".1";
            }

            Node skip = new Node(
                excludeNumber,
                nextLevel,
                currentItem.Name,
                current.Weight,
                current.Value,
                $"{currentItem.Name} = 0"
            );

            //Calculate the upper-bound for this node
            skip.Bound = CalculateBound(skip);

            //Store node for export
            allNodes.Add(skip);

            //explore branch if it can make a better solution
            if (skip.Bound > bestValue)
            {
                int remaining = capacity - skip.Weight;
                queue.Enqueue(skip, remaining);
            }

            /*
                Problem 2: include item e.g. xi=1
            */

            //get current Item for include
            Item currentItem = items[nextLevel];

            string includeNumber;

            if (current.Number == "")
            {
                includeNumber = "2";
            }
            else
            {
                includeNumber = current.Number + ".2";
            }

            Node take = new Node(
                includeNumber,
                nextLevel, 
                currentItem.Name,
                current.Weight + currentItem.Weight,
                current.Value + currentItem.Value,
                $"{currentItem.Name} = 1"                
            );

            /*
                Check capacity
             */
            //make the item not exceed capacity
            if (take.Weight <= capacity)
            {
                //calculate the upper-bound for this node
                take.Bound = CalculateBound(take);

                //Store node for export
                allNodes.Add(take);

                //if this solution is better than best, update bestValue
                if (take.Value > bestValue)
                {
                    bestValue = take.Value;
                }

                //only explore this node if it has potential to improve the current solution
                if (take.Bound > bestValue)
                {
                    int remaining = capacity - take.Weight;
                    queue.Enqueue(take, remaining);
                }
            }
            else
            {
                //Node is infeasible because its weight exceeds capacity
                take.Bound = 0;

                //Store node for export
                allNodes.Add(take);
            }

        }
        return bestValue;
    }

    //returns all nodes created during the B&B Knapsack process
    public List<Node> GetAllNodes()
    { 
        return allNodes;
    }

    //returns the stored items
    public List<Item> GetItems()
    {
        return items();
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
        var();
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
