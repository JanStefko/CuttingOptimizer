using CuttingOptimizer.Models.Calculation;

namespace CuttingOptimizer.Models.DTOs
{
    public class OptimizeCutPlanResponse
    {
        public CutPlan CutPlan { get; set; } = new();
    }
}
