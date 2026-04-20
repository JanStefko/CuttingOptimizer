namespace CuttingOptimizer.Models.Calculation
{
    public class CutPlanSheet
    {
        public int SheetNumber { get; set; }

        public double Length { get; set; }
        public double Width { get; set; }

        public double UsedArea { get; set; }
        public double WasteArea { get; set; }
        public double WastePercentage { get; set; }

        public List<CutPlanItem> Items { get; set; } = [];
    }
}
