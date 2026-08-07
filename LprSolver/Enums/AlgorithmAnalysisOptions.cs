namespace LprSolver.Enums;

public enum AlgorithmAnalysisOptions
{
    INVALID_OPTION,

    DisplayNonBasicVariableRange,
    ApplyNonBasicVariableChange,

    DisplayBasicVariableRange,
    ApplyBasicVariableChange,

    DisplayConstraintRightHandSideRange,
    ApplyConstraintRightHandSideChange,

    DisplayNonBasicColumnVariableRange,
    ApplyNonBasicColumnVariableChange,

    AddActivityToOptimalSolution,
    AddConstraintToOptimalSolution,

    DisplayShadowPrices,

    ApplyDuality,
    SolveDualProgrammingModel,
    VerifyStrongOrWeakDuality,
}
