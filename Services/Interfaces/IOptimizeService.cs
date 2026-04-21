using CuttingOptimizer.Models.DTOs;

namespace CuttingOptimizer.Services.Interfaces;

public interface IOptimizeService
{
    Task<OptimizeCutPlanResponse> OptimizeAsync(OptimizeCutPlanRequest request);
}