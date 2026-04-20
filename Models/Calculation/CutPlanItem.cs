namespace CuttingOptimizer.Models.Calculation
{
    public class CutPlanItem
    {
        public int PanelId { get; set; }

        public string Description { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Note { get; set; }

        public double X { get; set; }
        public double Y { get; set; }

        public double FinalLength { get; set; }
        public double FinalWidth { get; set; }

        public double CutLength { get; set; }
        public double CutWidth { get; set; }

        public bool IsRotated { get; set; }

        public int FrontEdgeCode { get; set; }
        public int BackEdgeCode { get; set; }
        public int LeftEdgeCode { get; set; }
        public int RightEdgeCode { get; set; }
    }
}