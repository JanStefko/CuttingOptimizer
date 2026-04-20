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

        public double Length { get; set; }
        public double Width { get; set; }

        public bool IsRotated { get; set; }

        public int? FrontEdgeId { get; set; }
        public int? BackEdgeId { get; set; }
        public int? LeftEdgeId { get; set; }
        public int? RightEdgeId { get; set; }
    }
}
