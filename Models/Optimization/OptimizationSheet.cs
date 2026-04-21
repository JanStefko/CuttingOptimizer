namespace CuttingOptimizer.Models.Optimization;

public class OptimizationSheet
{
    public int SheetNumber { get; set; }
    public double UsableLength { get; set; }
    public double UsableWidth { get; set; }
    public List<FreeRectangle> FreeRectangles { get; set; } = [];
    public List<PanelPlacement> Placements { get; set; } = [];
}