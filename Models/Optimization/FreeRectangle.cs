namespace CuttingOptimizer.Models.Optimization;

public class FreeRectangle
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }

    public bool CanFit(double panelLength, double panelWidth)
        => panelLength <= Length && panelWidth <= Width;
}