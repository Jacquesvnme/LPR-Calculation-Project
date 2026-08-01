using LprSolver.Enums;

namespace LprSolver.Models;

/// <summary>
/// This represents a linear programming problem.
/// This class will contain all the required data for the algorithms to solve the linear programming problem.
/// Inherits from GeneralResponse to provide a message and success status for error handling.
/// </summary>
public class LinearProgram : GeneralResponse
{
    public Objective Objective { get; set; } = new();
    public List<Constraint> Constraints { get; set; } = new();
    public Restriction Restriction { get; set; } = new();

    public LinearProgram(
        string message,
        bool isSuccess,
        Objective objective,
        List<Constraint> constraints,
        Restriction restriction
    )
        : base(message, isSuccess)
    {
        Objective = objective;
        Constraints = constraints;
        Restriction = restriction;
    }
}

public class Objective
{
    public OptimizationDirection Direction { get; set; }
    public List<double> Objectives { get; set; } = new();
}

public class Constraint
{
    public List<double> Coefficients { get; set; } = new();
    public ConstraintRelation Relation { get; set; }
    public double RightHandSide { get; set; }
}

public class Restriction
{
    public List<VariableRestriction> Restrictions { get; set; } = new();
}
