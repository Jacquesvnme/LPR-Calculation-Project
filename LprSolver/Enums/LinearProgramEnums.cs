namespace LprSolver.Enums;

public enum OptimizationDirection
{
    Maximize,
    Minimize,
}

public enum ConstraintRelation
{
    LessOrEqual,
    Equal,
    GreaterOrEqual,
}

public enum VariableRestriction
{
    NonNegative,
    NonPositive,
    Unrestricted,
    Integer,
    Binary,
}

public enum SolverStatus
{
    Optimal,
    Infeasible,
    Unbounded,
    InvalidModel,
    IterationLimit,
}

public enum SolverAlgorithm
{
    INVALID_OPTION,
    PrimalSimplex,
    Revised_PrimalSimplex,
    BranchAndBound,
    Revised_BranchAndBound,
    CuttingPlane,
    Revised_CuttingPlane,
    BranchAndBoundKnapsack,
    NonLinearProblem,
}
