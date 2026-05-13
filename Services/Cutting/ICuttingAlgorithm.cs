using CuttingOptimizer.Models.Optimization;

namespace CuttingOptimizer.Services.Cutting;

public interface ICuttingAlgorithm
{
    OptimizationResult Cut(
        double usableLength,
        double usableWidth,
        double kerf,
        List<OptimizationPanel> panels);
}