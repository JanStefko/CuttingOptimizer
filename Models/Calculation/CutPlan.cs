namespace CuttingOptimizer.Models.Calculation
{
    public class CutPlan
    {
        public int SheetMaterialId { get; set; }

        public double SheetLength { get; set; }
        public double SheetWidth { get; set; }

        public double Kerf { get; set; }
        public double TrimMargin { get; set; }

        public int TotalPanelsCount { get; set; }
        public int UsedSheetsCount { get; set; }

        public double TotalUsedArea { get; set; }
        public double TotalWasteArea { get; set; }
        public double WastePercentage { get; set; }

        public List<CutPlanSheet> Sheets { get; set; } = [];
    }
}
