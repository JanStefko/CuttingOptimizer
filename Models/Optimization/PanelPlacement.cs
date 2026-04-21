namespace CuttingOptimizer.Models.Optimization;

public class PanelPlacement
{
    public int SheetNumber { get; set; }
    public int PanelIndex { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public bool IsRotated { get; set; }
    public OptimizationPanel Panel { get; set; } = null!;
}