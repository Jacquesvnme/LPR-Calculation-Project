using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using LprSolver.Models;
using Spectre.Console;

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

/*
    Classes for knapsack
 */
// An item in the knapsack
class Item
{
    public string Name { get; set; } //name of the item e.g. xi = where i = 1,2,...
    public double Weight { get; set; } //weight of the item
    public double Value { get; set; } //value or profit of the item

    //Value-to-Weight ratio
    //Used when calculating the upper bound
    public double Ratio
    {
        get { return (double)Value / Weight; }
    }

    public Item(string name, double weight, double value) //constructure
    {
        Name = name;
        Weight = weight; //TODO change to double
        Value = value; //TODO change to double
    }
}

class ItemRanked : Item
{
    public int Rank { get; set; } = 0; //for the ratio test

    public ItemRanked(string name, double weight, double value, int rank)
        : base(name, weight, value) //constructure
    {
        Rank = rank;
    }
}

// One node of the Branch and Bound Tree
class Node
{
    public string Number { get; set; } //node number e.g. 1.1 , 1.2
    public int Level { get; set; } // Intex of the last item considerd
    public string ItemName { get; set; } // Name of item being condiderd at this node
    public double Weight { get; set; } // Total Weight of items selected so far
    public double Value { get; set; } // Total Value of the items so far
    public double Bound { get; set; } // Upperbound: Maximum value that could be obtained from this node
    public string Decision { get; set; } //Include / Exclude item

    public Node(
        string number,
        int level,
        string itemName,
        double weight,
        double value,
        string decision
    )
    {
        Number = number;
        Level = level;
        ItemName = itemName;
        Weight = weight;
        Value = value;
        Decision = decision;
    }
}

public class KnapsackNodeExport
{
    public string Node { get; set; } = "";
    public int Level { get; set; }
    public string Decision { get; set; } = "";
    public double Weight { get; set; }
    public double Value { get; set; }
    public double Bound { get; set; }

    public override string ToString()
    {
        return string.Format(
            "{0,-10}{1,-10}{2,-25}{3,-10:F2}{4,-10:F2}{5,-10:F2}",
            Node,
            Level,
            Decision,
            Weight,
            Value,
            Bound
        );
    }
}

class Knapsack
{
    //create table data for export
    List<object> tables = new List<object>();
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
            .ToList(); //adding to the list

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
        double totalWeight = node.Weight;

        // Start with the next item in the ratio
        int i = node.Level + 1;

        // Add items while they fit in the knapsack
        while (i < items.Count && totalWeight + items[i].Weight >= capacity)
        {
            totalWeight += items[i].Weight;
            bound += items[i].Value;
            i++;
        }

        // if next items remains, take a Fraction of it
        // only for calculating the bound, items remain intergers
        if (i < items.Count)
        {
            double remainingCapacity = capacity - totalWeight;
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
            "", //Root has no number
            -1, // level, no items considerd yet
            "", //Root has no item being considered
            0, // weight, knapsack is empty
            0, // value, no value collected yet
            "ROOT"
        );

        // upper bound of the root
        root.Bound = CalculateBound(root);

        //store root
        allNodes.Add(root);

        //Priority queue, smaller remaining capacity processed first
        queue = new PriorityQueue<Node, int>();

        // remaining capacity left over at the root
        double remainingCapacity = capacity - root.Weight;

        // add root to the priority queue
        queue.Enqueue(root, (int)remainingCapacity);

        /*
         BRANCH AND BOUND KNAPSACK
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
            Item item = items[nextLevel];

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
                item.Name, //current item
                current.Weight,
                current.Value,
                $"{current.ItemName} = 0"
            );

            //Console.WriteLine(skip); //display node skip
            Console.WriteLine($"Problem {skip.Number} {skip.Decision}");
            Console.WriteLine("Weight = " + skip.Weight);
            Console.WriteLine($"Value = {skip.Value}\n");

            //Calculate the upper-bound for this node
            skip.Bound = CalculateBound(skip);

            //Store node for export
            allNodes.Add(skip);

            //explore branch if it can make a better solution
            if (skip.Bound > bestValue)
            {
                double remaining = capacity - skip.Weight;
                queue.Enqueue(skip, (int)remaining);
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
                item.Name,
                current.Weight + currentItem.Weight,
                current.Value + currentItem.Value,
                $"{currentItem.Name} = 1"
            );

            //Console.WriteLine(take); //display node take
            Console.WriteLine($"Problem {take.Number} {take.Decision}");
            Console.WriteLine($"Problem {take.Number} {take.Decision}");
            Console.WriteLine("Weight = " + take.Weight);
            Console.WriteLine($"Value = {take.Value}\n");

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
                    bestValue = (int)take.Value;
                }

                //only explore this node if it has potential to improve the current solution
                if (take.Bound > bestValue)
                {
                    double remaining = capacity - take.Weight;
                    queue.Enqueue(take, (int)remaining);
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

    /*
        Display
     */

    //Display sorted items
    public List<Item> DisplayItems()
    {
        tables.Add("");
        tables.Add("===");
        tables.Add("     RATIO TEST SORTED RESULT"); //TODO delete item .weight .value .ratio
        tables.Add("===");
        tables.Add(
            string.Format("{0,-10}{1,-15}{2,-10}{3,-10}", "Name", "Value / Weight", "Ratio", "Rank")
        );
        tables.Add("---");
        int rank = 1;
        foreach (Item item in items)
        {
            tables.Add(
                string.Format(
                    "{0,-10}{1,-15}{2,-10:F3}{3,-10}",
                    item.Name,
                    $"{item.Weight} / {item.Value} =",
                    item.Ratio,
                    rank++
                )
            );
        }
        return items;
    }

    //Display B&B nodes
    public List<Node> DisplayNodes()
    {
        tables.Add("");
        tables.Add("===");
        tables.Add("     BRANCH AND BOUND KNAPSACK NODES");
        tables.Add("===");
        tables.Add(
            string.Format(
                "{0,-10}{1,-12}{2,-25}{3,-10}{4,-10}{5,-10}",
                "Node",
                "Item",
                "Decision",
                "Weight",
                "Value",
                "Bound"
            )
        );
        tables.Add("---");

        foreach (Node node in allNodes)
        {
            string nodeNumber = string.IsNullOrEmpty(node.Number) ? "Root" : node.Number;

            tables.Add(
                string.Format(
                    "{0,-10} {1,-12} {2,-25} {3,-10} {4,-10} {5,-10:F2}",
                    nodeNumber,
                    node.ItemName,
                    node.Decision,
                    node.Weight,
                    node.Value,
                    node.Bound
                )
            );
        }
        return allNodes;
    }

    //returns all nodes created during the B&B Knapsack process
    public List<Node> GetAllNodes()
    {
        return allNodes;
    }

    //returns the stored items
    public List<Item> GetItems()
    {
        return DisplayItems();
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
    //create table data for export
    List<object> tables = new List<object>();

    public async Task<(bool Success, string Message, ExportReport exportTableData)> Execute(
        LinearProgram linearProgram
    )
    {
        // Add code here to implement the Algorithm.
        // Keep in mind that the return data should match the expected output format for the application.
        Console.WriteLine("===");
        Console.WriteLine("      BRANCH AND BOUND KNAPSACK"); //TODO tables.title
        Console.WriteLine("===");

        int numberOfItems = linearProgram.Constraints.Count();
        while (numberOfItems <= 0) // get new number if not int or neg or zero
        {
            Console.Write("\nNew number of items: ");
            //sets input and checks for int, neg, zero
            if (!int.TryParse(Console.ReadLine(), out numberOfItems))
            {
                Console.WriteLine("Not an interger"); //if user enters anything else than numbers
                numberOfItems = 0;
            }
            else if (numberOfItems <= 0) //if user number is neg or equal to zero
            {
                Console.WriteLine("Number of items cannot be negative or zero.");
            }
        }
        //Console.WriteLine("Number of items: " + numberOfItems);
        tables.Add($"Number of Items: {numberOfItems}"); //add to export

        //Ask for knapsack capacity
        if (
            !int.TryParse(
                linearProgram.Constraints.First().RightHandSide.ToString(),
                out int capacity
            )
        )
        {
            capacity = 0;
        }

        while (capacity <= 0) // get new number if not int or neg or zero
        {
            Console.Write("\nNew capacity: ");
            //sets input and checks for int, neg, zero
            if (!int.TryParse(Console.ReadLine(), out capacity))
            {
                Console.WriteLine("Not an interger"); //if user enters anything else than numbers
                capacity = 0;
            }
            else if (capacity <= 0) //if user number is neg or equal to zero
            {
                Console.WriteLine("Number of items cannot be negative or zero.");
            }
        }
        //Console.WriteLine("Capacity: " + capacity);
        tables.Add($"Capacity: {capacity}");

        tables.Add("===");
        tables.Add("     ORIGINAL ITEMS");
        tables.Add("===");
        tables.Add(
            string.Format("{0,-10}{1,-10}{2,-10}{3,-15}", "Name", "Weight", "Value", "Ratio")
        );
        tables.Add("---");

        //Create item list
        List<Item> items = new List<Item>();
        //get objectives
        int[] valueArray = linearProgram
            .Objective.Objectives.Select(value => Convert.ToInt32(value))
            .ToArray();
        //get constraints
        int[] weightArray = linearProgram
            .Constraints.First()
            .Coefficients.Select(value => Convert.ToInt32(value))
            .ToArray();

        //all numbers must be int and non neg   //TODO allow non integers
        for (int i = 0; i < valueArray.Length; i++)
        {
            while (valueArray[i] < 0) //numbers in array non neg and int
            {
                Console.Write($"Value at x{i} is negative. Enter a new value: ");

                if (int.TryParse(Console.ReadLine(), out int newValue) && newValue >= 0)
                {
                    valueArray[i] = newValue;
                }
                else
                {
                    Console.WriteLine("Invalid input.Please enter a non-negative integer.");
                }
            }

            while (weightArray[i] < 0) //numbers in array non neg and int
            {
                Console.Write($"Weight at x{i} is negative. Enter a new value: ");

                if (int.TryParse(Console.ReadLine(), out int newWeight) && newWeight >= 0)
                {
                    weightArray[i] = newWeight;
                }
                else
                {
                    Console.WriteLine("Invalid input.Please enter a non-negative integer.");
                }
            }
        }

        for (int i = 0; i < numberOfItems; i++)
        {
            string itemName = $"x{i + 1}";
            int itemValue = valueArray[i];
            int itemWeight = weightArray[i];

            //Add item to list
            items.Add(new Item(itemName, itemWeight, itemValue)); //TODO change to Item( string, double, double)
        }

        //display list
        foreach (Item item in items)
        {
            tables.Add(
                string.Format(
                    "{0,-10}{1,-10}{2,-10}{3,-10:F6}", //round to 6 decimal
                    item.Name,
                    item.Weight,
                    item.Value,
                    item.Ratio
                )
            );
        }

        //Create solver
        Knapsack solver = new Knapsack(items, capacity);

        //Display sorted items
        items = solver.DisplayItems();

        //solve
        int bestValue = solver.SolveKnapsack();

        //display branch and bound knapsack tree
        List<Node> nodes = solver.GetAllNodes();

        nodes = solver.DisplayNodes();

        //node headers
        tables.Add(
            string.Format(
                "{0,-10}{1,-10}{2,-25}{3,-10}{4,-10}{5,-10}",
                "Node",
                "Level",
                "Decision",
                "Weight",
                "Value",
                "Bound"
            )
        );

        //add each B&B node to the table
        foreach (Node node in nodes)
        {
            tables.Add(
                new KnapsackNodeExport
                {
                    Node = string.IsNullOrEmpty(node.Number) ? "ROOT" : node.Number,
                    Level = node.Level,
                    Decision = node.Decision,
                    Weight = node.Weight,
                    Value = node.Value,
                    Bound = Math.Round(node.Bound, 2),
                }
            );
        }

        //show values in export
        //tables.Add

        //Display

        var exportReport = ExportData(tables, nodes.Count, bestValue);
        //TODO display best canidate xi and z

        //return result
        return new(true, $"Branch and Bound Knapsack completed.", exportReport);
    }

    public ExportReport ExportData(List<object> tables, int nodeCount, int bestValue)
    {
        List<string> importantDetails = new();
        List<string> additionalData = new();
        List<string> sensitivityAnalysis = new();

        importantDetails.Add(
            $"Maximum value = {bestValue}. Node generated = {nodeCount}" // TODO best CANDIDATE found TODO implement CANDITATEs
        );

        additionalData.Add("Add new entries that will be exported");
        sensitivityAnalysis.Add("Add new entries that will be exported");

        var exportReport = new ExportReport
        {
            AdditionalData = new AdditionalData()
            {
                Title = "Additional Data Title",
                Rows = additionalData,
            },
            ImportantDetails = new ImportantDetails()
            {
                Title = "Important Details Title",
                Rows = importantDetails,
            },
            SensitivityAnalysis = new SensitivityAnalysis()
            {
                Title = "Sensitivity Analysis Title",
                Rows = sensitivityAnalysis,
            },
            Tables = new ExportTable { Tables = tables, Title = "Branch and Bound Knapsack" },
        };

        return exportReport;
    }
}
