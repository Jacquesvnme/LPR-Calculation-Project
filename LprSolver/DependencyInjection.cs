using LprSolver.Application.Solvers.AlgorithmSet1;
using LprSolver.Application.Solvers.AlgorithmSet2;
using LprSolver.Application.Solvers.AlgorithmSet3;
using LprSolver.Application.Solvers.AlgorithmSet4;
using LprSolver.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LprSolver;

public static class DependencyInjection
{
    public static IServiceCollection AddConfigureServices(this IServiceCollection services)
    {
        // Register your services here

        // Configuration setup
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Main application logic services
        services.AddScoped<IMenu, Menu>();
        services.AddScoped<IImporter, Importer>();
        services.AddScoped<ISolverSelection, SolverSelection>();
        services.AddScoped<IExporter, Exporter>();

        // Injections for the solvers solutions
        services.AddScoped<IPrimal_Simplex_Algorithm, Primal_Simplex_Algorithm>();
        services.AddScoped<IRevised_Primal_Simplex_Algorithm, Revised_Primal_Simplex_Algorithm>();
        services.AddScoped<IB_B_Simplex_Algorithm, B_B_Simplex_Algorithm>();
        services.AddScoped<IRevised_B_B_Simplex_Algorithm, Revised_B_B_Simplex_Algorithm>();
        services.AddScoped<ICutting_Plane_Algorithm, Cutting_Plane_Algorithm>();
        services.AddScoped<IRevised_Cutting_Plane_Algorithm, Revised_Cutting_Plane_Algorithm>();
        services.AddScoped<IB_B_Knapsack_Algorithm, B_B_Knapsack_Algorithm>();
        services.AddScoped<INonLinearProblem, NonLinearProblem>();

        return services;
    }
}
