namespace CuttingOptimizer.Models.Optimization;

public class OptimizationResult
{
    public List<OptimizationSheet> Sheets { get; set; } = [];
    public List<OptimizationPanel> UnplacedPanels { get; set; } = [];

    public double TotalUsedArea =>
        Sheets.SelectMany(x => x.Placements).Sum(x => x.Length * x.Width);
}